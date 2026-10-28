// 커비풍 3D 땅 셰이더 — 모바일 최적화판 (Built-in / URP 공용)
//
// 최적화 포인트
// · 면 방향(윗면/옆면)을 버텍스에서 한 번만 판정 → 픽셀당 텍스처 2번만 읽음 (기존 트라이플래너 6번)
// · 윗면 높이를 메시(UV3)에 구워 넣어서 MaterialPropertyBlock 불필요 → 한 번에 그리기(배칭) 가능
// · half 정밀도, 분기 없는 조명
// · 2단계 품질: Shader.globalMaximumLOD ≥ 300 → High(노멀맵·광택),  < 300 → Low(조명을 버텍스에서 계산)
//   PuffyMobileBoot 가 기기 성능에 맞게 자동 선택
Shader "PuffyWorld/Ground"
{
    Properties
    {
        [Header(Top)]
        _TopTex ("윗면 텍스처", 2D) = "white" {}
        [Normal] _TopNormal ("윗면 노멀맵", 2D) = "bump" {}
        _TopTint ("윗면 색", Color) = (1,1,1,1)
        _TopScale ("윗면 타일 (1/m)", Float) = 0.25
        _TopGloss ("윗면 광택", Range(0,1)) = 0.25

        [Header(Side)]
        _SideTex ("옆면 텍스처", 2D) = "white" {}
        [Normal] _SideNormal ("옆면 노멀맵", 2D) = "bump" {}
        _SideTint ("옆면 색", Color) = (1,1,1,1)
        _SideScale ("옆면 타일 (1/m)", Float) = 0.25
        _NormalStrength ("노멀 세기", Range(0,2)) = 1

        [Header(Lip)]
        _LipColor ("테두리 색", Color) = (0.47,0.8,0.33,1)
        _LipHeight ("테두리 두께 (m)", Float) = 0.3
        _LipWave ("물결 깊이 (m)", Float) = 0.12
        _LipFreq ("물결 개수 (1/m)", Float) = 2
        _SurfaceY ("윗면 높이 (구운 값 없을 때)", Float) = 0

        [Header(Lighting)]
        _LightDir ("빛 방향 (Light Sync 없을 때)", Vector) = (0.4, 0.85, -0.35, 0)
        _ShadowColor ("그늘 색", Color) = (0.62,0.58,0.85,1)
        _SkyAmbient ("하늘 환경광", Color) = (0.95,0.97,1.05,1)
        _GroundAmbient ("땅 환경광", Color) = (0.85,0.8,0.9,1)
        _ToonSoftness ("명암 부드러움", Range(0.01,1)) = 0.45
        _RimColor ("림 라이트 색", Color) = (1,1,1,1)
        _RimStrength ("림 라이트", Range(0,1)) = 0.35
        _Brightness ("밝기", Range(0.5,2)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _TopTex, _SideTex, _TopNormal, _SideNormal;
    half4 _TopTint, _SideTint, _LipColor, _ShadowColor, _SkyAmbient, _GroundAmbient, _RimColor;
    float _TopScale, _SideScale, _LipHeight, _LipWave, _LipFreq, _SurfaceY;
    half _TopGloss, _NormalStrength, _ToonSoftness, _RimStrength, _Brightness;
    float4 _LightDir;
    float4 _PuffyLightDir;      // PuffyLightSync
    half4 _PuffyLightColor;

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 uv3 : TEXCOORD2;     // x = 윗면 높이, y = 1 이면 구운 값 사용
    };

    // 면 판정 + UV + 접선 공간 (버텍스에서 1번)
    void FaceSetup(float3 wpos, float3 n, out float4 uv, out half3 t, out half3 b)
    {
        float3 an = abs(n);
        float isY = step(max(an.x, an.z), an.y);
        float isTop = isY * step(0, n.y);
        float xDom = step(an.z, an.x);
        float sx = n.x >= 0 ? 1 : -1;
        float sz = n.z >= 0 ? 1 : -1;
        float2 uvX = float2(wpos.z * sx, wpos.y);
        float2 uvZ = float2(-wpos.x * sz, wpos.y);
        float2 side = lerp(uvZ, uvX, xDom);
        float2 uvSide = lerp(side, wpos.xz, isY) * _SideScale;
        float2 uvTop = wpos.xz * _TopScale;
        uv.xy = lerp(uvSide, uvTop, isTop);
        uv.z = isTop;
        uv.w = lerp(wpos.x, wpos.z, xDom);          // 테두리 물결용 가로 좌표
        float3 tX = float3(0, 0, sx), tZ = float3(-sz, 0, 0);
        t = (half3)lerp(lerp(tZ, tX, xDom), float3(1, 0, 0), isY);
        b = (half3)lerp(float3(0, 1, 0), float3(0, 0, 1), isY);
    }

    float SurfaceOf(float2 uv3) { return uv3.y > 0.5 ? uv3.x : _SurfaceY; }

    half3 LightDirW() { return (half3)(dot(_PuffyLightDir.xyz, _PuffyLightDir.xyz) > 0.0001 ? normalize(_PuffyLightDir.xyz) : normalize(_LightDir.xyz)); }
    half3 LightCol()  { return _PuffyLightColor.a > 0 ? _PuffyLightColor.rgb : half3(1, 1, 1); }

    // 테두리: 0..1 (옆면 윗부분), 그리고 테두리 바로 아래 그림자
    void Lip(float wy, float surf, float h, half isTop, out half lip, out half under, out half roll)
    {
        float edge = surf - _LipHeight - _LipWave * abs(sin(h * _LipFreq * 3.14159));
        float d = wy - edge;
        lip = (half)(saturate(d * 60 + 0.5) * (1 - isTop));
        under = (half)(saturate(1 + d * 4) * (1 - lip) * (1 - isTop) * step(d, 0));
        roll = (half)(saturate(1 - d / max(_LipHeight * 0.6, 0.01)) * lip);
    }
    ENDCG

    // ───────────────────────── High ─────────────────────────
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv  : TEXCOORD0;      // xy uv, z isTop, w 가로좌표
                float4 lp  : TEXCOORD1;      // x 월드Y, y 윗면높이
                half3 n    : TEXCOORD2;
                half3 t    : TEXCOORD3;
                half3 b    : TEXCOORD4;
                float3 wpos : TEXCOORD5;
                UNITY_FOG_COORDS(6)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 n = UnityObjectToWorldNormal(v.normal);
                FaceSetup(wpos, n, o.uv, o.t, o.b);
                o.n = (half3)n;
                o.wpos = wpos;
                o.lp = float4(wpos.y, SurfaceOf(v.uv3), 0, 0);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half isTop = (half)i.uv.z;
                // 텍스처 2번: 색 + 노멀 (윗면/옆면 중 하나만)
                float2 dx = ddx(i.uv.xy), dy = ddy(i.uv.xy);
                half4 col; half3 tn;
                UNITY_BRANCH if (isTop > 0.5h)
                {
                    col = tex2Dgrad(_TopTex, i.uv.xy, dx, dy) * _TopTint;
                    tn = UnpackNormal(tex2Dgrad(_TopNormal, i.uv.xy, dx, dy));
                }
                else
                {
                    col = tex2Dgrad(_SideTex, i.uv.xy, dx, dy) * _SideTint;
                    tn = UnpackNormal(tex2Dgrad(_SideNormal, i.uv.xy, dx, dy));
                }
                tn.xy *= _NormalStrength;
                half3 N = normalize(i.t * tn.x + i.b * tn.y + normalize(i.n) * tn.z);

                half lip, under, roll;
                Lip(i.lp.x, i.lp.y, i.uv.w, isTop, lip, under, roll);
                N = normalize(N - half3(0, 1.2h, 0) * roll * 0.7h);
                col.rgb = lerp(col.rgb, _LipColor.rgb, lip);

                half3 L = LightDirW();
                half3 lc = LightCol();
                half3 V = (half3)normalize(_WorldSpaceCameraPos - i.wpos);
                half ndl = dot(N, L);
                half toon = smoothstep(-_ToonSoftness * 0.5h, _ToonSoftness, ndl);
                half3 amb = lerp(_GroundAmbient.rgb, _SkyAmbient.rgb, N.y * 0.5h + 0.5h);
                half3 lit = lerp(_ShadowColor.rgb, lc, toon) * amb;
                half nh = saturate(dot(N, normalize(L + V)));
                half nh2 = nh * nh; half nh4 = nh2 * nh2; half nh8 = nh4 * nh4; half nh32 = nh8 * nh8; nh32 *= nh32;
                half spec = nh32 * _TopGloss * toon * (isTop + lip * 0.6h);
                half f = 1 - saturate(dot(N, V));
                half rim = f * f * f * _RimStrength * (0.4h + 0.6h * toon);

                half3 c = col.rgb * lit + spec * lc + rim * _RimColor.rgb;
                c *= (1 - under * 0.35h) * _Brightness;
                half4 o = half4(c, 1);
                UNITY_APPLY_FOG(i.fogCoord, o);
                return o;
            }
            ENDCG
        }
    }

    // ───────────────────────── Low (저사양) ─────────────────────────
    // 노멀맵/광택 없음, 조명은 버텍스에서 계산, 텍스처 1번
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv  : TEXCOORD0;
                float2 lp  : TEXCOORD1;
                half3 lit  : TEXCOORD2;
                half3 rim  : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 n = (half3)UnityObjectToWorldNormal(v.normal);
                half3 t, b;
                FaceSetup(wpos, n, o.uv, t, b);
                o.lp = float2(wpos.y, SurfaceOf(v.uv3));
                half3 L = LightDirW();
                half toon = smoothstep(-_ToonSoftness * 0.5h, _ToonSoftness, dot(n, L));
                half3 amb = lerp(_GroundAmbient.rgb, _SkyAmbient.rgb, n.y * 0.5h + 0.5h);
                o.lit = lerp(_ShadowColor.rgb, LightCol(), toon) * amb * _Brightness;
                half3 V = (half3)normalize(_WorldSpaceCameraPos - wpos);
                half f = 1 - saturate(dot(n, V));
                o.rim = f * f * f * _RimStrength * _RimColor.rgb;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half isTop = (half)i.uv.z;
                float2 dx = ddx(i.uv.xy), dy = ddy(i.uv.xy);
                half4 col;
                UNITY_BRANCH if (isTop > 0.5h) col = tex2Dgrad(_TopTex, i.uv.xy, dx, dy) * _TopTint;
                else                          col = tex2Dgrad(_SideTex, i.uv.xy, dx, dy) * _SideTint;
                half lip, under, roll;
                Lip(i.lp.x, i.lp.y, i.uv.w, isTop, lip, under, roll);
                col.rgb = lerp(col.rgb, _LipColor.rgb * (1 - roll * 0.25h), lip);
                half3 c = col.rgb * i.lit + i.rim;
                c *= 1 - under * 0.35h;
                half4 o = half4(c, 1);
                UNITY_APPLY_FOG(i.fogCoord, o);
                return o;
            }
            ENDCG
        }
    }
    Fallback "Mobile/Diffuse"
}
