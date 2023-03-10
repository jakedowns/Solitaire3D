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
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Slider>().onValueChanged.AddListener(delegate { OnSliderChanged(gameObject.GetComponent<Slider>()); });
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
