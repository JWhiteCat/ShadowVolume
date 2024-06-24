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
    public class MultiplyPass : ScriptableRenderPass
    {
        public float shadowIntensity;

        protected List<ShaderTagId> shaderTagsToRender;
        protected Material visualizeShadowsFullscreen;

        public MultiplyPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (visualizeShadowsFullscreen == null)
            {
                visualizeShadowsFullscreen = new Material(Shader.Find("ShadowVolume/VisualizeShadowsFullscreen"));
                if (visualizeShadowsFullscreen == null)
                {
                    return;
                }
            }

            var camera = renderingData.cameraData.camera;

            var cmd = CommandBufferPool.Get("SharpShadowsToolkit Multiply Pass");

            // Visualize shadows
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, visualizeShadowsFullscreen);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}