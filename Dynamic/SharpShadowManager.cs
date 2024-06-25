using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowVolume
{
#if SHARP_SHADOWS_DEBUG
    using Debug = UnityEngine.Debug;
#else
    using Debug = DebugNoOp;
#endif

    [ExecuteInEditMode]
    public class SharpShadowManager : MonoBehaviour
    {
        protected static SharpShadowManager _instance;

        public static bool isInitialized
        {
            get { return _instance != null; }
        }

        public static SharpShadowManager instance
        {
            get { return _instance; }
        }

        protected List<SharpShadow> shadows;
        protected bool stencilBufferSupported;
        protected static Material updateStencilAlwaysMaterial;
        protected static Material updateStencilOnDepthPassMaterial;

        public void OnEnable()
        {
            Debug.Log("Manager enabled");

            _instance = this;

            shadows = new List<SharpShadow>();
            stencilBufferSupported = RenderTexture.SupportsStencil(null);
        }

        public void OnDisable()
        {
            Debug.Log("Manager disabled");

            if (_instance == this)
            {
                _instance = null;
            }

            shadows.Clear();
        }

        public void Add(SharpShadow shadow)
        {
            shadows.Add(shadow);
        }

        public void Remove(SharpShadow shadow)
        {
            shadows.Remove(shadow);
        }

        public void LateUpdate()
        {
            // if (!stencilBufferSupported)
            // {
            //     return;
            // }

            if (!updateStencilAlwaysMaterial)
            {
                updateStencilAlwaysMaterial = new Material(Shader.Find("ShadowVolume/VolumeUpdateStencilAlways"))
                {
                    enableInstancing = true,
                };
            }
            if (!updateStencilOnDepthPassMaterial)
            {
                updateStencilOnDepthPassMaterial = new Material(Shader.Find("ShadowVolume/VolumeUpdateStencilOnDepthPass"))
                {
                    enableInstancing = true,
                };
            }

            for (var i = 0; i < shadows.Count; i++)
            {
                shadows[i].Render(updateStencilAlwaysMaterial, updateStencilOnDepthPassMaterial);
            }
        }
    }
}
