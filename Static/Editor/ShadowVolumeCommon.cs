using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ShadowVolumeCommon
{
    public static List<GameObject> GetStaticShadowGameObjects()
    {
        List<GameObject> gos = GetAllSceneObjectsWithInactive();
        List<GameObject> staticShadowOnlyGos = FilterStaticShadowGameObjects(gos);
        return staticShadowOnlyGos;
    }

    public static List<GameObject> GetAndSetActiveStaticShadowOnly(bool active = true)
    {
        List<GameObject> gos = GetAllSceneObjectsWithInactive();
        List<GameObject> staticShadowOnlyGos = FilterStaticShadowOnlyGameObjects(gos);
        SetActiveGameObjects(staticShadowOnlyGos, active);
        return staticShadowOnlyGos;
    }

    // 这个会搜到prefab资产
    private static List<GameObject> GetAllSceneObjectsWithInactiveRuntime()
    {
        List<GameObject> result = new List<GameObject>();
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform transform in transforms)
        {
            GameObject go = transform.gameObject;
            if (go != null)
            {
                result.Add(go);
            }
        }

        return result;
    }

    //用于获取所有Hierarchy中的物体，包括被禁用的物体
    private static List<GameObject> GetAllSceneObjectsWithInactive()
    {
        var allTransforms = Resources.FindObjectsOfTypeAll(typeof(Transform));
        var previousSelection = Selection.objects;
        Selection.objects = allTransforms.Cast<Transform>()
            .Where(x => x != null)
            .Select(x => x.gameObject)
            //如果你只想获取所有在Hierarchy中被禁用的物体，反注释下面代码
            //.Where(x => x != null && !x.activeInHierarchy)
            .Cast<UnityEngine.Object>().ToArray();

        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        Selection.objects = previousSelection;

        return selectedTransforms.Select(tr => tr.gameObject).ToList();
    }

    private static List<GameObject> FilterStaticShadowGameObjects(List<GameObject> gos)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var go in gos)
        {
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (meshRenderer.shadowCastingMode is UnityEngine.Rendering.ShadowCastingMode.On
                        or UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly &&
                    meshRenderer.staticShadowCaster)
                {
                    result.Add(go);
                }
            }
        }

        return result;
    }

    private static List<GameObject> FilterStaticShadowOnlyGameObjects(List<GameObject> gos)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var go in gos)
        {
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (meshRenderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly &&
                    meshRenderer.staticShadowCaster)
                {
                    result.Add(go);
                }
            }
        }

        return result;
    }

    private static void SetActiveGameObjects(List<GameObject> gos, bool active = true)
    {
        foreach (var go in gos)
        {
            go.SetActive(active);
        }
    }
}