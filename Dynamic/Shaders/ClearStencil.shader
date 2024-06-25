Shader "ShadowVolume/ClearStencil"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="LightweightPipeline"
            "IgnoreProjector"="true"
        }

        Pass
        {
            Name "ClearStencil"
            Tags
            {
                "LightMode"="ClearStencil"
            }
            ColorMask 0
            Stencil
            {
                Ref 0
                Comp Always
                Pass Replace
            }
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 position : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.position = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(0.0, 0.0, 0.0, 1.0);
            }
            ENDHLSL
        }
    }
}