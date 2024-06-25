using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;

#else
using UnityEngine.Rendering.LWRP;
#endif

namespace ShadowVolume
{
    public class ClearStencilPass : ScriptableRenderPass
    {
        protected List<ShaderTagId> tagsToRender;
        protected Material ClearStencilMaterial;

        public ClearStencilPass(RenderPassEvent evt, string[] shaderTagsToRender)
        {
            base.profilingSampler = new ProfilingSampler(nameof(ClearStencilPass));
            renderPassEvent = evt;
            
            tagsToRender = new List<ShaderTagId>();
            for (var i = 0; i < shaderTagsToRender.Length; i++)
            {
                tagsToRender.Add(new ShaderTagId(shaderTagsToRender[i]));
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (ClearStencilMaterial == null)
            {
                ClearStencilMaterial = new Material(Shader.Find("ShadowVolume/ClearStencil"));
                //ClearStencilMaterial = new Material(Shader.Find("Custom URP/Unlit"));
                if (ClearStencilMaterial == null)
                {
                    return;
                }
            }
            
            var drawingSettings = CreateDrawingSettings(tagsToRender, ref renderingData, SortingCriteria.CommonOpaque);
            drawingSettings.overrideMaterial = ClearStencilMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;
            
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1, 1 << 7);
            var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings,
                ref renderStateBlock);
        }
    }
}