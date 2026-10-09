Shader "SuyuRun/PortalThreshold"
{
    Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _DoorPlaneX("Door threshold",Float)=0 }
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
            sampler2D _MainTex;float _DoorPlaneX;
            struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct Output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float x:TEXCOORD1;};
            Output vert(Input i){Output o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;o.x=mul(unity_ObjectToWorld,i.vertex).x;return o;}
            fixed4 frag(Output i):SV_Target {clip(_DoorPlaneX-i.x);return tex2D(_MainTex,i.uv)*i.color;}
            ENDCG
        }
    }
}
