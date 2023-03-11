using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderLabel : MonoBehaviour
{
    [SerializeField]
    public Text labelText;
    [SerializeField]
    public string prefix = "Name";

    private Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        gameObject.GetComponent<Slider>().onValueChanged.AddListener(delegate { OnSliderChanged(gameObject.GetComponent<Slider>()); });

        // get parent > parent > MinusButton
        var minusButton = gameObject.transform.parent.parent.Find("MinusButton").gameObject.GetComponent<Button>();
        // get parent > parent > PlusButton
        var plusButton = gameObject.transform.parent.parent.Find("PlusButton").gameObject.GetComponent<Button>();

        // determine a good step_size based on the slider's min/max and if wholeNumbers is true or not
        float step_size = 0.1f;
        if (slider.wholeNumbers)
        {
            step_size = 1.0f;
        }
        else
        {
            float range = slider.maxValue - slider.minValue;
            step_size = range / 10.0f;
        }

        // add event listener to minusButton to decrement slider value
        minusButton.onClick.AddListener(delegate { slider.value -= step_size; OnSliderChanged(slider); });
        // add event listener to plusButton to increment slider value
        plusButton.onClick.AddListener(delegate { slider.value += step_size; OnSliderChanged(slider); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderChanged(Slider slider)
    {
        // update label
        labelText.text = prefix + ": " + slider.value;
        // notify game manager
        PoweredOn.Managers.GameManager.Instance.OnSliderChanged(slider);
    }
}
