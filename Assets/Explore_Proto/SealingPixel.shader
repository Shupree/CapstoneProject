Shader "CyberExorcist/SealingPixel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Reveal ("Left to right reveal", Range(0,1)) = 1
        _RevealFromRight ("Reveal from right", Float) = 0
        _Dissolve ("Pixel dissolve", Range(0,1)) = 0
        _Brightness ("Brightness", Float) = 1
        _PixelGrid ("Pixel grid", Vector) = (150,100,0,0)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float4 world : TEXCOORD1; };
            sampler2D _MainTex;
            float4 _Color, _ClipRect, _PixelGrid, _TextureSampleAdd;
            float _Reveal, _Dissolve, _Brightness, _RevealFromRight;
            v2f vert(appdata v)
            {
                v2f o;
                o.world = v.vertex; o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; o.color = v.color * _Color;
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float2 cell = floor(i.uv * _PixelGrid.xy);
                float noise = frac(sin(dot(cell, float2(12.9898,78.233))) * 43758.5453);
                float revealX = lerp(i.uv.x, 1 - i.uv.x, _RevealFromRight);
                if (_Reveal < 1) clip(_Reveal - revealX - 0.000001);
                if (_Dissolve > 0) clip(noise - _Dissolve - 0.000001);
                float4 c = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;
                c.rgb *= _Brightness;
                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(i.world.xy, _ClipRect);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
