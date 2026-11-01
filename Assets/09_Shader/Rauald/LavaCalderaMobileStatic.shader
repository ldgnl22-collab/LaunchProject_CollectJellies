Shader "Rauald/Skybox/Lava Caldera Mobile Static"
{
    Properties
    {
        [NoScaleOffset] _Tex ("Mobile Cubemap (LDR)", Cube) = "grey" {}
        _Exposure ("Brightness", Range(0, 2)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
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
            #pragma target 2.0
            #include "UnityCG.cginc"

            samplerCUBE _Tex;
            half _Exposure;
            float _Rotation;

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
                float angle = _Rotation * UNITY_PI / 180.0;
                float sa = sin(angle), ca = cos(angle);
                // Rotate only the cube lookup direction; this work runs per vertex.
                o.direction = float3(ca * v.vertex.x - sa * v.vertex.z,
                    v.vertex.y, sa * v.vertex.x + ca * v.vertex.z);
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                // One cubemap lookup. No time, HDR decoding, masks, or per-pixel trig.
                return half4(texCUBE(_Tex, i.direction).rgb * _Exposure, 1.0h);
            }
            ENDCG
        }
    }
    Fallback Off
}
