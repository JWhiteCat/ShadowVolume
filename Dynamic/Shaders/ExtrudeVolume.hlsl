#if UNITY_VERSION >= 201930
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#else
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
#endif

float _SST_NearExtrusionDistance;
float _SST_FarExtrusionDistance;

struct Attributes
{
    float4 position     : POSITION;
    float3 normal       : NORMAL;
    float4 tangent      : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 position : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings ExtrudeVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    VertexPositionInputs v = GetVertexPositionInputs(input.position.xyz);
    VertexNormalInputs n = GetVertexNormalInputs(input.normal, input.tangent);

    Light mainLight = GetMainLight();
    float amount = dot(n.normalWS, mainLight.direction) < 0.0 ? _SST_FarExtrusionDistance : _SST_NearExtrusionDistance;
    float3 worldPos = v.positionWS - mainLight.direction * amount;

    output.position = TransformWorldToHClip(worldPos);
    return output;
}

half4 ExtrudeFragment(Varyings input) : SV_Target
{
    return half4(0.0, 0.0, 0.0, 1.0);
}
