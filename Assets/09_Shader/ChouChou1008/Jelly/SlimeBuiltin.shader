// Gravity Collect - glossy gradient slime body for the Built-in Render Pipeline.
// Built-in render pipeline only (no URP needed). Tuned for mobile: one directional light, no add passes.
Shader "GravityCollect/Slime Body (Built-in)"
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
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        CGPROGRAM
        // mobile-friendly: main light only, no extra per-pixel light passes, no lightmap/deferred variants
        #pragma surface surf SlimeWrap vertex:vert addshadow noforwardadd nolightmap nodynlightmap nodirlightmap nometa exclude_path:deferred exclude_path:prepass
        #pragma target 3.0

        fixed4 _TopColor;
        fixed4 _BottomColor;
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
        float _Tip;
        float _Lean;
        sampler2D _PatternTex;
        fixed4 _PatternColor;
        float _PatternScale;
        float4 _PatternScroll;
        half _SelfGlow;

        half3 SlimePattern(sampler2D tex, float3 p, float3 n, float scale, float2 scroll, float time)
        {
            float3 w = abs(n); w = pow(w, 4.0); w /= (w.x + w.y + w.z + 1e-4);
            float2 o = scroll * time;
            half3 a = tex2D(tex, p.xy * scale + o).rgb;
            half3 b = tex2D(tex, p.zy * scale + o).rgb;
            half3 c = tex2D(tex, p.xz * scale + o).rgb;
            return a * w.z + b * w.x + c * w.y;
        }

        float3 SlimeDeform(float3 p, float tip, float lean)
        {
            float t = saturate(p.y / 0.78);
            float t2 = t * t;
            p.xz *= 1.0 - tip * 0.5 * pow(t, 2.2);   // narrow the top ...
            p.y += tip * 0.42 * t2 * t;          // ... and pull it up into a drop
            p.z += lean * 0.3 * t2;              // lean forward (+Z) / back
            return p;
        }

        struct Input
        {
            float3 viewDir;
            float heightOS;
            float3 normalVS;
            float3 posOS;
            float3 normalOS;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.heightOS = v.vertex.y;
            o.posOS = v.vertex.xyz;
            o.normalOS = v.normal;
            v.vertex.xyz = SlimeDeform(v.vertex.xyz, _Tip, _Lean);
            o.normalVS = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
        }

        half4 LightingSlimeWrap(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half wrap = saturate((dot(s.Normal, lightDir) + _Wrap) / (1.0 + _Wrap));
            half3 h = normalize(lightDir + viewDir);
            half spec = pow(saturate(dot(s.Normal, h)), _Gloss) * _SpecStrength;
            half fres = pow(1.0 - saturate(dot(s.Normal, viewDir)), 2.4);
            half back = pow(saturate(dot(-s.Normal, lightDir) * 0.5 + 0.5), 2.0) * fres * 0.25;
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (wrap * lerp(0.6, 1.0, atten) * 0.7 + back) + _LightColor0.rgb * spec * atten;
            c.a = 1;
            return c;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float g = smoothstep(_GradientBase, _GradientHeight, IN.heightOS);
            half3 baseCol = lerp(_BottomColor.rgb, _TopColor.rgb, g);
            half lum = dot(baseCol, half3(0.3, 0.59, 0.11));
            baseCol = lerp(baseCol, lum * 0.45 * half3(1.1, 1.0, 0.8), _Gray) * lerp(0.82, 1.0, smoothstep(0.0, 0.12, IN.heightOS));
            half fres = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), 2.4);
            half hl = smoothstep(_HighlightSize, _HighlightSize + 0.013, dot(normalize(IN.normalVS), normalize(_HighlightDir.xyz)));
            o.Albedo = lerp(baseCol, 0, _Flash);
            half3 pat = SlimePattern(_PatternTex, IN.posOS, normalize(IN.normalOS), _PatternScale, _PatternScroll.xy, _Time.y) * _PatternColor.rgb * (1 - _Gray);
            o.Emission = lerp(lerp(_TopColor.rgb, lum.xxx, _Gray) * fres * _EdgeGlow + hl * _HighlightStrength + baseCol * (0.2 + _SelfGlow) + pat, 1, _Flash);
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
