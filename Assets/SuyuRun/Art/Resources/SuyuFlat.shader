Shader "SuyuRun/FlatColor"
{
    Properties { _Opacity ("Opacity", Range(0,1)) = 1 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half _Opacity;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; };
            struct Varyings { float4 positionHCS : SV_POSITION; half4 color : COLOR; };
            Varyings vert(Attributes v) { Varyings o; o.positionHCS=TransformObjectToHClip(v.positionOS.xyz); o.color=v.color; return o; }
            half4 frag(Varyings v) : SV_Target
            {
                #ifndef UNITY_COLORSPACE_GAMMA
                v.color.rgb=SRGBToLinear(v.color.rgb);
                #endif
                v.color.a *= _Opacity;
                return v.color;
            }
            ENDHLSL
        }
    }
}
