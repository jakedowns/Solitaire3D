using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LabeledSlider : MonoBehaviour
{
    Slider slider;
    public string VariableName;
    Text text;
    public Component component;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        text = GetComponentInChildren<Text>();
        float val = GetPropertyValue(VariableName, component);
        slider.SetValueWithoutNotify(val);
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            float val = GetPropertyValue(VariableName, component);
            slider.SetValueWithoutNotify(val);
        }
        text.text = VariableName + ": " + slider.value;
    }

    float GetPropertyValue(string propertyName, Component component)
    {
        System.Reflection.PropertyInfo propName = component.GetType().GetProperty(propertyName);
        if (propName != null)
        {
            return (float)propName.GetValue(component);
        }

        return -1;
    }
}
