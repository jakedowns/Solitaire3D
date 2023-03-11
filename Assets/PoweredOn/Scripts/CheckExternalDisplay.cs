using UnityEngine;
using System.Collections;
using PoweredOn.Managers;

public class CheckExternalDisplay : MonoBehaviour
{

    private AndroidJavaObject displayManager;
    private int checkIntervalSeconds = 3;
    private float lastCheckTime = 0;

    public int DisplayCount { get; private set; }

    void Start()
    {
        // if the current runtime is not android or we are not running on a device, then exit
        DisplayCount = Display.displays.Length;
        Debug.LogWarning("CheckExternalDisplay: DisplayCount: " + DisplayCount);
        
        if (Application.platform == RuntimePlatform.Android && !Application.isEditor)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
            displayManager = context.Call<AndroidJavaObject>("getSystemService", "display");
        }

    }

    void Update()
    {

        if (Time.timeSinceLevelLoad - lastCheckTime > checkIntervalSeconds)
        {
            lastCheckTime = Time.timeSinceLevelLoad;
            //string[] audio_devices = GameManager.Instance.ListAudioDeviceNames();
            // if the current runtime is not android or we are not running on a device, then exit
            if (Application.platform != RuntimePlatform.Android || Application.isEditor)
            {
                DisplayCount = Display.displays.Length;
                Debug.LogWarning("CheckExternalDisplay: DisplayCount: " + DisplayCount);
                return;
            }
            AndroidJavaObject[] displays = displayManager.Call<AndroidJavaObject[]>("getDisplays");
            DisplayCount = displays.Length;
            Debug.LogWarning("CheckExternalDisplay: DisplayCount: " + DisplayCount);
        }
    }
}
