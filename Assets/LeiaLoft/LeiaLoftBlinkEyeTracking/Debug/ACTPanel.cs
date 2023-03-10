using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ACTPanel : MonoBehaviour
{
    [SerializeField] private LeiaPersistentSettings leiaPersistentSettings;
    [SerializeField] private Dropdown actModeDropdown;
    [SerializeField] private GameObject MultiViewActPanel;
    [SerializeField] private GameObject SingleTapActPanel;

    [SerializeField] private Slider[] ACT_Sliders;

    [SerializeField] private Text ACT_A_Label;
    [SerializeField] private Text ACT_B_Label;
    [SerializeField] private Text ACT_C_Label;
    [SerializeField] private Text ACT_D_Label;
    [SerializeField] private Text ACT_E_Label;
    [SerializeField] private Text ACT_F_Label;

    [SerializeField] private Slider SingleTapCoeffSlider;
    [SerializeField] private Text SingleTapCoeffLabel;
    [SerializeField] private Button ResetCoeffButton;
    [SerializeField] private Button ResetSingtapCoeffButton;

    DisplayConfig config
    {
        get
        {
            return LeiaDisplay.Instance.GetDisplayConfig();
        }
    }
    
#if UNITY_ANDROID && !UNITY_EDITOR
    Leia.Config sdkConfig
    {
        get
        {
            return LeiaDisplay.Instance.sdkConfig;
        }
    }
#endif
    private void Start()
    {
        actModeDropdown.onValueChanged.AddListener(delegate{ OnACTModeDropdownValueChanged(); });
        SingleTapCoeffSlider.onValueChanged.AddListener(delegate{ SetSingleTapActCoeff(SingleTapCoeffSlider.value); });
        ResetCoeffButton.onClick.AddListener(ResetActCoeff);
        ResetSingtapCoeffButton.onClick.AddListener(ResetActCoeff);

        UpdateUI();
    }
    
    void OnACTModeDropdownValueChanged()
    {
        LeiaDisplay.Instance.ActMode = (LeiaDisplay.ACTMODE) actModeDropdown.value;
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void UpdateUI()
    {
        UpdateDropdownAndSliders();
        UpdateLabels();
    }

    void UpdateDropdownAndSliders()
    {
        switch(LeiaDisplay.Instance.ActMode)
        {
            case LeiaDisplay.ACTMODE.SINGLETAP:
                actModeDropdown.value = 0;
                break;
            case LeiaDisplay.ACTMODE.MULTIVIEW:
                actModeDropdown.value = 1;
                break;
            case LeiaDisplay.ACTMODE.OFF:
                actModeDropdown.value = 2;
                break;
            default:
                actModeDropdown.value = 0;
                break;
        }

        SingleTapActPanel.SetActive(actModeDropdown.value == 0);
        MultiViewActPanel.SetActive(actModeDropdown.value == 1);

        if (LeiaDisplay.Instance.ActMode == LeiaDisplay.ACTMODE.MULTIVIEW)
        {
            LeiaDisplay.Instance.SetACTEnabled(true);
        }
        else if (LeiaDisplay.Instance.ActMode == LeiaDisplay.ACTMODE.SINGLETAP)
        {
            LeiaDisplay.Instance.SetACTEnabled(true);
#if !UNITY_ANDROID
            SingleTapCoeffSlider.SetValueWithoutNotify(config.SingleTapActCoefficient);
#elif UNITY_ANDROID && !UNITY_EDITOR
            SingleTapCoeffSlider.SetValueWithoutNotify(sdkConfig.act_singleTapCoef);
#endif
        }
        else if (LeiaDisplay.Instance.ActMode == LeiaDisplay.ACTMODE.OFF)
        {
            LeiaDisplay.Instance.SetACTEnabled(false);
        }

        for (int i = 0; i < ACT_Sliders.Length; i++)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (i < config.UserActCoefficients[0].Count + config.UserActCoefficients[1].Count)
            {
                ACT_Sliders[i].interactable = true;
                if(i < config.UserActCoefficients[0].Count)
                {
                    ACT_Sliders[i].SetValueWithoutNotify(LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][i]);
                }
                else if (i >= config.UserActCoefficients[0].Count && i < config.UserActCoefficients[0].Count + config.UserActCoefficients[1].Count)
                {
                    ACT_Sliders[i].SetValueWithoutNotify(LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[1][i- config.UserActCoefficients[0].Count]);
                }
            }
            else
            {
                ACT_Sliders[i].interactable = false;
            }
#elif !UNITY_ANDROID
            if (i < config.UserActCoefficients[0].Count)
            {
                ACT_Sliders[i].interactable = true;
                if(i < config.UserActCoefficients[0].Count)
                {
                    ACT_Sliders[i].SetValueWithoutNotify(LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][i]);
                }
            }
            else
            {
                ACT_Sliders[i].interactable = false;
            }
#endif
        }

    }
    void UpdateLabels()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if(config.UserActCoefficients != null && config.UserActCoefficients[0] != null && config.UserActCoefficients[0].Count >= 2)
        {
            ACT_A_Label.text = "ACT-A: " + config.UserActCoefficients[0][0];
            ACT_B_Label.text = "ACT-B: " + config.UserActCoefficients[0][1];
        }

        if (config.UserActCoefficients != null && config.UserActCoefficients[1] != null && config.UserActCoefficients[1].Count >= 2)
        {
            ACT_C_Label.text = "ACT-C: " + config.UserActCoefficients[1][0];
            ACT_D_Label.text = "ACT-D: " + config.UserActCoefficients[1][1];
        }
        SingleTapCoeffLabel.text = "Single Tap ACT Coef: " + LeiaDisplay.Instance.sdkConfig.act_singleTapCoef.ToString();
