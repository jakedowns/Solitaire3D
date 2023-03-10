using UnityEngine;
using System.Collections;

public class CheckExternalDisplay : MonoBehaviour
{

    private AndroidJavaObject displayManager;
    private int checkIntervalSeconds = 3;
    private float lastCheckTime = 0;

    public int DisplayCount { get; private set; }

    void Start()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
        displayManager = context.Call<AndroidJavaObject>("getSystemService", "display");
    }

    void Update()
    {
        if (Time.time - lastCheckTime > checkIntervalSeconds)
        {
            lastCheckTime = Time.time;
            AndroidJavaObject[] displays = displayManager.Call<AndroidJavaObject[]>("getDisplays");
            DisplayCount = displays.Length;
        }
    }
}
