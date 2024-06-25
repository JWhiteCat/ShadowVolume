using UnityEngine;

namespace ShadowVolume
{
    [System.Serializable]
    public class ShadowAsset : ScriptableObject
    {
        [Header("Build-time constants")]
        public Mesh shadowMesh;
        public bool isAnimated;
        public bool isTwoManifold;
        public bool usesThirtyTwoBitIndices;
        public int vertexCount;
        public float boundsPadFactor;
        public int triangleCount;

        [Header("Configurables")]

        [Tooltip(Docs.Tooltip.AllowCameraInShadow)]
        public bool allowCameraInShadow;

        [Tooltip(Docs.Tooltip.RenderLayer)]
        public int renderLayer;
    }
}