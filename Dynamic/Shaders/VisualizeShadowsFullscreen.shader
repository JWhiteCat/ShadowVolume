Shader "ShadowVolume/VisualizeShadowsFullscreen"
{
    Properties
    {
    }
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
            Name "ShadowsFullscreen"
            Tags { "LightMode"="LightweightForward" }
            Stencil
            {
                Comp NotEqual
                Ref 0
            }
            Cull Back
            ZTest Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma vertex VisualizeShadowVertex
            #pragma fragment VisualizeShadowFragment
#if UNITY_VERSION >= 201930
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
#endif

            half _SST_ShadowIntensity;

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

            Varyings VisualizeShadowVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 VisualizeShadowFragment(Varyings input) : SV_Target
            {
                return half4(0.0, 0.0, 0.0, _SST_ShadowIntensity);
            }
            ENDHLSL
        }
    }
}
