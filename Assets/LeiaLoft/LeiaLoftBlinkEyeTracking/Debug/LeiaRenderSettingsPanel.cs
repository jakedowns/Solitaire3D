using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class LeiaRenderSettingsPanel : MonoBehaviour
{
    [SerializeField] private LeiaPersistentSettings leiaPersistentSettings;
    [SerializeField] private Toggle AntiAliasingToggle;
    [SerializeField] private Toggle R0TestToggle;
    [SerializeField] private InputField baselineInput;
    [SerializeField] private InputField cameraShiftInput;
    [SerializeField] private Slider baselineSlider;
    [SerializeField] private Slider cameraShiftSlider;
    [SerializeField] private Slider SWBrightnessSlider;
    [SerializeField] private InputField tileWidthInput;
    [SerializeField] private InputField tileHeightInput;
    [SerializeField] private InputField SWBrightnessInput;
    [SerializeField] private Button ResetSWBrightnessButton;
    [SerializeField] private Button ResetTileWidthButton;
    [SerializeField] private Button ResetTileHeightButton;
    [SerializeField] private Text CurrentTrackingBackend;
    [SerializeField] private Button SetGPUButton;
    [SerializeField] private Button SetCPUButton;
    [SerializeField] private GameObject trackingBackendPanel;
    [SerializeField] private PerformanceStatsPanel performanceStatsPanel;
    LeiaCamera _leiaCamera;
    LeiaCamera leiaCamera
    {
        get
        {
            if (_leiaCamera == null)
            {
                _leiaCamera = FindObjectOfType<LeiaCamera>();
            }
            return _leiaCamera;
        }
    }

    void Start()
    {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        trackingBackendPanel.SetActive(false);
#endif
/// </remove_from_public>

#if UNITY_ANDROID && !UNITY_EDITOR
        if(LeiaDisplay.Instance.tracker.faceTrackingBackend == Leia.FaceDetectorBackend.CPU)
        {
            CurrentTrackingBackend.text = "Current Backend: CPU";
        }
        else if (LeiaDisplay.Instance.tracker.faceTrackingBackend == Leia.FaceDetectorBackend.GPU)
        {
            CurrentTrackingBackend.text = "Current Backend: GPU";
        }
        else
        {
            CurrentTrackingBackend.text = "Current Backend: UnKnown";
        }
#endif
        UpdateLabelsAndSliders();
        InitializeInputsAndSliders();
    }

    public void UpdateUI()
    {
        UpdateLabelsAndSliders();
    }

    void InitializeInputsAndSliders()
    {
        baselineInput.onValueChanged.AddListener(delegate { SetBaselineScaling(baselineInput.text); });
        cameraShiftInput.onValueChanged.AddListener(delegate { SetCameraShiftScaling(cameraShiftInput.text); });
        SWBrightnessInput.onValueChanged.AddListener(delegate { SetSWBrightness(SWBrightnessInput.text); });
        tileWidthInput.onValueChanged.AddListener(SetTileWidth);
        tileHeightInput.onValueChanged.AddListener(SetTileHeight);

        ResetTileWidthButton.onClick.AddListener(ResetTileWidth);
        ResetTileHeightButton.onClick.AddListener(ResetTileHeight);
        ResetSWBrightnessButton.onClick.AddListener(ResetSWBrightness);
#if UNITY_ANDROID && !UNITY_EDITOR
        SetGPUButton.onClick.AddListener(SetGPU);
        SetCPUButton.onClick.AddListener(SetCPU);
#endif
    }
    void UpdateLabelsAndSliders()
    {
        baselineInput.text = "" + leiaCamera.BaselineScaling;
        cameraShiftInput.text = "" + leiaCamera.CameraShiftScaling;
        SWBrightnessInput.text = "" + LeiaDisplay.Instance.SWBrightness;
/// <remove_from_public>
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        tileWidthInput.text = "" + LeiaDisplay.Instance.displayConfig.ViewResolution.x;
        tileHeightInput.text = "" + LeiaDisplay.Instance.displayConfig.ViewResolution.y;
#endif
/// </remove_from_public>
#if UNITY_ANDROID && !UNITY_EDITOR
        tileWidthInput.text = "" + LeiaDisplay.Instance.sdkConfig.viewResolution[0];
        tileHeightInput.text = "" + LeiaDisplay.Instance.sdkConfig.viewResolution[1];
#endif
        baselineSlider.SetValueWithoutNotify(leiaCamera.BaselineScaling);
        cameraShiftSlider.SetValueWithoutNotify(leiaCamera.CameraShiftScaling);
        SWBrightnessSlider.SetValueWithoutNotify(LeiaDisplay.Instance.SWBrightness);
    }

    public void SetTileWidth(string tileWidthStr)
    {
        int tileWidth;
        if (int.TryParse(tileWidthStr, out tileWidth))
        {
            LeiaDisplay.Instance.displayConfig.ViewResolution.x = tileWidth;
            LeiaDisplay.Instance.displayConfig.UserViewResolution.x = tileWidth;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.viewResolution[0] = tileWidth;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
            tileWidthInput.text = "" + tileWidth;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetTileHeight(string tileHeightStr)
    {
        int tileHeight;
        if (int.TryParse(tileHeightStr, out tileHeight))
        {
            LeiaDisplay.Instance.displayConfig.ViewResolution.y = tileHeight;
            LeiaDisplay.Instance.displayConfig.UserViewResolution.y = tileHeight;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.viewResolution[1] = tileHeight;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
            tileHeightInput.text = "" + tileHeight;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void ResetTileWidth()
    {
        /// <remove_from_public>
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        LeiaDisplay.Instance.displayConfig.ViewResolution.x = LeiaDisplay.Instance.GetUnmodifiedDisplayConfig().ViewResolution.x;
        LeiaDisplay.Instance.displayConfig.UserViewResolution.x = LeiaDisplay.Instance.GetUnmodifiedDisplayConfig().ViewResolution.x;
        tileWidthInput.text = "" + LeiaDisplay.Instance.displayConfig.ViewResolution.x;
#endif
        /// </remove_from_public>
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.viewResolution[0] = LeiaDisplay.Instance.displayConfig.ViewResolution.x;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
        tileWidthInput.text = "" + LeiaDisplay.Instance.sdkConfig.viewResolution[0];
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void ResetTileHeight()
    {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        LeiaDisplay.Instance.displayConfig.ViewResolution.y = LeiaDisplay.Instance.GetUnmodifiedDisplayConfig().ViewResolution.y;
        LeiaDisplay.Instance.displayConfig.UserViewResolution.y = LeiaDisplay.Instance.GetUnmodifiedDisplayConfig().ViewResolution.y;
        tileHeightInput.text = "" + LeiaDisplay.Instance.displayConfig.ViewResolution.y;
#endif
/// </remove_from_public>
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.viewResolution[1] = LeiaDisplay.Instance.displayConfig.ViewResolution.y;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
        tileHeightInput.text = "" + LeiaDisplay.Instance.sdkConfig.viewResolution[1];
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void SetPerPixelCorrectionEnabled(bool enabled)
    {
        LeiaDisplay.Instance.PerPixelCorrectionEnabled = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACTEnabled(bool enabled)
    {
        LeiaDisplay.Instance.ACTEnabled = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetAntiAliasingEnabled(bool enabled)
    {
        LeiaDisplay.Instance.AntiAliasing = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetR0TestEnabled(bool enabled)
    {
        LeiaDisplay.Instance.ShowR0Test = enabled;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetRoundViewPeeling(bool enabled)
    {
        LeiaDisplay.Instance.SetRoundViewPeel(enabled);
        leiaPersistentSettings.SaveSettings();
    }

    public void SetBaselineScaling(float baselineScaling)
    {
        leiaCamera.BaselineScaling = baselineScaling;
        baselineInput.text = "" + baselineScaling;
        baselineSlider.SetValueWithoutNotify(baselineScaling);
        leiaPersistentSettings.SaveSettings();
    }

    public void SetCameraShiftScaling(float cameraShiftScaling)
    {
        leiaCamera.CameraShiftScaling = cameraShiftScaling;
        cameraShiftInput.text = "" + cameraShiftScaling;
        cameraShiftSlider.SetValueWithoutNotify(cameraShiftScaling);
        leiaPersistentSettings.SaveSettings();
    }

    public void SetBaselineScaling(string baselineScalingStr)
    {
        float baselineScaling;
        if (float.TryParse(baselineScalingStr, out baselineScaling))
        {
            baselineScaling = Round(baselineScaling, 2);
            SetBaselineScaling(baselineScaling);
        }
    }

    public void SetCameraShiftScaling(string cameraShiftScalingStr)
    {
        float cameraShiftScaling;
        if (float.TryParse(cameraShiftScalingStr, out cameraShiftScaling))
        {
            cameraShiftScaling = Round(cameraShiftScaling, 2);
            SetCameraShiftScaling(cameraShiftScaling);
        }
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    public void SetSWBrightness(float value)
    {
        LeiaDisplay.Instance.SWBrightness = value;
        SWBrightnessInput.text = "" + value;
        leiaPersistentSettings.SaveSettings();
    }

    public void SetSWBrightness(string value)
    {
        float brightness;
        if (float.TryParse(value, out brightness))
        {
            brightness = Round(brightness, 2);
            SetSWBrightness(brightness);
            SWBrightnessSlider.SetValueWithoutNotify(brightness);
        }

    }

    public void ResetSWBrightness()
    {
        SetSWBrightness(1.0f);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public void SetGPU()
    {
        SetFaceTrackingBackend(Leia.FaceDetectorBackend.GPU);
        CurrentTrackingBackend.text = "Current Backend: GPU";
    }
    public void SetCPU()
    {
        SetFaceTrackingBackend(Leia.FaceDetectorBackend.CPU);
        CurrentTrackingBackend.text = "Current Backend: CPU";
    }
    private void SetFaceTrackingBackend(Leia.FaceDetectorBackend backend)
    {
        Leia.FaceDetectorConfig config = new Leia.FaceDetectorConfig();
        config.backend = backend;
        config.inputType = Leia.FaceDetectorInputType.Unknown;
        LeiaDisplay.Instance.CNSDK.SetFaceTrackingConfig(config);
    }
#endif
}
