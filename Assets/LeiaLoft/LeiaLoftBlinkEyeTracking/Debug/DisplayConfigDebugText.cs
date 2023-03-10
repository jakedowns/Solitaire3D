using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class DisplayConfigDebugText : MonoBehaviour
{
    Text text;
    DisplayConfig config;

    void Start()
    {
        text = GetComponent<Text>();
        UpdateNow();
    }

    void UpdateNow()
    {
        config = LeiaDisplay.Instance.GetDisplayConfig();
        text.text = config.ToStringV2();
        Invoke("UpdateNow",.5f);
    }
}
