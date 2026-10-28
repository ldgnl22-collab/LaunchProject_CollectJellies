// 3D 소품 / 테두리 쿠션용 클레이 셰이더 — 모바일 최적화판 (Built-in / URP 공용)
// 버텍스 컬러 RGB = 색, A < 1 = 스스로 빛나는 부분 (별, 창문, 크리스탈)
// · 텍스처 없음 (메모리 0, 대역폭 0)
// · GPU 인스턴싱 지원 → 같은 소품을 여러 개 놓아도 드로우콜 1
// · High: 픽셀 조명 / Low(globalMaximumLOD < 300): 버텍스 조명
Shader "PuffyWorld/Clay"
{
    Properties
    {
        _Color ("색 곱하기", Color) = (1,1,1,1)
        _LightDir ("빛 방향 (Light Sync 없을 때)", Vector) = (0.4, 0.85, -0.35, 0)
        _ShadowColor ("그늘 색", Color) = (0.62,0.58,0.85,1)
        _SkyAmbient ("하늘 환경광", Color) = (0.95,0.97,1.05,1)
        _GroundAmbient ("땅 환경광", Color) = (0.85,0.8,0.9,1)
        _ToonSoftness ("명암 부드러움", Range(0.01,1)) = 0.5
        _Gloss ("광택", Range(0,1)) = 0.3
        _RimColor ("림 라이트 색", Color) = (1,1,1,1)
        _RimStrength ("림 라이트", Range(0,1)) = 0.4
        _Emission ("발광 세기", Range(0,4)) = 1.6
        _Brightness ("밝기", Range(0.5,2)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    half4 _Color, _ShadowColor, _SkyAmbient, _GroundAmbient, _RimColor;
    float4 _LightDir, _PuffyLightDir;
    half4 _PuffyLightColor;
    half _ToonSoftness, _Gloss, _RimStrength, _Emission, _Brightness;

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        half4 color : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    half3 LightDirW() { return (half3)(dot(_PuffyLightDir.xyz, _PuffyLightDir.xyz) > 0.0001 ? normalize(_PuffyLightDir.xyz) : normalize(_LightDir.xyz)); }
    half3 LightCol()  { return _PuffyLightColor.a > 0 ? _PuffyLightColor.rgb : half3(1, 1, 1); }

    half3 Shade(half3 albedo, half glow, half3 N, half3 V, half3 L, half3 lc, half doSpec)
    {
        half toon = smoothstep(-_ToonSoftness * 0.5h, _ToonSoftness, dot(N, L));
        half3 amb = lerp(_GroundAmbient.rgb, _SkyAmbient.rgb, N.y * 0.5h + 0.5h);
        half3 lit = lerp(_ShadowColor.rgb, lc, toon) * amb;
        half nh = saturate(dot(N, normalize(L + V)));
        half nh2 = nh * nh; half nh4 = nh2 * nh2; half nh8 = nh4 * nh4; half nh32 = nh8 * nh8; nh32 *= nh32;
        half spec = nh32 * _Gloss * toon * doSpec;
        half f = 1 - saturate(dot(N, V));
        half rim = f * f * f * _RimStrength * (0.4h + 0.6h * toon);
        half3 c = albedo * lit + spec * lc + rim * _RimColor.rgb;
        c = lerp(c, albedo * _Emission + rim, glow);
        return c * _Brightness;
    }
    ENDCG

    // ───── High: 픽셀 조명 ─────
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 wpos : TEXCOORD0;
                half3 n : TEXCOORD1;
                half4 col : COLOR;
                UNITY_FOG_COORDS(2)
            };

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.n = (half3)UnityObjectToWorldNormal(v.normal);
                o.col = v.color * _Color;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 N = normalize(i.n);
                half3 V = (half3)normalize(_WorldSpaceCameraPos - i.wpos);
                half3 c = Shade(i.col.rgb, 1 - i.col.a, N, V, LightDirW(), LightCol(), 1);
                half4 o = half4(c, 1);
                UNITY_APPLY_FOG(i.fogCoord, o);
                return o;
            }
            ENDCG
        }
    }

    // ───── Low: 버텍스 조명 ─────
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct v2f
            {
                float4 pos : SV_POSITION;
                half4 col : COLOR;
                UNITY_FOG_COORDS(0)
            };

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 N = (half3)UnityObjectToWorldNormal(v.normal);
                half3 V = (half3)normalize(_WorldSpaceCameraPos - wpos);
                half4 vc = v.color * _Color;
                o.col = half4(Shade(vc.rgb, 1 - vc.a, N, V, LightDirW(), LightCol(), 0), 1);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 o = i.col;
                UNITY_APPLY_FOG(i.fogCoord, o);
                return o;
            }
            ENDCG
        }
    }
    Fallback "Mobile/VertexLit"
}
