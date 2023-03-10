using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LeiaPersistentSettings : MonoBehaviour
{
    Dictionary<string, string> persistentSettings;

    [SerializeField] private LeiaRenderSettingsPanel leiaRenderSettingsPanel;
    [SerializeField] private EyeTrackingSettingsPanel eyeTrackingSettingsPanel;
    [SerializeField] private ACTPanel actPanel;
    [SerializeField] private DisplayConfigPanel displayConfigPanel;

    string settingsFilename;

    string letters = "ABCDEFG";

    void Start()
    {
        settingsFilename = Application.persistentDataPath + "/" + "LeiaPersistentSettings.txt";
        DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
        persistentSettings = new Dictionary<string, string>();
        AddPersistentSetting("R0Test", "" + LeiaDisplay.Instance.ShowR0Test);
        AddPersistentSetting("BlackViews", "" + LeiaDisplay.Instance.blackViews);
        AddPersistentSetting("CloseRangeSafety", "" + LeiaDisplay.Instance.CloseRangeSafety);
        AddPersistentSetting("ZParallax", "" + LeiaCamera.Instance.cameraZaxisMovement);
        AddPersistentSetting("ShowTrackingStatusBar", "" + LeiaDisplay.Instance.eyeTrackingStatusBarEnabled);
#if !UNITY_ANDROID
        AddPersistentSetting("Delay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
        AddPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
        AddPersistentSetting("SingleTapActCoeff", "" + LeiaDisplay.Instance.displayConfig.SingleTapActCoefficient);
        AddPersistentSetting("OriginalSingleTapActCoeff", "" + LeiaDisplay.Instance.displayConfig.SingleTapActCoefficient);

        int count = config.UserActCoefficients[0].Count;
        for (int i = 0; i < count; i++)
        {
            AddPersistentSetting("ACT-" + letters[i], "" + config.UserActCoefficients[0][i]);
        }

#elif UNITY_ANDROID && !UNITY_EDITOR
        AddPersistentSetting("Delay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
        AddPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
        AddPersistentSetting("SingleTapActCoeff", "" + LeiaDisplay.Instance.sdkConfig.act_singleTapCoef);
        AddPersistentSetting("OriginalSingleTapActCoeff", "" + LeiaDisplay.Instance.sdkConfig.act_singleTapCoef);
        
        int count = config.UserActCoefficients[0].Count;
        for (int i = 0; i < count; i++)
        {
            AddPersistentSetting("ACT-" + letters[i], "" + config.UserActCoefficients[0][i]);
        }
        for (int i = 0; i < config.UserActCoefficients[1].Count; i++)
        {
            AddPersistentSetting("ACT-" + letters[i + count], "" + config.UserActCoefficients[1][i]);
        }
#endif

        AddPersistentSetting("ACTMode", "" + LeiaDisplay.Instance.ActMode);
        AddPersistentSetting("NumViews", "" + config.NumViews.x);
        AddPersistentSetting("CenterView", "" + config.CenterViewNumber);
        AddPersistentSetting("ColorInversion", "" + config.colorInversion);
        AddPersistentSetting("ColorSlant", "" + config.colorSlant);
        AddPersistentSetting("d_over_n", "" + config.d_over_n);
        AddPersistentSetting("n", "" + config.n);
        AddPersistentSetting("p_over_du", "" + config.p_over_du);
        AddPersistentSetting("p_over_dv", "" + config.p_over_dv);
        AddPersistentSetting("pixelPitch", "" + config.PixelPitchInMM.x);
        AddPersistentSetting("s", "" + config.s);
        AddPersistentSetting("theta", "" + config.theta);
        AddPersistentSetting("ScreenWidth", "" + config.PanelResolution.x);
        AddPersistentSetting("ScreenHeight", "" + config.PanelResolution.y);
        AddPersistentSetting("BaselineScaling", "" + LeiaCamera.Instance.BaselineScaling);
        AddPersistentSetting("CameraShiftScaling", "" + LeiaCamera.Instance.CameraShiftScaling);
        AddPersistentSetting("SWBrightness", "" + LeiaDisplay.Instance.SWBrightness);

        if (!File.Exists(settingsFilename))
        {
            SaveSettings();
        }
#if !UNITY_EDITOR
        if (File.Exists(settingsFilename))
        {
            LoadSettings();
        }
#endif
    }

    void GetPersistentSettingsFromUI()
    {
        if (persistentSettings == null)
        {
            Debug.Log("persistentSettings is null.");
            return;
        }
        DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
        SetPersistentSetting("R0Test", "" + LeiaDisplay.Instance.ShowR0Test);
        SetPersistentSetting("BlackViews", "" + LeiaDisplay.Instance.blackViews);
        SetPersistentSetting("CloseRangeSafety", "" + LeiaDisplay.Instance.CloseRangeSafety);
        SetPersistentSetting("ZParallax", "" + LeiaCamera.Instance.cameraZaxisMovement);
        SetPersistentSetting("ShowTrackingStatusBar", "" + LeiaDisplay.Instance.eyeTrackingStatusBarEnabled);
#if !UNITY_ANDROID
        SetPersistentSetting("Delay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
        SetPersistentSetting("SingleTapActCoeff", "" + LeiaDisplay.Instance.displayConfig.SingleTapActCoefficient);
#elif UNITY_ANDROID && !UNITY_EDITOR
        SetPersistentSetting("Delay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
        SetPersistentSetting("SingleTapActCoeff", "" + LeiaDisplay.Instance.sdkConfig.act_singleTapCoef);
#endif
        if (!persistentSettings.ContainsKey("OriginalTimeDelay"))
        {
#if !UNITY_ANDROID
            SetPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.displayConfig.timeDelay);
#elif UNITY_ANDROID && !UNITY_EDITOR
            SetPersistentSetting("OriginalTimeDelay", "" + LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs);
#endif
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        int count = config.UserActCoefficients[0].Count;
        for (int i = 0; i < count; i++)
        {
            SetPersistentSetting("ACT-" + letters[i], "" + config.UserActCoefficients[0][i]);
        }
        for (int i = 0; i < config.UserActCoefficients[1].Count; i++)
        {
            SetPersistentSetting("ACT-" + letters[i+count], "" + config.UserActCoefficients[1][i]);
        }
#elif !UNITY_ANDROID
        int count = config.UserActCoefficients[0].Count;
        for (int i = 0; i < count; i++)
        {
            SetPersistentSetting("ACT-" + letters[i], "" + config.UserActCoefficients[0][i]);
        }
#endif
        if (!persistentSettings.ContainsKey("OriginalSingleTapActCoeff"))
        {
#if !UNITY_ANDROID
            SetPersistentSetting("OriginalSingleTapActCoeff", "" + LeiaDisplay.Instance.displayConfig.SingleTapActCoefficient);
#elif UNITY_ANDROID && !UNITY_EDITOR
            SetPersistentSetting("OriginalSingleTapActCoeff", "" + LeiaDisplay.Instance.sdkConfig.act_singleTapCoef);
#endif
        }


        SetPersistentSetting("ACTMode", "" + LeiaDisplay.Instance.ActMode);
        SetPersistentSetting("NumViews", "" + config.NumViews.x);
        SetPersistentSetting("CenterView", "" + config.CenterViewNumber);
        SetPersistentSetting("ColorInversion", "" + config.colorInversion);
        SetPersistentSetting("ColorSlant", "" + config.colorSlant);
        SetPersistentSetting("d_over_n", "" + config.d_over_n);
        SetPersistentSetting("n", "" + config.n);
        SetPersistentSetting("p_over_du", "" + config.p_over_du);
        SetPersistentSetting("p_over_dv", "" + config.p_over_dv);
        SetPersistentSetting("pixelPitch", "" + config.PixelPitchInMM.x);
        SetPersistentSetting("s", "" + config.s);
        SetPersistentSetting("theta", "" + config.theta);
        SetPersistentSetting("ScreenWidth", "" + config.PanelResolution.x);
        SetPersistentSetting("ScreenHeight", "" + config.PanelResolution.y);
        SetPersistentSetting("BaselineScaling", "" + LeiaCamera.Instance.BaselineScaling);
        SetPersistentSetting("CameraShiftScaling", "" + LeiaCamera.Instance.CameraShiftScaling);
        SetPersistentSetting("SWBrightness", "" + LeiaDisplay.Instance.SWBrightness);
    }

    void SetPersistentSetting(string key, string value)
    {
        if (persistentSettings == null)
        {
            Debug.Log("persistentSettings is null.");
            return;
        }
        persistentSettings[key] = value;
    }

    void AddPersistentSetting(string key, string value)
    {
        persistentSettings.Add(key, value);
    }

    public void SaveSettings()
    {
        if (persistentSettings == null)
        {
            return;
        }
#if !UNITY_EDITOR
        GetPersistentSettingsFromUI();
        string savestr = "";

        foreach (KeyValuePair<string, string> entry in persistentSettings)
        {
            if (!persistentSettings.ContainsKey(entry.Key))
            {
                Debug.LogError("Persistent settings list does not contain a key titled \"" + entry.Key + "\"");
            }
            else
            {
                savestr += entry.Key + ", " + entry.Value + "\n";
            }
        }

        File.WriteAllText(settingsFilename, savestr);
#endif
    }

    void LoadSettings()
    {
        if (!File.Exists(settingsFilename))
        {
            return;
        }
        string loadStr = File.ReadAllText(settingsFilename);
        StringReader stringReader = new StringReader(loadStr);

        while (true)
        {
            string line = stringReader.ReadLine();
            if (line == null)
            {
                break;
            }
            int commaPos = line.IndexOf(',');
            string key = line.Substring(0, commaPos);
            string value = line.Substring(commaPos + 2);
            persistentSettings[key] = value;
        }

        SetSettingsFromDictionary();
    }

    void SetSettingsFromDictionary()
    {
        LeiaDisplay.Instance.ShowR0Test = bool.Parse(persistentSettings["R0Test"]);
        LeiaDisplay.Instance.blackViews = bool.Parse(persistentSettings["BlackViews"]);
        LeiaDisplay.Instance.CloseRangeSafety = bool.Parse(persistentSettings["CloseRangeSafety"]);
        LeiaCamera.Instance.cameraZaxisMovement = bool.Parse(persistentSettings["ZParallax"]);
        LeiaDisplay.Instance.eyeTrackingStatusBarEnabled = bool.Parse(persistentSettings["ShowTrackingStatusBar"]);
        LeiaDisplay.Instance.OriginalTimeDelay = float.Parse(persistentSettings["OriginalTimeDelay"]);
#if !UNITY_EDITOR
        LeiaDisplay.Instance.OriginalSingleTapActCoeff = float.Parse(persistentSettings["OriginalSingleTapActCoeff"]);
#endif

        DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
        config.timeDelay = float.Parse(persistentSettings["Delay"]);
        config.SingleTapActCoefficient = float.Parse(persistentSettings["SingleTapActCoeff"]);

#if UNITY_ANDROID && !UNITY_EDITOR
        int count = config.UserActCoefficients[0].Count;
        for (int i = 0; i < count; i++)
        {
            config.UserActCoefficients[0][i] = float.Parse(persistentSettings["ACT-" + letters[i]]);
        }

        for (int i = 0; i < config.UserActCoefficients[1].Count; i++)
        {
            config.UserActCoefficients[1][i] = float.Parse(persistentSettings["ACT-" + letters[i+ count]]);
        }
#elif !UNITY_ANDROID
        int count = config.UserActCoefficients[0].Count;
        for (int i = 0; i < count; i++)
        {
            config.UserActCoefficients[0][i] = float.Parse(persistentSettings["ACT-" + letters[i]]);
        }
#endif

        config.NumViews = new XyPair<int>(int.Parse(persistentSettings["NumViews"]), 1);
        config.CenterViewNumber = float.Parse(persistentSettings["CenterView"]);
        config.colorInversion = bool.Parse(persistentSettings["ColorInversion"]);
        config.colorSlant = int.Parse(persistentSettings["ColorSlant"]);
        config.d_over_n = float.Parse(persistentSettings["d_over_n"]);
        config.n = float.Parse(persistentSettings["n"]);
        config.p_over_du = float.Parse(persistentSettings["p_over_du"]);
        config.p_over_dv = float.Parse(persistentSettings["p_over_dv"]);
        config.PixelPitchInMM.x = float.Parse(persistentSettings["pixelPitch"]);
        config.s = float.Parse(persistentSettings["s"]);
        config.theta = float.Parse(persistentSettings["theta"]);
        config.PanelResolution = new XyPair<int>(
            int.Parse(persistentSettings["ScreenWidth"]),
            int.Parse(persistentSettings["ScreenHeight"])
            );

        LeiaCamera.Instance.BaselineScaling = float.Parse(persistentSettings["BaselineScaling"]);
        LeiaCamera.Instance.CameraShiftScaling = float.Parse(persistentSettings["CameraShiftScaling"]);
        LeiaDisplay.Instance.SWBrightness = float.Parse(persistentSettings["SWBrightness"]);

        switch (persistentSettings["ACTMode"])
        {
            case "SINGLETAP":
                LeiaDisplay.Instance.ActMode = LeiaDisplay.ACTMODE.SINGLETAP;
                break;

            case "MULTIVIEW":
                LeiaDisplay.Instance.ActMode = LeiaDisplay.ACTMODE.MULTIVIEW;
                break;

            case "OFF":
                LeiaDisplay.Instance.ActMode = LeiaDisplay.ACTMODE.OFF;
                break;

            default:
                LeiaDisplay.Instance.ActMode = LeiaDisplay.ACTMODE.SINGLETAP;
                break;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.sdkConfig.facePredictLatencyMs = float.Parse(persistentSettings["Delay"]);
        LeiaDisplay.Instance.sdkConfig.act_singleTapCoef = float.Parse(persistentSettings["SingleTapActCoeff"]);
        LeiaDisplay.Instance.sdkConfig.numViews[0] = int.Parse(persistentSettings["NumViews"]);
        LeiaDisplay.Instance.sdkConfig.centerViewNumber = float.Parse(persistentSettings["CenterView"]);
        LeiaDisplay.Instance.sdkConfig.colorInversion = bool.Parse(persistentSettings["ColorInversion"]) ?  1 :  0;
        LeiaDisplay.Instance.sdkConfig.colorSlant = int.Parse(persistentSettings["ColorSlant"]);
        LeiaDisplay.Instance.sdkConfig.d_over_n = float.Parse(persistentSettings["d_over_n"]);
        LeiaDisplay.Instance.sdkConfig.n = float.Parse(persistentSettings["n"]);
        LeiaDisplay.Instance.sdkConfig.p_over_du = float.Parse(persistentSettings["p_over_du"]);
        LeiaDisplay.Instance.sdkConfig.p_over_dv = float.Parse(persistentSettings["p_over_dv"]);
        LeiaDisplay.Instance.sdkConfig.dotPitchInMM[0] = float.Parse(persistentSettings["pixelPitch"]);
        LeiaDisplay.Instance.sdkConfig.s = float.Parse(persistentSettings["s"]);
        LeiaDisplay.Instance.sdkConfig.theta = float.Parse(persistentSettings["theta"]);
        LeiaDisplay.Instance.sdkConfig.panelResolution[0] = int.Parse(persistentSettings["ScreenWidth"]);
        LeiaDisplay.Instance.sdkConfig.panelResolution[1] = int.Parse(persistentSettings["ScreenHeight"]);

        LeiaDisplay.Instance.UpdateCNSDKConfig();
#endif

        leiaRenderSettingsPanel.UpdateUI();
        eyeTrackingSettingsPanel.UpdateUI();
        actPanel.UpdateUI();
        displayConfigPanel.UpdateUI();
    }
}
