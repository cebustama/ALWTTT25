Shader "ALWTTT/SpriteOutlineURP"
{
    // URP 2D-compatible sprite shader with a pixel-space alpha-neighbor outline.
    // Intended for M1.7 character hover highlight.
    //
    // Activation: set _OutlineWidth > 0 at runtime via MaterialPropertyBlock
    //   (see SpriteOutlineController.cs). Width is in texels of _MainTex.
    //
    // How it works:
    //   For each fragment, if the sampled pixel is transparent (alpha below
    //   _AlphaThreshold) but at least one of its 8 diagonal/cardinal neighbors
    //   (at _OutlineWidth texel offset) is opaque, output _OutlineColor.
    //   Otherwise output the sprite color as usual.
    //
    // Notes:
    //   - Tags include "RenderPipeline" = "UniversalPipeline" and "LightMode" =
    //     "Universal2D" so the URP 2D Renderer picks this up correctly.
    //   - Uses standard sprite blending (SrcAlpha OneMinusSrcAlpha), not
    //     premultiplied. Works with default sprite import settings.
    //   - For pixel-art sprites with hard alpha edges, _AlphaThreshold = 0.1
    //     is fine. For anti-aliased sprites, raise to 0.4–0.5 to avoid halos.

    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1, 0.85, 0.2, 1)
        _OutlineWidth ("Outline Width (texels)", Range(0, 8)) = 0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1

        // Sprite batching / pivot bits — kept for SpriteRenderer compatibility.
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _EnableExternalAlpha ("EnableExternalAlpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4  color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                half4  _OutlineColor;
                float  _OutlineWidth;
                float  _AlphaThreshold;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.color = IN.color * _Color;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // Early out if outline is disabled.
                if (_OutlineWidth < 0.001)
                    return c;

                // Offset is in UV space — _MainTex_TexelSize.xy = (1/w, 1/h)
                float2 o = _MainTex_TexelSize.xy * _OutlineWidth;

                // 8-neighbor alpha sampling.
                half aN  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( 0,     o.y)).a;
                half aS  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( 0,    -o.y)).a;
                half aE  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x,  0 )).a;
                half aW  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x,  0 )).a;
                half aNE = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x,  o.y)).a;
                half aSE = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x, -o.y)).a;
                half aNW = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x,  o.y)).a;
                half aSW = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x, -o.y)).a;

                half maxA = max(max(max(aN, aS), max(aE, aW)),
                                max(max(aNE, aSE), max(aNW, aSW)));

                // Current pixel transparent, any neighbor opaque → outline pixel.
                half isOutline = step(c.a, _AlphaThreshold) * step(_AlphaThreshold, maxA);

                // Replace transparent-border pixel with outline color.
                c = lerp(c, _OutlineColor, isOutline);

                return c;
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}
