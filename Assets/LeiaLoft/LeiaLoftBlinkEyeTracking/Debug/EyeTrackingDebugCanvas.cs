using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class EyeTrackingDebugCanvas : MonoBehaviour
{
    [SerializeField] private GameObject EyeTrackingStatusBar;
    [SerializeField] private Text textLabel;
    BlinkTrackingUnityPlugin blink;

    [SerializeField] private GameObject logoScene;
    [SerializeField] private GameObject patternMediaViewer;

    [SerializeField] private GameObject debugPanel;

    LeiaDisplay leiaDisplay;

    [SerializeField] private EyeTrackingModeDropdown eyeTrackingMode;

    // Start is called before the first frame update

    void Start()
    {
        leiaDisplay = FindObjectOfType<LeiaDisplay>();
        blink = FindObjectOfType<BlinkTrackingUnityPlugin>();
        Invoke("UpdateStatus", 1f);
    }

    public void ShowPattern(bool visible)
    {
        if (logoScene != null)
        {
            logoScene.SetActive(!visible);
        }
        if (patternMediaViewer != null)
        {
            patternMediaViewer.SetActive(visible);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Invoke("EnableShifter", .5f);
            if (!leiaDisplay.viewPeeling)
            {
                eyeTrackingMode.SetViewPeelingMode();
            }
            else
            {
                eyeTrackingMode.SetTrackedStereoMode();
            }
        }
    }

    void UpdateStatus()
    {
        Invoke("UpdateStatus", .1f);

        if (Application.isEditor)
        {
            textLabel.text = "Eye tracking not supported in editor. Make a build to test with eye tracking.";
            EyeTrackingStatusBar.SetActive(true && leiaDisplay.eyeTrackingStatusBarEnabled);
        }
        else
        if (!blink.CameraConnected)
        {
            textLabel.text = "Eye tracking camera not connected. Check the USB connection.";
            EyeTrackingStatusBar.SetActive(true && leiaDisplay.eyeTrackingStatusBarEnabled);
        }
        else if (blink.NumFaces == 0)
        {
            textLabel.text = "No faces detected.";
            EyeTrackingStatusBar.SetActive(true && leiaDisplay.eyeTrackingStatusBarEnabled);
        }
        else
        {
            EyeTrackingStatusBar.SetActive(false);
        }
    }
}
