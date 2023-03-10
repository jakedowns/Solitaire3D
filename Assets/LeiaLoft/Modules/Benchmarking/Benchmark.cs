using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Benchmark : MonoBehaviour
{
    bool isTracking = true;

    [SerializeField]
    BlinkTrackingUnityPlugin blinkObject;

    [SerializeField]
    Text blinkText;

    [SerializeField]
    LeiaLoft.LeiaDisplay leiaDisplay;

    [SerializeField]
    Text backlightText;

    [SerializeField]
    Text interlacingText;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif
        //leiaDisplay.isBenchmarking = true;
    }


    public void ToggleTracking()
    {
        if(isTracking)
        {
            blinkObject.StopTracking();
            blinkText.text = "Face Tracking [Off]";
        }
        else
        {
            blinkObject.StartTracking();
            blinkText.text = "Face Tracking [On]";
        }

        isTracking = !isTracking;
    }

    public void ToggleBacklight()
    {
        if (leiaDisplay.LeiaDevice.GetBacklightMode() == 3)
        {
            leiaDisplay.LeiaDevice.SetBacklightMode(2);
            backlightText.text = "Backlight [Off]";
        }
        else
        {
            leiaDisplay.LeiaDevice.SetBacklightMode(3);
            backlightText.text = "Backlight [On]";
        }
    }

    public void ToggleInterlacing()
    {
        if(leiaDisplay.DesiredLightfieldMode == LeiaLoft.LeiaDisplay.LightfieldMode.On)
        {
            leiaDisplay.DesiredLightfieldMode = LeiaLoft.LeiaDisplay.LightfieldMode.Off;
            interlacingText.text = "Interlacing [Off]";
        }   
        else
        {
            leiaDisplay.DesiredLightfieldMode = LeiaLoft.LeiaDisplay.LightfieldMode.On;
            interlacingText.text = "Interlacing [On]";
        }

        
        
    }
}
