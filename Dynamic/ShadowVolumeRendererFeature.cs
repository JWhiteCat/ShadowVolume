using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;

#else
using UnityEngine.Rendering.LWRP;
#endif

namespace ShadowVolume
{
    public class ShadowVolumeRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public enum ShadeMode
        {
            InjectIntoScreenSpaceShadowResolveTexture,
            MultiplySceneAfterOpaque,
        }

        [System.Serializable]
        public class Settings
        {
            public bool enabled = true;

            [Delayed] [Tooltip(Docs.Tooltip.NearExtrusionDistance)]
            public float nearExtrusionDistance = 0.002f;

            [Delayed] [Tooltip(Docs.Tooltip.FarExtrusionDistance)]
            public float farExtrusionDistance = 100.0f;

            [Range(0.0f, 1.0f)] [Tooltip(Docs.Tooltip.ShadowIntensity)]
            public float shadowIntensity = 1.0f;

            [Tooltip(Docs.Tooltip.ShadeMode)] public ShadeMode shadeMode = ShadeMode.MultiplySceneAfterOpaque;

            [Tooltip(Docs.Tooltip.MitigateSelfShadowArtifacts)]
            public bool mitigateSelfShadowArtifacts = true;
        }

        public Settings settings = new Settings();

        protected StencilPass stencilPass;
        protected MultiplyPass multiplyPass;
        protected bool stencilBufferSupported;

        public override void Create()
        {
            stencilPass = new StencilPass(RenderPassEvent.AfterRenderingOpaques + 1, new string[]
            {
                // "UniversalForward", "LightweightForward", "SRPDefaultUnlit",
                "VolumeStencil"
            });
            multiplyPass = new MultiplyPass(RenderPassEvent.AfterRenderingOpaques + 2);

            stencilBufferSupported = RenderTexture.SupportsStencil(null);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled || renderingData.lightData.mainLightIndex == -1 || !stencilBufferSupported)
            {
                return;
            }

            // Update materials
            var nearDist = settings.mitigateSelfShadowArtifacts
                ? settings.farExtrusionDistance
                : settings.nearExtrusionDistance;
            var farDist = settings.mitigateSelfShadowArtifacts
                ? settings.nearExtrusionDistance
                : settings.farExtrusionDistance;

            Shader.SetGlobalFloat("_SST_ShadowIntensity", settings.shadowIntensity);
            Shader.SetGlobalFloat("_SST_NearExtrusionDistance", nearDist);
            Shader.SetGlobalFloat("_SST_FarExtrusionDistance", farDist);

            // Queue passes
            renderer.EnqueuePass(stencilPass);

            multiplyPass.shadowIntensity = settings.shadowIntensity;
            renderer.EnqueuePass(multiplyPass);
        }
    }
}