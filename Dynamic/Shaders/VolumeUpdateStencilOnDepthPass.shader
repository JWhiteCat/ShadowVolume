Shader "ShadowVolume/VolumeUpdateStencilOnDepthPass"
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
            Name "ExtrudeAndStencil"
            Tags { "LightMode"="VolumeStencil" }
            ColorMask 0
            Stencil
            {
                PassFront IncrWrap
                PassBack DecrWrap
            }
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma vertex ExtrudeVertex
            #pragma fragment ExtrudeFragment
            #include "ExtrudeVolume.hlsl"
            ENDHLSL
        }
    }
}
