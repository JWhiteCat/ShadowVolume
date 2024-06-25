using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace ShadowVolume
{
    public class ShadowSettings : ScriptableObject
    {
        #region memeber

        public int finalLayer = 6;
        public float capsOffset = 0.001f;

        private static ShadowSettings instance;

        public static ShadowSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<ShadowSettings>("Assets/ShadowSettings.asset");
                    if (instance == null)
                    {
                        instance = CreateInstance<ShadowSettings>();
                        instance.name = "ShadowSettings";
#if UNITY_EDITOR
                        AssetDatabase.CreateAsset(instance, "Assets/ShadowSettings.asset");
#endif
                    }
                }

                return instance;
            }
        }

        #endregion
    }
}
#endif