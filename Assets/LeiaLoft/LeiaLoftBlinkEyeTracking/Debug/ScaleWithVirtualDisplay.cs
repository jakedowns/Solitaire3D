using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithVirtualDisplay : MonoBehaviour
{
    Vector3 startScale;
    float startVDHeight;
    LeiaVirtualDisplay virtualDisplay;

    void Start()
    {
        virtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();
        startVDHeight = virtualDisplay.Height;
        startScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = startScale * (virtualDisplay.Height / startVDHeight);
    }
}
