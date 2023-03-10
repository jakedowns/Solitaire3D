using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class CameraShiftScaleSliderInitializer : MonoBehaviour
{
    Slider slider;
    Text text;
    LeiaCamera leiaCamera;

    void Start()
    {
        text = GetComponentInChildren<Text>();
        slider = GetComponent<Slider>();
        leiaCamera = FindObjectOfType<LeiaCamera>();
        //slider.maxValue = Mathf.Max(virtualDisplay.CameraShiftScaleStereo * 4f , 2f);
        slider.SetValueWithoutNotify(leiaCamera.CameraShiftScaling);
        slider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    void OnValueChanged()
    {
        leiaCamera.CameraShiftScaling = slider.value;
        text.text = "aX: "+slider.value;
    }
}
