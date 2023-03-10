using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LabelUpdater : MonoBehaviour
{
    // Update is called once per frame
    
    void OnValidate()
    {
        Text text = GetComponentInChildren<Text>();
        if (text != null)
        {
        text.text = gameObject.name.Replace("Panel","");
        text.text = text.text.Replace("Button","");
        text.text = text.text.Replace("Label","");

        if (gameObject.name.Contains("("))
        {
            int cutPoint = gameObject.name.IndexOf("(")-1;
            gameObject.name = gameObject.name.Remove(cutPoint);
        }
        }
        else
        {
            Debug.LogError("text is null for gameobject "+gameObject.name);
        }
    }
    
}
