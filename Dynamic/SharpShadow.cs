using UnityEngine;

namespace ShadowVolume
{
#if SHARP_SHADOWS_DEBUG
    using Debug = UnityEngine.Debug;
#else
    using Debug = DebugNoOp;
#endif

    [System.Serializable]
    public class RuntimeCreationSettings
    {
        [Tooltip(Docs.Tooltip.AllowCameraInShadow)]
        public bool allowCameraInShadow = false;

        [Tooltip(Docs.Tooltip.BoundsPadFactor)]
        public float boundsPadFactor = 1.0f;
    }

    [HelpURL("https://gustavolsson.com/projects/sharp-shadows-toolkit/")]
    [ExecuteInEditMode]
    public class SharpShadow : MonoBehaviour
    {
        [Tooltip(Docs.Tooltip.ShadowAsset)]
        public ShadowAsset shadowAsset;

        [Tooltip(Docs.Tooltip.CreateRuntimeShadowAsset)]
        public bool createRuntimeShadowAsset;

        [Tooltip(Docs.Tooltip.RuntimeCreationSettings)]
        public RuntimeCreationSettings runtimeCreationSettings;

        protected ShadowAsset runtimeShadowAsset;
        protected SkinnedMeshRenderer skinnedRenderer;

        public void CreateRuntimeShadowAsset(bool forceUpdate)
        {
            if (!createRuntimeShadowAsset)
            {
                // We should not be here...
                CleanUpRuntimeShadowAsset();
                return;
            }
            if (runtimeShadowAsset && !forceUpdate)
            {
                // Runtime asset already exists
                return;
            }

            // Re-create runtime asset
            CleanUpRuntimeShadowAsset();

            Debug.LogFormat("Creating runtime shadow asset for '{0}'", name);

            Mesh sourceMesh = null;
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter)
            {
                sourceMesh = meshFilter.sharedMesh;
            }
            var skinnedMeshRenderer = transform.parent?.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer)
            {
                sourceMesh = skinnedMeshRenderer.sharedMesh;
            }
            if (!sourceMesh)
            {
                return;
            }
            var asset = ScriptableObject.CreateInstance<ShadowAsset>();
            ShadowMesh.Create(
                sourceMesh,
                runtimeCreationSettings.boundsPadFactor,
                ref asset.shadowMesh,
                out asset.isAnimated,
                out asset.isTwoManifold,
                out asset.usesThirtyTwoBitIndices,
                out asset.vertexCount,
                out asset.triangleCount,
                out asset.boundsPadFactor);
            if (!asset.shadowMesh)
            {
                return;
            }
            asset.allowCameraInShadow = runtimeCreationSettings.allowCameraInShadow;

            runtimeShadowAsset = asset;
        }

        protected static void DestroyAlways(Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
#else
            Destroy(obj);
#endif
        }

        protected void CleanUpRuntimeShadowAsset()
        {
            if (runtimeShadowAsset)
            {
                Debug.LogFormat("Cleaning up runtime shadow asset for '{0}'", name);

                if (runtimeShadowAsset.shadowMesh)
                {
                    DestroyAlways(runtimeShadowAsset.shadowMesh);
                }
                DestroyAlways(runtimeShadowAsset);
                runtimeShadowAsset = null;
            }
        }

        protected void Initialize(bool forceUpdate)
        {
            CreateRuntimeShadowAsset(forceUpdate);

            if (shadowAsset && shadowAsset.isAnimated ||
                runtimeShadowAsset && runtimeShadowAsset.isAnimated)
            {
                skinnedRenderer = GetComponent<SkinnedMeshRenderer>();
            }
        }

        public void OnEnable()
        {
            Initialize(true);

#if SHARP_SHADOWS_DEBUG
            if (!SharpShadowManager.isInitialized)
            {
                Debug.LogError("Manager not yet initialized");
            }
#endif

            SharpShadowManager.instance?.Add(this);
        }

        public void OnDisable()
        {
            SharpShadowManager.instance?.Remove(this);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Initialize(true);
        }
#endif

        public void Render(Material updateStencilAlwaysMaterial, Material updateStencilOnDepthPassMaterial)
        {
            // Prefer runtime asset
            var asset = runtimeShadowAsset ? runtimeShadowAsset : shadowAsset;
            if (!asset || !asset.shadowMesh)
            {
                return;
            }

            if (asset.isAnimated && skinnedRenderer)
            {
                // Skinned

                skinnedRenderer.sharedMesh = asset.shadowMesh;
                skinnedRenderer.localBounds = asset.shadowMesh.bounds;
                skinnedRenderer.updateWhenOffscreen = false;

                var materials = skinnedRenderer.sharedMaterials;
                if (asset.allowCameraInShadow)
                {
                    if (materials.Length != 2)
                    {
                        materials = new Material[2];
                    }
                    materials[0] = updateStencilAlwaysMaterial;
                    materials[1] = updateStencilOnDepthPassMaterial;
                }
                else
                {
                    if (materials.Length != 1)
                    {
                        materials = new Material[1];
                    }
                    materials[0] = updateStencilOnDepthPassMaterial;
                }
                skinnedRenderer.sharedMaterials = materials;
            }
            else
            {
                // Non-skinned
                if (asset.allowCameraInShadow)
                {
                    Graphics.DrawMesh(asset.shadowMesh, transform.localToWorldMatrix, updateStencilAlwaysMaterial, gameObject.layer);
                }
                Graphics.DrawMesh(asset.shadowMesh, transform.localToWorldMatrix, updateStencilOnDepthPassMaterial, gameObject.layer);
            }
        }
    }
}
