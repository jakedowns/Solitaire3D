using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeiaLoft;
using System;

public class BacklightEnforcer : MonoBehaviour
{
    float backlightLockDuration = 1f;

    void Awake()
    {
        int currentTime = GetUnixTime();

        if (currentTime - lastQuitTime > backlightLockDuration)
        {
            appQuitting = false;
        }
    }

    public static int GetUnixTime()
    {
        return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    void OnApplicationQuit()
    {
        lastQuitTime = GetUnixTime();
        LeiaDisplay.Instance.QuitApp();
        appQuitting = true;
    }

    public static int lastQuitTime
    {
        get
        {
            return PlayerPrefs.GetInt("lastQuitTime");
        }
        set
        {
            PlayerPrefs.SetInt("lastQuitTime", value);
        }
    }

    public static bool appQuitting
    {
        get
        {
            return (PlayerPrefs.GetInt("AppQuitting") == 1);
        }
        set
        {
            PlayerPrefs.SetInt("AppQuitting", value ? 1 : 0);
        }
    }
}