#endif
#if !UNITY_ANDROID

        if (config.UserActCoefficients != null && config.UserActCoefficients[0] != null)
        {
            if (config.UserActCoefficients[0].Count >= 4)
            {
                ACT_A_Label.text = "ACT-A: " + config.UserActCoefficients[0][0];
                ACT_B_Label.text = "ACT-B: " + config.UserActCoefficients[0][1];
                ACT_C_Label.text = "ACT-C: " + config.UserActCoefficients[0][2];
                ACT_D_Label.text = "ACT-D: " + config.UserActCoefficients[0][3];
            }
            if(config.UserActCoefficients[0].Count == 6)
            {
                ACT_E_Label.text = "ACT-E: " + config.UserActCoefficients[0][4];
                ACT_F_Label.text = "ACT-F: " + config.UserActCoefficients[0][5];
            }
        }

        SingleTapCoeffLabel.text = "Single Tap ACT Coef: " + config.SingleTapActCoefficient.ToString();
#endif

    }

    void SetSingleTapActCoeff(float newVal)
    {
        LeiaDisplay.Instance.GetDisplayConfig().SingleTapActCoefficient = newVal;
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.act_singleTapCoef = newVal;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    void ResetActCoeff()
    {
        LeiaDisplay.Instance.displayConfig = DisplayConfig.CopyDisplayConfig(LeiaDisplay.Instance.GetUnmodifiedDisplayConfig());
        LeiaDisplay.Instance.GetDisplayConfig().SingleTapActCoefficient = LeiaDisplay.Instance.OriginalSingleTapActCoeff;

#if UNITY_ANDROID && !UNITY_EDITOR

        Leia.Config newConfig = new Leia.Config();
        LeiaDisplay.Instance.sdkConfig = newConfig;
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = LeiaDisplay.Instance.OriginalTimeDelay;
        LeiaDisplay.Instance.sdkConfig.act_singleTapCoef = LeiaDisplay.Instance.OriginalSingleTapActCoeff;
        LeiaDisplay.Instance.sdkConfig.act_beta = LeiaDisplay.Instance.GetDisplayConfig().Beta;
        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACT_A(float newVal)
    {
#if UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][0] = newVal;
#elif !UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][0] = newVal;
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACT_B(float newVal)
    {
#if UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][1] = newVal;
#elif !UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][1] = newVal;
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACT_C(float newVal)
    {
#if UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[1][0] = newVal;
#elif !UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][2] = newVal;
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACT_D(float newVal)
    {
#if UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[1][1] = newVal;
#elif !UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][3] = newVal;
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACT_E(float newVal)
    {
#if !UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][4] = newVal;
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

    public void SetACT_F(float newVal)
    {
#if !UNITY_ANDROID
        LeiaDisplay.Instance.GetDisplayConfig().UserActCoefficients[0][5] = newVal;
#endif
        UpdateUI();
        leiaPersistentSettings.SaveSettings();
    }

}
