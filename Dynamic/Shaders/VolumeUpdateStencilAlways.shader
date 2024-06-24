Shader "ShadowVolume/VolumeUpdateStencilAlways"
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
            Tags { "LightMode"="VolumeStencil" }
            ColorMask 0
            Stencil
            {
                PassFront DecrWrap
                PassBack IncrWrap
            }
            Cull Off
            ZWrite Off
            ZTest Off

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
