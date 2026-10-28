Shader "SuyuRun/SierraTaytaMatte"
{
    Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _SierraMatte("Matte palette",Float)=1 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/SuyuRun/Art/Shaders/SierraTaytaPalette.cginc"
            sampler2D _MainTex;float _SierraMatte;
            struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct Output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            Output vert(Input i){Output o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
            fixed4 frag(Output i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv);if(_SierraMatte>.5)c.rgb=SierraTaytaMatte(c.rgb);return c*i.color;}
            ENDCG
        }
    }
}
