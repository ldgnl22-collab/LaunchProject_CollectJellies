// Gravity Collect - glossy gradient slime body for the Universal Render Pipeline (URP).
// Same properties and look as "GravityCollect/Slime Body (Built-in)", so a material can switch between the two
// without losing its colours: bottom-to-top gradient, soft wrap light, glowing edge, glossy spec, cartoon highlight,
// triplanar pattern (galaxy / flame / circuit ...), self glow.
// _Flash fades to white, _Gray drains the colour, _Tip/_Lean bend the shape (driven by SlimeMotion / SlimeCharacter).
// Shadow and depth passes use the same bend.
Shader "GravityCollect/Slime Body (URP)"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1.0, 0.82, 0.89, 1)
        _BottomColor ("Bottom Color", Color) = (1.0, 0.36, 0.62, 1)
        _GradientBase ("Gradient Base", Float) = 0.02
        _GradientHeight ("Gradient Height", Float) = 0.78
        _Wrap ("Light Wrap", Range(0, 1)) = 0.6
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 0.55
        _Gloss ("Gloss", Range(8, 256)) = 90
        _SpecStrength ("Specular", Range(0, 2)) = 0.9
        _HighlightDir ("Highlight Direction (view space)", Vector) = (-0.45, 0.62, 0.64, 0)
        _HighlightSize ("Highlight Size", Range(0.8, 0.999)) = 0.955
        _HighlightStrength ("Highlight Strength", Range(0, 1)) = 0.8
        _Flash ("Flash", Range(0, 1)) = 0
        _Gray ("Gray (hurt/death)", Range(0, 1)) = 0
        _PatternTex ("Pattern (additive, triplanar)", 2D) = "black" {}
        _PatternColor ("Pattern Color", Color) = (1, 1, 1, 1)
        _PatternScale ("Pattern Scale", Float) = 1.5
        _PatternScroll ("Pattern Scroll (xy)", Vector) = (0, 0, 0, 0)
        _SelfGlow ("Self Glow", Range(0, 2)) = 0
        _Tip ("Drop Tip", Range(-1, 1)) = 0
        _Lean ("Lean", Range(-1, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _TopColor;
            half4 _BottomColor;
            float _GradientBase;
            float _GradientHeight;
            half _Wrap;
            half _EdgeGlow;
            half _Gloss;
            half _SpecStrength;
            float4 _HighlightDir;
            half _HighlightSize;
            half _HighlightStrength;
            half _Flash;
            half _Gray;
            float4 _PatternTex_ST;
            half4 _PatternColor;
            float _PatternScale;
            float4 _PatternScroll;
            half _SelfGlow;
            float _Tip;
            float _Lean;
        CBUFFER_END

        TEXTURE2D(_PatternTex);
        SAMPLER(sampler_PatternTex);

        // Same bend as the Built-in body and the face decal.
        float3 SlimeDeform(float3 p)
        {
            float t = saturate(p.y / 0.78);
            float t2 = t * t;
            p.xz *= 1.0 - _Tip * 0.5 * pow(t, 2.2);   // narrow the top ...
            p.y += _Tip * 0.42 * t2 * t;              // ... and pull it up into a drop
            p.z += _Lean * 0.3 * t2;                  // lean forward (+Z) / back
            return p;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float heightOS : TEXCOORD2;
                float3 posOS : TEXCOORD3;
                float3 normalOS : TEXCOORD4;
                float fogFactor : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half3 SlimePattern(float3 p, float3 n)
            {
                float3 w = abs(n);
                w = w * w * w * w;
                w /= (w.x + w.y + w.z + 1e-4);
                float2 o = _PatternScroll.xy * _Time.y;
                half3 a = SAMPLE_TEXTURE2D(_PatternTex, sampler_PatternTex, p.xy * _PatternScale + o).rgb;
                half3 b = SAMPLE_TEXTURE2D(_PatternTex, sampler_PatternTex, p.zy * _PatternScale + o).rgb;
                half3 c = SAMPLE_TEXTURE2D(_PatternTex, sampler_PatternTex, p.xz * _PatternScale + o).rgb;
                return a * w.z + b * w.x + c * w.y;
            }

            Varyings vert(Attributes i)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);

                VertexPositionInputs vp = GetVertexPositionInputs(SlimeDeform(i.positionOS.xyz));
                VertexNormalInputs vn = GetVertexNormalInputs(i.normalOS);

                o.positionCS = vp.positionCS;
                o.positionWS = vp.positionWS;
                o.normalWS = vn.normalWS;
                o.heightOS = i.positionOS.y;
                o.posOS = i.positionOS.xyz;
                o.normalOS = i.normalOS;
                o.fogFactor = ComputeFogFactor(vp.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 n = normalize(i.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float3 l = light.direction;
                half atten = light.shadowAttenuation * light.distanceAttenuation;

                // colour gradient (+ gray when hurt / dead)
                float g = smoothstep(_GradientBase, _GradientHeight, i.heightOS);
                half3 baseCol = lerp(_BottomColor.rgb, _TopColor.rgb, g);
                half lum = dot(baseCol, half3(0.3, 0.59, 0.11));
                baseCol = lerp(baseCol, lum * 0.45 * half3(1.1, 1.0, 0.8), _Gray);

                // soft wrap light + ambient
                half wrap = saturate((dot(n, l) + _Wrap) / (1.0 + _Wrap));
                half fres = pow(1.0 - saturate(dot(n, v)), 2.4);
                half3 col = baseCol * (light.color * wrap * lerp(0.6, 1.0, atten) * 0.7 + SampleSH(n) * 0.45);

                col += lerp(_TopColor.rgb, lum.xxx, _Gray) * fres * _EdgeGlow;                 // translucent edge
                col += baseCol * 0.25 * pow(saturate(dot(-n, l) * 0.5 + 0.5), 2.0) * fres;      // light through the back

                float3 h = normalize(l + v);
                col += light.color * pow(saturate(dot(n, h)), _Gloss) * _SpecStrength * atten;  // glossy spec

                float3 nV = normalize(mul((float3x3)UNITY_MATRIX_V, n));                        // cartoon highlight
                half hl = smoothstep(_HighlightSize, _HighlightSize + 0.013, dot(nV, normalize(_HighlightDir.xyz)));
                col += hl * _HighlightStrength;

                col *= lerp(0.82, 1.0, smoothstep(0.0, 0.12, i.heightOS));                       // darker where it meets the floor
                col += baseCol * _SelfGlow;                                                      // glowing jellies (fire, cyber ...)
                col += SlimePattern(i.posOS, normalize(i.normalOS)) * _PatternColor.rgb * (1.0 - _Gray);

                col = lerp(col, 1.0, _Flash);                                                    // hit flash
                col = MixFog(col, i.fogFactor);
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct AttributesS
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsS
            {
                float4 positionCS : SV_POSITION;
            };

            VaryingsS vertShadow(AttributesS i)
            {
                VaryingsS o;
                UNITY_SETUP_INSTANCE_ID(i);

                float3 posWS = TransformObjectToWorld(SlimeDeform(i.positionOS.xyz));
                float3 nWS = TransformObjectToWorldNormal(i.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDir = normalize(_LightPosition - posWS);
            #else
                float3 lightDir = _LightDirection;
            #endif
                float4 cs = TransformWorldToHClip(ApplyShadowBias(posWS, nWS, lightDir));
            #if UNITY_REVERSED_Z
                cs.z = min(cs.z, UNITY_NEAR_CLIP_VALUE);
            #else
                cs.z = max(cs.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                o.positionCS = cs;
                return o;
            }

            half4 fragShadow(VaryingsS i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex vertDepth
            #pragma fragment fragDepth
            #pragma multi_compile_instancing

            struct AttributesD
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsD
            {
                float4 positionCS : SV_POSITION;
            };

            VaryingsD vertDepth(AttributesD i)
            {
                VaryingsD o;
                UNITY_SETUP_INSTANCE_ID(i);
                o.positionCS = TransformObjectToHClip(SlimeDeform(i.positionOS.xyz));
                return o;
            }

            half4 fragDepth(VaryingsD i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Simple Lit"
}
