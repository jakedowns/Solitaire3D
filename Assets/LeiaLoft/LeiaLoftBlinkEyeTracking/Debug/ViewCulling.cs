using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewCulling : MonoBehaviour
{
    Camera[] cameras;
    void Start()
    {
        Invoke("Go", 1f);
    }

    void Go()
    {
        cameras = GetComponentsInChildren<Camera>();
        for(int i = 2; i < cameras.Length; i++)
        {
            int layerNum = i-2;
            cameras[i].cullingMask = 
                (1 << LayerMask.NameToLayer("View"+(layerNum)) | (1 << LayerMask.NameToLayer("Default")));
        }
        Invoke("Go", 1f);
    }
}
