using LeiaLoft;
using UnityEngine;

public class DisplayConfigErrorPanel : MonoBehaviour
{
    [SerializeField] private GameObject errorPanelAndroid;
    [SerializeField] private GameObject errorPanelWindows;

    void Start()
    {
        DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();

        if (config.status != DisplayConfig.Status.SuccessfullyLoadedFromDevice)
        {
            #if UNITY_ANDROID
            if (errorPanelAndroid != null)
            {
                errorPanelAndroid.SetActive(true);
            }
            #endif
            /// <remove_from_public>
            #if UNITY_STANDALONE_WIN
            if (errorPanelWindows != null)
            {
                errorPanelWindows.SetActive(true);
            }
            #endif
            /// </remove_from_public>
        }
    }
}
