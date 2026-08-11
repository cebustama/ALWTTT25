// [PRES-1 / D-PRES1-1=B+ / D-PRES1-5=A] Invert-blend wave for Psychic Wave v4.
//
// ONE region, TWO phases. The inverted area is the annulus between
// _InnerRadius and _OuterRadius (soft edges, aspect-corrected):
//   Phase 1 (cover):   _InnerRadius = 0, _OuterRadius grows -> inverted DISC
//                      expands from the anchor until the screen is covered.
//   Phase 2 (uncover): _InnerRadius grows -> a hole expands from the same
//                      anchor, undoing the effect.
// Driven per-frame by PsychicWaveOverlayController.
//
// TRUE colour inversion with NO framebuffer read. The fixed-function blend
//     Blend OneMinusDstColor OneMinusSrcAlpha
// with a premultiplied source (rgb = _RingColor.rgb * a) evaluates to
//     result = RingColor*a*(1-Dst) + (1-a)*Dst
// i.e. tinted exact inversion where a == 1, untouched where a == 0. White
// _RingColor = pure inversion; any other colour pushes the inverted result
// toward that hue (useful on dark scenes, where pure inversion reads white).
//
// WHY NOT A GRAB-PASS: the overlay is UGUI on a Screen-Space-Overlay canvas.
// Under URP that canvas is composited AFTER the SRP has finished, so no SRP
// texture (_CameraOpaqueTexture and friends) ever contains what this overlay
// covers, and GrabPass does not exist in URP at all. Blending, by contrast,
// always operates against the real backbuffer.
//
// SCOPE LIMIT: only content drawn BEFORE this overlay in the composition
// order is inverted. Canvases sorted above it (tooltips, modals) are
// untouched. Accepted, and arguably desirable.
//
// Driven properties:
//   _WaveEnabled  0/1              wave on/off
//   _WaveCenter   xy in [0,1]      anchor in viewport space (the performer)
//   _OuterRadius  viewport units   leading (cover) front
//   _InnerRadius  viewport units   trailing (uncover) front
//   _EdgeWidth    viewport units   soft-edge width of both fronts
//   _Aspect       w/h              aspect correction (else fronts are ellipses)
//   _Intensity    0..1             global strength
//   _RingColor    rgb              tint multiplied into the inversion
//
// The overlay Image must cover the screen, but needs NO sprite: viewport
// position is derived from ComputeScreenPos, not from sprite UVs.
Shader "ALWTTT/UI/PsychicWaveInvert"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint (UGUI slot, unused)", Color) = (1,1,1,1)

        _WaveEnabled ("Wave Enabled", Float) = 0
        _WaveCenter ("Wave Center (viewport)", Vector) = (0.5, 0.5, 0, 0)
        _OuterRadius ("Outer Radius (viewport)", Float) = 0
        _InnerRadius ("Inner Radius (viewport)", Float) = 0
        _EdgeWidth ("Edge Softness (viewport)", Float) = 0.12
        _Aspect ("Screen Aspect w/h", Float) = 1.777
        _Intensity ("Invert Intensity", Range(0,1)) = 1
        _RingColor ("Wave Colour (multiplies the inversion)", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        // THE point of this shader. Do NOT "fix" this to SrcAlpha OneMinusSrcAlpha:
        // that turns the wave into an additive glow and the inversion is gone.
        Blend OneMinusDstColor OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "InvertWave"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _ClipRect;
            float _WaveEnabled;
            float4 _WaveCenter;
            float _OuterRadius;
            float _InnerRadius;
            float _EdgeWidth;
            float _Aspect;
            float _Intensity;
            fixed4 _RingColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                // [v4-fix] Screen position, NOT sprite UV. An Image with no
                // sprite writes zero UVs to every vertex (Image.cs:
                // GenerateSimpleSprite -> uv = Vector4.zero), which collapses
                // the radial maths. ComputeScreenPos also handles the D3D/GL
                // Y-flip that a raw SV_POSITION divide would get wrong.
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // [v4-fix] Viewport UV from screen position. Independent of
                // the sprite slot, of sprite atlasing, and of the Image's exact
                // rect - all three of which silently broke the texcoord version.
                float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w, 1e-6);

                float2 d = screenUV - _WaveCenter.xy;

                // Without this the fronts are stretched to the screen aspect.
                d.x *= _Aspect;

                float dist = length(d);
                float w = max(_EdgeWidth, 1e-4);

                // Inside the leading (cover) front, soft edge just inside it.
                float outer = 1.0 - smoothstep(_OuterRadius - w, _OuterRadius, dist);

                // Outside the trailing (uncover) front. The (R - w, R) window
                // makes _InnerRadius == 0 evaluate to 1 everywhere (full disc),
                // so phase 1 needs no special case.
                float inner = smoothstep(_InnerRadius - w, _InnerRadius, dist);

                float a = outer * inner;

                a *= _Intensity * _WaveEnabled;

                // Let CanvasGroup / vertex alpha fades flow through.
                a *= IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                // Premultiplied: rgb = colour * a. Emitting (colour, a) without
                // the premultiply gives a washed glow instead of an inversion.
                return fixed4(_RingColor.rgb * a, a);
            }
        ENDCG
        }
    }
}
