using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class EyeTrackingSettingsPanel : MonoBehaviour
{
    [SerializeField] private LeiaPersistentSettings leiaPersistentSettings;
    [SerializeField] private Toggle BlackViewsToggle;
    [SerializeField] private Toggle CameraZShiftToggle;
    [SerializeField] private Toggle CloseRangeSafetyToggle;
    [SerializeField] private Toggle EyeTrackingToggle;
    [SerializeField] private Toggle EyeTrackingStatusBarToggle;
    [SerializeField] private InputField DelayInputField;
    [SerializeField] private Slider DelaySlider;
    [SerializeField] private InputField FaceXInputField;
    [SerializeField] private InputField FaceYInputField;
    [SerializeField] private InputField FaceZInputField;
    [SerializeField] private Button DelayResetButton;

    private void Start()
    {
        FaceXInputField.onValueChanged.AddListener(delegate { SetSimulatedFaceX(FaceXInputField.text); });
        FaceYInputField.onValueChanged.AddListener(delegate { SetSimulatedFaceY(FaceYInputField.text); });
        FaceZInputField.onValueChanged.AddListener(delegate { SetSimulatedFaceZ(FaceZInputField.text); });

        DelaySlider.onValueChanged.AddListener(x => SetTimeDelay(x));
        DelaySlider.onValueChanged.AddListener(delegate { UpdateDelayInputField(); });
        DelaySlider.onValueChanged.AddListener(delegate { UpdateDelaySlider(); });
        DelayInputField.onEndEdit.AddListener(x => SetTimeDelay(x));
        DelayInputField.onEndEdit.AddListener(delegate { UpdateDelayInputField(); });
        DelayInputField.onEndEdit.AddListener(delegate { UpdateDelaySlider(); });

        DelayResetButton.onClick.AddListener(ResetTimeDelay);

        UpdateUI();
    }

    public void UpdateUI()
    {
        UpdateLabelsAndSliders();
    }

    void UpdateLabelsAndSliders()
    {
        FaceXInputField.interactable = !LeiaDisplay.Instance.tracker.enabled;
        FaceYInputField.interactable = !LeiaDisplay.Instance.tracker.enabled;
        FaceZInputField.interactable = !LeiaDisplay.Instance.tracker.enabled;

        if (BlackViewsToggle != null)
        {
            BlackViewsToggle.SetIsOnWithoutNotify(LeiaDisplay.Instance.blackViews);
        }
        if (CameraZShiftToggle != null)
        {
            CameraZShiftToggle.SetIsOnWithoutNotify(LeiaCamera.Instance.cameraZaxisMovement);
        }
        if (EyeTrackingToggle != null)
        {
            EyeTrackingToggle.SetIsOnWithoutNotify(LeiaDisplay.Instance.tracker.enabled);
        }
        if (CloseRangeSafetyToggle != null)
        {
            CloseRangeSafetyToggle.SetIsOnWithoutNotify(LeiaDisplay.Instance.CloseRangeSafety);
        }
        if (EyeTrackingStatusBarToggle != null)
        {
            EyeTrackingStatusBarToggle.SetIsOnWithoutNotify(LeiaDisplay.Instance.eyeTrackingStatusBarEnabled);
        }

        UpdateDelaySlider();
        UpdateDelayInputField();
    }

    private void UpdateDelaySlider()
    {

#if !UNITY_ANDROID
    if (DelaySlider != null)
            DelaySlider.SetValueWithoutNotify(LeiaDisplay.Instance.displayConfig.timeDelay);
#elif UNITY_ANDROID && !UNITY_EDITOR
    if (DelaySlider != null)
            DelaySlider.SetValueWithoutNotify(LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
#endif
    }

    private void UpdateDelayInputField()
    {
#if !UNITY_ANDROID
        DelayInputField.text = "" + LeiaDisplay.Instance.displayConfig.timeDelay;
#elif UNITY_ANDROID && !UNITY_EDITOR
        DelayInputField.text = "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs;
#endif
    }

    LeiaCamera[] _leiaCameras;
    LeiaCamera[] leiaCameras
    {
        get
        {
            if (_leiaCameras == null)
            {
                _leiaCameras = FindObjectsOfType<LeiaCamera>();
            }
            return _leiaCameras;
        }
    }

    void Update()
    {
        if (LeiaDisplay.Instance.tracker.enabled)
        {
            FaceXInputField.SetTextWithoutNotify("" + LeiaDisplay.Instance.viewerPosition.x);
            FaceYInputField.SetTextWithoutNotify("" + LeiaDisplay.Instance.viewerPosition.y);
            FaceZInputField.SetTextWithoutNotify("" + LeiaDisplay.Instance.viewerPosition.z);
        }
    }


    public void SetEyeTrackingEnabled(bool enabled)
    {
        LeiaDisplay.Instance.SetTrackerEnabled(enabled);
        LeiaDisplay.Instance.UsingSimulatedFacePosition = !enabled;
        if (LeiaDisplay.Instance.UsingSimulatedFacePosition)
        {
            LeiaDisplay.Instance.viewerPosition.x = 0;
            LeiaDisplay.Instance.viewerPosition.y = 0;
            LeiaDisplay.Instance.viewerPosition.z = LeiaDisplay.Instance.GetDisplayConfig().ConvergenceDistance;

            FaceXInputField.SetTextWithoutNotify("" + LeiaDisplay.Instance.viewerPosition.x);
            FaceYInputField.SetTextWithoutNotify("" + LeiaDisplay.Instance.viewerPosition.y);
            FaceZInputField.SetTextWithoutNotify("" + LeiaDisplay.Instance.viewerPosition.z);
        }

        FaceXInputField.interactable = !LeiaDisplay.Instance.tracker.enabled;
        FaceYInputField.interactable = !LeiaDisplay.Instance.tracker.enabled;
        FaceZInputField.interactable = !LeiaDisplay.Instance.tracker.enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetBlackViewsEnabled(bool enabled)
    {
        LeiaDisplay.Instance.blackViews = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetCloseRangeSafetyEnabled(bool enabled)
    {
        LeiaDisplay.Instance.CloseRangeSafety = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetZParallaxEnabled(bool enabled)
    {
        LeiaCamera[] leiaCameras = FindObjectsOfType<LeiaCamera>();
        int count = leiaCameras.Length;

        for (int i = 0; i < count; i++)
        {
            leiaCameras[i].cameraZaxisMovement = enabled;
        }

        leiaPersistentSettings.SaveSettings();
    }

    public void SetCameraShiftEnabled(bool enabled)
    {
        LeiaDisplay.Instance.CameraShiftEnabled = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetShaderShiftEnabled(bool enabled)
    {
        LeiaDisplay.Instance.ShaderShiftEnabled = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetSimulatedFaceX(string faceXString)
    {
        if (LeiaDisplay.Instance.tracker.enabled)
        {
            Debug.Log("Should not get here 0!");
            return;
        }
        float faceX;
        if (float.TryParse(faceXString, out faceX))
        {
            LeiaDisplay.Instance.SimulatedFaceX = faceX;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetSimulatedFaceY(string faceYString)
    {
        if (LeiaDisplay.Instance.tracker.enabled)
        {
            Debug.Log("Should not get here 1!");
            return;
        }

        float faceY;
        if (float.TryParse(faceYString, out faceY))
        {
            LeiaDisplay.Instance.SimulatedFaceY = faceY;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetSimulatedFaceZ(string faceZString)
    {
        if (LeiaDisplay.Instance.tracker.enabled)
        {
            Debug.Log("Should not get here 2!");
            return;
        }

        float faceZ;
        if (float.TryParse(faceZString, out faceZ))
        {
            LeiaDisplay.Instance.SimulatedFaceZ = faceZ;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetCameraShiftScale(float CameraShiftScale)
    {
        int count = leiaCameras.Length;

        Debug.Log("SetCameraShiftScale to " + CameraShiftScale);
        for (int i = 0; i < count; i++)
        {
            leiaCameras[i].CameraShiftScaling = CameraShiftScale;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetEyeTrackingStatusBarEnabled(bool enabled)
    {
        LeiaDisplay.Instance.eyeTrackingStatusBarEnabled = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetTimeDelay(string timeDelay)
    {
        LeiaDisplay.Instance.displayConfig.timeDelay = float.Parse(timeDelay);
#if UNITY_ANDROID && !UNITY_EDITOR        
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = float.Parse(timeDelay);
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void SetTimeDelay(float timeDelay)
    {
        LeiaDisplay.Instance.displayConfig.timeDelay = timeDelay;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = timeDelay;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void ResetTimeDelay()
    {
        LeiaDisplay.Instance.displayConfig.timeDelay = LeiaDisplay.Instance.OriginalTimeDelay;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = LeiaDisplay.Instance.OriginalTimeDelay;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        DelaySlider.SetValueWithoutNotify(LeiaDisplay.Instance.OriginalTimeDelay);
        UpdateDelayInputField();

        leiaPersistentSettings.SaveSettings();
    }
}
