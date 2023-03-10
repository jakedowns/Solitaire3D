using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPrefSetterUI : MonoBehaviour
{
    public InputField nameInput;
    public InputField valueInput;


    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetButtonPressed()
    {
        PlayerPrefs.SetString(nameInput.text, valueInput.text);
        Debug.Log("Setting player pref "+nameInput.text+" to value "+valueInput.text);
    }
}
