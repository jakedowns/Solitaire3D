using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DisplayConfigLabel : MonoBehaviour
{
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        Invoke("UpdateLabel",1f);
    }
    // Update is called once per frame
    void UpdateLabel()
    {
        Invoke("UpdateLabel",1f);
        text.text = LeiaDisplay.Instance.GetDisplayConfig().ToString();
    }
}