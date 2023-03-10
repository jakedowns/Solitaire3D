using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class DisplayConfigPanel : MonoBehaviour
{
    [SerializeField] private LeiaPersistentSettings leiaPersistentSettings;
    [SerializeField] private GameObject HWbrightnessPanel;
    [SerializeField] private InputField HWbrightnessInput;
    [SerializeField] private InputField numViewsInput;
    [SerializeField] private InputField centerViewInput;
    [SerializeField] private InputField colorInversionInput;
    [SerializeField] private InputField colorSlantInput;
    [SerializeField] private InputField dOverNInput;
    [SerializeField] private InputField nInput;
    [SerializeField] private InputField pOverDuInput;
    [SerializeField] private InputField pOverDvInput;
    [SerializeField] private InputField pixelPitchInput;
    [SerializeField] private InputField sInput;
    [SerializeField] private InputField thetaInput;
    [SerializeField] private InputField screenWidthInput;
    [SerializeField] private InputField screenHeightInput;
    [SerializeField] private Toggle BacklightToggle;
    DisplayConfig config
    {
        get
        {
            return LeiaDisplay.Instance.displayConfig;
        }
    }

    DisplayConfig unmodifiedConfig
    {
        get
        {
            return LeiaDisplay.Instance.GetUnmodifiedDisplayConfig();
        }
    }

    private void Start()
    {
#if !UNITY_ANDROID
        HWbrightnessPanel.SetActive(true);
#elif UNITY_ANDROID && !UNITY_EDITOR
        HWbrightnessPanel.SetActive(false);
#endif
        UpdateLabelsAndSliders();

        HWbrightnessInput.onValueChanged.AddListener(delegate { SetHWBrightness(HWbrightnessInput.text); });
        numViewsInput.onValueChanged.AddListener(delegate { SetNumViews(numViewsInput.text); });
        centerViewInput.onValueChanged.AddListener(delegate { SetCenterView(centerViewInput.text); });
        colorInversionInput.onValueChanged.AddListener(delegate { SetColorInversion(colorInversionInput.text); });
        colorSlantInput.onValueChanged.AddListener(delegate { SetColorSlant(colorSlantInput.text); });
        dOverNInput.onValueChanged.AddListener(delegate { SetDOverN(dOverNInput.text); });
        nInput.onValueChanged.AddListener(delegate { SetN(nInput.text); });
        pOverDuInput.onValueChanged.AddListener(delegate { SetPOverDu(pOverDuInput.text); });
        pOverDvInput.onValueChanged.AddListener(delegate { SetPOverDv(pOverDvInput.text); });
        pixelPitchInput.onValueChanged.AddListener(delegate { SetPixelPitch(pixelPitchInput.text); });
        sInput.onValueChanged.AddListener(delegate { SetS(sInput.text); });
        thetaInput.onValueChanged.AddListener(delegate { SetTheta(thetaInput.text); });
        screenWidthInput.onValueChanged.AddListener(delegate { SetScreenWidth(screenWidthInput.text); });
        screenHeightInput.onValueChanged.AddListener(delegate { SetScreenHeight(screenHeightInput.text); });
        BacklightToggle.onValueChanged.AddListener(SetBacklightEnabled);

    }

    public void Reset()
    {
        LeiaDisplay.Instance.displayConfig = DisplayConfig.CopyDisplayConfig(LeiaDisplay.Instance.GetUnmodifiedDisplayConfig());
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = LeiaDisplay.Instance.OriginalTimeDelay;
        LeiaDisplay.Instance.sdkConfig.act_singleTapCoef = LeiaDisplay.Instance.OriginalSingleTapActCoeff;
        LeiaDisplay.Instance.sdkConfig.numViews[0] = LeiaDisplay.Instance.displayConfig.NumViews.x;
        LeiaDisplay.Instance.sdkConfig.centerViewNumber = LeiaDisplay.Instance.displayConfig.CenterViewNumber;
        LeiaDisplay.Instance.sdkConfig.colorInversion = LeiaDisplay.Instance.displayConfig.colorInversion ? 1 : 0;
        LeiaDisplay.Instance.sdkConfig.colorSlant = LeiaDisplay.Instance.displayConfig.colorSlant;
        LeiaDisplay.Instance.sdkConfig.d_over_n = LeiaDisplay.Instance.displayConfig.d_over_n;
        LeiaDisplay.Instance.sdkConfig.n = LeiaDisplay.Instance.displayConfig.n;
        LeiaDisplay.Instance.sdkConfig.p_over_du = LeiaDisplay.Instance.displayConfig.p_over_du;
        LeiaDisplay.Instance.sdkConfig.p_over_dv = LeiaDisplay.Instance.displayConfig.p_over_dv;
        LeiaDisplay.Instance.sdkConfig.dotPitchInMM[0] = LeiaDisplay.Instance.displayConfig.PixelPitchInMM.x;
        LeiaDisplay.Instance.sdkConfig.s = LeiaDisplay.Instance.displayConfig.s;
        LeiaDisplay.Instance.sdkConfig.theta = LeiaDisplay.Instance.displayConfig.theta;
        LeiaDisplay.Instance.sdkConfig.panelResolution[0] = LeiaDisplay.Instance.displayConfig.PanelResolution.x;
        LeiaDisplay.Instance.sdkConfig.panelResolution[1] = LeiaDisplay.Instance.displayConfig.PanelResolution.y;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        UpdateLabelsAndSliders();
    }

    public void UpdateUI()
    {
        UpdateLabelsAndSliders();
    }

    void UpdateLabelsAndSliders()
    {
        numViewsInput.text = "" + config.NumViews.x;
        centerViewInput.text = "" + config.CenterViewNumber;
        colorInversionInput.text = "" + (config.colorInversion ? 1 : 0);
        colorSlantInput.text = "" + config.colorSlant;
        dOverNInput.text = "" + config.d_over_n;
        nInput.text = "" + config.n;
        pOverDuInput.text = "" + config.p_over_du;
        pOverDvInput.text = "" + config.p_over_dv;
        pixelPitchInput.text = "" + config.PixelPitchInMM.x;
        sInput.text = "" + config.s;
        thetaInput.text = "" + config.theta;
        screenWidthInput.text = "" + config.PanelResolution.x;
        screenHeightInput.text = "" + config.PanelResolution.y;
    }

    void SaveSettings()
    {
        leiaPersistentSettings.SaveSettings();
    }


    public void SetHWBrightness(string strValue)
    {
        int intValue;
        char charValue;

        if (int.TryParse(strValue, out intValue))
        {
            charValue = (char)intValue;
            char brightness = charValue;
            //SET BRIGHTNESS HERE
            LeiaDisplay.Instance.SetDisplayBrightness(brightness);
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetNumViews(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.NumViews.x = value;
            numViewsInput.text = "" + value;
            Debug.Log("config.NumViews.x = " + config.NumViews.x);
            Debug.Log("unmodifiedConfig.NumViews.x = " + unmodifiedConfig.NumViews.x);
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.numViews[0] = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
            LeiaDisplay.Instance.numViews = value;
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetCenterView(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.CenterViewNumber = value;
            centerViewInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.centerViewNumber = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetColorInversion(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.colorInversion = (value == 1);
            colorInversionInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.colorInversion = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetColorSlant(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.colorSlant = value;
            colorSlantInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.colorSlant = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetDOverN(string strValue)
    {
        float value;
        if (float.TryParse(strValue, out value))
        {
            config.d_over_n = value;
            dOverNInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.d_over_n = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetN(string strValue)
    {
        float value;
        if (float.TryParse(strValue, out value))
        {
            config.n = value;
            nInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.n = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetPOverDu(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.p_over_du = value;
            pOverDuInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.p_over_du = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetPOverDv(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.p_over_dv = value;
            pOverDvInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.p_over_dv = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetPixelPitch(string strValue)
    {
        float value;
        if (float.TryParse(strValue, out value))
        {
            config.PixelPitchInMM.x = value;
            pixelPitchInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.dotPitchInMM[0] = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetS(string strValue)
    {
        float value;
        if (float.TryParse(strValue, out value))
        {
            config.s = value;
            sInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.s = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetTheta(string strValue)
    {
        float value;
        if (float.TryParse(strValue, out value))
        {
            config.theta = value;
            thetaInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.theta = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetScreenWidth(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.DisplaySizeInMm.x = value;
            screenWidthInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.displaySizeInMm[0] = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void SetScreenHeight(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.DisplaySizeInMm.y = value;
            screenHeightInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.displaySizeInMm[1] = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementCenterView()
    {
        config.CenterViewNumber += .1f;
        centerViewInput.text = "" + config.CenterViewNumber;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.centerViewNumber = config.CenterViewNumber;
        centerViewInput.text = "" + LeiaDisplay.Instance.sdkConfig.centerViewNumber;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementCenterView()
    {
        config.CenterViewNumber -= .1f;
        centerViewInput.text = "" + config.CenterViewNumber;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.centerViewNumber = config.CenterViewNumber;
        centerViewInput.text = "" + LeiaDisplay.Instance.sdkConfig.centerViewNumber;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementNumView()
    {
        config.NumViews.x += 1;
        numViewsInput.text = "" + config.NumViews.x;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.numViews[0] = config.NumViews.x;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementNumView()
    {
        config.NumViews.x -= 1;
        numViewsInput.text = "" + config.NumViews.x;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.numViews[0] = config.NumViews.x;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementColorInversion()
    {
        if(!config.colorInversion)
        {
            config.colorInversion = true;
            colorInversionInput.text = "" + 1;

#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.colorInversion = 1;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementColorInversion()
    {
        if (config.colorInversion)
        {
            config.colorInversion = false;
            colorInversionInput.text = "" + 0;

#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.colorInversion = 0;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementColorSlant()
    {
        config.colorSlant += 1;
        colorSlantInput.text = "" + config.colorSlant;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.colorSlant = config.colorSlant;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementColorSlant()
    {
        config.colorSlant -= 1;
        colorSlantInput.text = "" + config.colorSlant;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.colorSlant = config.colorSlant;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementDOverN()
    {
        config.d_over_n += 0.01f;
        dOverNInput.text = "" + config.d_over_n;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.d_over_n = config.d_over_n;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementDOverN()
    {
        config.d_over_n -= 0.01f;
        dOverNInput.text = "" + config.d_over_n;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.d_over_n = config.d_over_n;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementN()
    {
        config.n += 0.1f;
        nInput.text = "" + config.n;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.n = config.n;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementN()
    {
        config.n -= 0.1f;
        nInput.text = "" + config.n;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.n = config.n;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    /*
     * 
    public void SetPOverDv(string strValue)
    {
        int value;
        if (int.TryParse(strValue, out value))
        {
            config.p_over_dv = value;
            pOverDvInput.text = "" + value;
#if UNITY_ANDROID && !UNITY_EDITOR
            LeiaDisplay.Instance.sdkConfig.p_over_dv = value;
            LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        }
        leiaPersistentSettings.SaveSettings();
    }
*/
    public void IncrementPOverDu()
    {
        config.p_over_du += 1;
        pOverDuInput.text = "" + config.p_over_du;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.p_over_du = config.p_over_du;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementPOverDu()
    {
        config.p_over_du -= 1;
        pOverDuInput.text = "" + config.p_over_du;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.p_over_du = config.p_over_du;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementPOverDv()
    {
        config.p_over_dv += 1;
        pOverDvInput.text = "" + config.p_over_dv;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.p_over_dv = config.p_over_dv;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementPOverDv()
    {
        config.p_over_dv -= 1;
        pOverDvInput.text = "" + config.p_over_dv;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.p_over_dv = config.p_over_dv;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementS()
    {
        config.s += 0.05f;
        sInput.text = "" + config.s;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.s = config.s;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementS()
    {
        config.s -= 0.05f;
        sInput.text = "" + config.s;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.s = config.s;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void IncrementTheta()
    {
        config.theta += 0.05f;
        thetaInput.text = "" + config.theta;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.theta = config.theta;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void DecrementTheta()
    {
        config.theta -= 0.05f;
        thetaInput.text = "" + config.theta;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.theta = config.theta;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        leiaPersistentSettings.SaveSettings();
    }

    public void SetBacklightEnabled(bool enabled)
    {
        LeiaDisplay.Instance.SetBacklightEnabled(enabled);
        leiaPersistentSettings.SaveSettings();
    }
}
