using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ShowEntireScrollableAreaInEditMode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isPlaying)
        {
            Mask mask = GetComponent<Mask>();
            mask.enabled = true;
        }
    }
    
    
    // Update is called once per frame
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Mask mask = GetComponent<Mask>();
            mask.enabled = false;
        }
    }
    
}
