using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderLayerTest : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.LogError(gameObject.GetComponent<MeshRenderer>().renderingLayerMask);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError(gameObject.GetComponent<MeshRenderer>().renderingLayerMask);
    }

    // Update is called once per frame
    void Update()
    {
    }
}