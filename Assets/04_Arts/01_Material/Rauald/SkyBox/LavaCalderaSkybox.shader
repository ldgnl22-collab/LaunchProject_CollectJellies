Shader "Rauald/Skybox/Lava Caldera"
{
    Properties
    {
        [NoScaleOffset] _Tex ("360 HDR Panorama", 2D) = "grey" {}
        _Exposure ("Exposure", Range(0, 4)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
        [Toggle] _Animate ("Animate Lava", Float) = 0
        _FlowSpeed ("Flow Speed (cycles/sec)", Range(0, 0.5)) = 0.06
        _FlowDistance ("Downward Flow Distance", Range(0, 0.05)) = 0.018
        _MotionStrength ("Lava Motion Strength", Range(0, 1)) = 0.8
        _RippleAmount ("Lake Ripple Amount", Range(0, 0.01)) = 0.0025
        _PulseStrength ("Lava Glow Variation", Range(0, 0.3)) = 0.12
        _SeamBlend ("Panorama Seam Feather", Range(0, 0.05)) = 0.012
        [HDR] _ZenithColor ("Zenith Blend Color", Color) = (0.1, 0.3, 0.6, 1)
        [HDR] _NadirColor ("Nadir Blend Color", Color) = (1, 0.12, 0.005, 1)
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _Tex;
            float4 _Tex_HDR;
            float _Exposure, _Rotation, _Animate;
            float _FlowSpeed, _FlowDistance, _MotionStrength;
            float _RippleAmount, _PulseStrength, _SeamBlend;
            float4 _ZenithColor, _NadirColor;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.position = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }

            float3 ReadPanorama(float2 uv)
            {
                uv.x = frac(uv.x);
                uv.y = saturate(uv.y);
                float3 color = DecodeHDR(tex2D(_Tex, uv), _Tex_HDR);
                // Feather only the longitude boundary, without rotating the scene.
                float distanceToSeam = min(uv.x, 1.0 - uv.x);
                float seamWeight = 0.5 * (1.0 - smoothstep(0.0, max(_SeamBlend, 0.00001), distanceToSeam));
                float3 otherEdge = DecodeHDR(tex2D(_Tex, float2(1.0 - uv.x, uv.y)), _Tex_HDR);
                color = lerp(color, otherEdge, seamWeight);
                // All longitudes meet at each pole. Keep the final few degrees continuous.
                color = lerp(_NadirColor.rgb, color, smoothstep(0.0, 0.02, uv.y));
                color = lerp(_ZenithColor.rgb, color, smoothstep(0.0, 0.02, 1.0 - uv.y));
                return color;
            }

            float LavaMask(float3 color)
            {
                float warm = (color.r - color.b) / max(color.r, 0.001);
                float bright = smoothstep(0.08, 0.35, color.r);
                return smoothstep(0.3, 0.65, warm) * bright
                    * smoothstep(0.04, 0.18, color.r - color.g * 0.8);
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 direction = normalize(i.direction);
                float angle = _Rotation * UNITY_PI / 180.0;
                float sa = sin(angle), ca = cos(angle);
                direction.xz = float2(ca * direction.x - sa * direction.z,
                    sa * direction.x + ca * direction.z);
                float2 uv = float2(0.5 + atan2(direction.x, direction.z) / UNITY_TWO_PI,
                    0.5 + asin(clamp(direction.y, -1.0, 1.0)) / UNITY_PI);
                float3 color = ReadPanorama(uv);

                // The static material never uses time. Blue sky and dark basalt have zero mask.
                if (_Animate > 0.5)
                {
                    float mask = LavaMask(color);
                    float t = _Time.y;
                    float lake = 1.0 - smoothstep(0.38, 0.48, uv.y);
                    float poleFade = smoothstep(0.0, 0.03, uv.y);
                    // 3D direction-based noise is continuous across the panorama seam.
                    float n0 = sin(dot(direction, float3(19.0, 11.0, 17.0)) + t * 1.2);
                    float n1 = cos(dot(direction, float3(13.0, 23.0, -9.0)) - t * 0.9);
                    float2 ripple = float2(n0, n1) * _RippleAmount * lake * mask * poleFade;
                    float2 motion = lerp(float2(0.0, _FlowDistance),
                        float2(_FlowDistance * 0.35, _FlowDistance * 0.2), lake) * poleFade;
                    float phase0 = frac(t * _FlowSpeed);
                    float phase1 = frac(t * _FlowSpeed + 0.5);
                    float blend = abs(phase0 * 2.0 - 1.0);
                    // Crossfade two flow phases so neither reset creates a visible jump.
                    float3 a = ReadPanorama(uv + motion * (phase0 - 0.5) + ripple);
                    float3 b = ReadPanorama(uv + motion * (phase1 - 0.5) + ripple);
                    float3 flowing = lerp(a, b, blend);
                    float movingMask = mask * LavaMask(flowing) * _MotionStrength;
                    color = lerp(color, flowing, movingMask);
                    float pulse = sin(t * 1.7 + dot(direction, float3(11.0, 7.0, 13.0)))
                        * cos(t * 0.8 + direction.y * 15.0);
                    color *= 1.0 + mask * _PulseStrength * pulse;
                }

                return float4(max(color, 0.0) * _Exposure, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
