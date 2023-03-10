using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderLimits : MonoBehaviour
{
    Slider slider;
    Text maxValueLabel;
    private void Start()
    {
        slider = transform.parent.GetComponentInChildren<Slider>();
        maxValueLabel = GetComponentsInChildren<Text>()[1];
        UpdateLabels();
    }
    public void DoubleMax()
    {
        if (slider.maxValue * 2f < float.MaxValue / 2f)
        {
            slider.maxValue *= 2f;
        }
        UpdateLabels();
    }
    public void HalfMax()
    {
        if (slider.maxValue / 2f > 0)
        {
            slider.maxValue /= 2f;
        }
        UpdateLabels();
    }
    void UpdateLabels()
    {
        maxValueLabel.text = "Max\n"+slider.maxValue;
    }
}
