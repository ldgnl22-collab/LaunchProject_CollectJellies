// Gravity Collect - slime face decal for the Built-in render pipeline (simple unlit pass, cheap on mobile; no
// pass, drawn by the forward renderer). Picks one cell of the 4x5 face atlas with _FaceIndex.
Shader "GravityCollect/Slime Face"
{
    Properties
    {
        _MainTex ("Face Atlas", 2D) = "white" {}
        _FaceIndex ("Face Index", Float) = 0
        _Columns ("Columns", Float) = 4
        _Rows ("Rows", Float) = 5
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _InkColor ("Eye/Line Color (for dark jellies use white)", Color) = (0.165, 0.094, 0.188, 1)
        _Flash ("Flash", Range(0, 1)) = 0
        _FaceFade ("Fade Out", Range(0, 1)) = 0
        _Tip ("Drop Tip", Range(-1, 1)) = 0
        _Lean ("Lean", Range(-1, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Geometry+10" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset -1, -1
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Columns;
            float _Rows;
            fixed4 _Tint;
            fixed4 _InkColor;
            float _FaceFade;
            float _Tip;
            float _Lean;

            float3 SlimeDeform(float3 p, float tip, float lean)
            {
                float t = saturate(p.y / 0.78);
                float t2 = t * t;
                p.xz *= 1.0 - tip * 0.5 * pow(t, 2.2);   // narrow the top ...
                p.y += tip * 0.42 * t2 * t;              // ... and pull it up into a drop
                p.z += lean * 0.3 * t2;                  // lean forward (+Z) / back
                return p;
            }
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _FaceIndex)
                UNITY_DEFINE_INSTANCED_PROP(float, _Flash)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float idx = floor(UNITY_ACCESS_INSTANCED_PROP(Props, _FaceIndex) + 0.5);
                float col = fmod(idx, _Columns);
                float row = floor(idx / _Columns);                      // row 0 is the top of the texture
                o.uv = float2((saturate(v.uv.x) + col) / _Columns, (saturate(v.uv.y) + (_Rows - 1 - row)) / _Rows);
                o.pos = UnityObjectToClipPos(float4(SlimeDeform(v.vertex.xyz, _Tip, _Lean), 1.0));   // same bend as the body
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 c = tex2D(_MainTex, i.uv);
                // recolour the dark outline/eye ink (atlas ink = #2A1830) so faces stay readable on dark jellies
                half3 ink = half3(0.165, 0.094, 0.188);
                half k = smoothstep(0.06, 0.16, distance(c.rgb, ink));
                half kw = smoothstep(0.06, 0.2, distance(c.rgb, half3(1, 1, 1)));
                half inv = step(0.5, dot(_InkColor.rgb, half3(0.333, 0.333, 0.333)));   // light ink -> eye sparkles turn dark
                half3 src = c.rgb;
                c.rgb = lerp(_InkColor.rgb, src, k);
                c.rgb = lerp(lerp(c.rgb, ink, inv), c.rgb, kw);
                c *= _Tint;
                c.a *= (1 - UNITY_ACCESS_INSTANCED_PROP(Props, _Flash)) * (1 - _FaceFade);
                return c;
            }
            ENDCG
        }
    }
}
