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
    public class StencilPass : ScriptableRenderPass
    {
        protected List<ShaderTagId> tagsToRender;

        public StencilPass(RenderPassEvent evt, string[] shaderTagsToRender)
        {
            renderPassEvent = evt;

            tagsToRender = new List<ShaderTagId>();
            for (var i = 0; i < shaderTagsToRender.Length; i++)
            {
                tagsToRender.Add(new ShaderTagId(shaderTagsToRender[i]));
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(tagsToRender, ref renderingData, SortingCriteria.CommonOpaque);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings,
                ref renderStateBlock);
        }
    }
}