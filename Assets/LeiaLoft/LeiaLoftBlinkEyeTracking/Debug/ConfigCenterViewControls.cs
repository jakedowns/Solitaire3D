using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class ConfigCenterViewControls : MonoBehaviour
{
    public Text label;
    // Start is called before the first frame update
    void Start()
    {
        label.text = ""+ LeiaDisplay.Instance.displayConfig.CenterViewNumber;
    }

    
    public void Subtract()
    {
        LeiaDisplay.Instance.displayConfig.CenterViewNumber -= .05f;
        label.text = ""+ LeiaDisplay.Instance.displayConfig.CenterViewNumber.ToString("0.00");
    }
    
    public void Add()
    {
        LeiaDisplay.Instance.displayConfig.CenterViewNumber += .05f;
        label.text = ""+ LeiaDisplay.Instance.displayConfig.CenterViewNumber.ToString("0.00");
    }
}
