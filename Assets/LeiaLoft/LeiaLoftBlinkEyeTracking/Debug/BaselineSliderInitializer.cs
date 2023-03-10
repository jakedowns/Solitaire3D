using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaselineSliderInitializer : MonoBehaviour
{
    LeiaCamera leiaCamera;
    Slider slider;
    void Start()
    {
        leiaCamera = FindObjectOfType<LeiaCamera>();
        slider = GetComponent<Slider>();
        //slider.maxValue = Mathf.Max(leiaCamera.BaselineScaling * 4f , 2f);
        slider.SetValueWithoutNotify(leiaCamera.BaselineScaling);
        slider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    void OnValueChanged()
    {
        leiaCamera.BaselineScaling = slider.value;
    }
}
