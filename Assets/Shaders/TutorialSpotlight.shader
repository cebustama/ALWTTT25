// [S4 D3=B] Tutorial spotlight overlay shader.
//
// A faithful UI/Default-derived shader (UGUI CanvasRenderer; works on
// Screen-Space-Overlay under URP — UGUI does not route through the SRP) that
// punches a HOLE into the rendered Image at a runtime-settable viewport rect,
// shaped by a swappable mask sprite's alpha (circle / star / any sprite).
//
// This is the hand-written equivalent of the inverted Canvas-Shader-Graph mask
// in the reference tutorial: we sample the hole sprite and use (1 - holeAlpha)
// as a multiplier on the overlay's own alpha. It is delivered as ShaderLab so it
// is text-authored and portable across Unity versions, and it adds the
// positionable/rotatable UV the reference graph lacks.
//
// Driven by TutorialOverlayView:
//   _HoleEnabled   0/1            spotlight on/off
//   _HoleCenter    xy in [0,1]    spotlight centre in viewport space
//   _HoleHalfSize  xy in [0,1]    spotlight half-extents in viewport space
//   _HoleRotation  radians        spotlight rotation
//   _HoleTex       sprite         hole shape (white-on-transparent)
//
// The overlay Image itself is full-screen stretched, so its UV (0..1) == viewport
// UV. Outside the spotlight quad the hole alpha is 0, so the overlay stays opaque.
Shader "ALWTTT/UI/TutorialSpotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,0.75)

        _HoleTex ("Hole Shape (alpha)", 2D) = "black" {}
        _HoleEnabled ("Hole Enabled", Float) = 0
        _HoleCenter ("Hole Center (viewport)", Vector) = (0.5, 0.5, 0, 0)
        _HoleHalfSize ("Hole Half Size (viewport)", Vector) = (0.1, 0.1, 0, 0)
        _HoleRotation ("Hole Rotation (rad)", Float) = 0

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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            sampler2D _HoleTex;
            float _HoleEnabled;
            float4 _HoleCenter;
            float4 _HoleHalfSize;
            float _HoleRotation;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                // --- Hole punch (inverted sprite mask at a viewport rect) ---
                if (_HoleEnabled > 0.5)
                {
                    // Image is full-screen stretched, so texcoord == viewport uv.
                    float2 d = IN.texcoord - _HoleCenter.xy;
                    float s = sin(_HoleRotation);
                    float c = cos(_HoleRotation);
                    float2 rd = float2(d.x * c + d.y * s, -d.x * s + d.y * c);

                    // Map into hole-local 0..1 over the spotlight quad.
                    float2 holeUV = rd / max(_HoleHalfSize.xy, 1e-5) * 0.5 + 0.5;

                    float inside =
                        step(0.0, holeUV.x) * step(holeUV.x, 1.0) *
                        step(0.0, holeUV.y) * step(holeUV.y, 1.0);

                    float holeA = tex2D(_HoleTex, holeUV).a * inside;
                    color.a *= saturate(1.0 - holeA);
                }

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
