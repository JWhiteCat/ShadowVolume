Shader "Custom URP/Unlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color",Color) = (1.0,1.0,1.0,1.0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitVertexPass
            #pragma fragment UnlitFragmentPass

            //核心库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 baseUV : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                //没有具体意义的语义可以自定义
                float2 baseUV : VAR_BASE_UV;
            };

            TEXTURE2D (_BaseMap);
            //贴图对应的采样器：samper+贴图变量名
            SAMPLER (sampler_BaseMap);
            //还可以声明如下
            //SAMPLER(sampler_linear_clamp);

            //SRP批处理需要把数据实例数据放到一个Buffer里
            CBUFFER_START (UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            CBUFFER_END

            Varyings UnlitVertexPass(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.baseUV = TRANSFORM_TEX(input.baseUV, _BaseMap);
                return output;
            }

            float4 UnlitFragmentPass(Varyings input) : SV_Target
            {
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
                float4 baseColor = _BaseColor;
                return baseMap * baseColor;
            }
            ENDHLSL
        }
    }
}