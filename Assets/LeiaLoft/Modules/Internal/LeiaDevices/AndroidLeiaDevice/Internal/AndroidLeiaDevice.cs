/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace LeiaLoft
{
    public class AndroidLeiaDevice : AbstractLeiaDevice
    {
        private AndroidJavaClass _leiaBacklightClass;
        private AndroidJavaObject _leiaBacklightInstance;
        private bool _isLeiaDevice = false;

        const int supportedRatioCount = 6;

        // maps backlight mode to initial light intensities
        private readonly Dictionary<int, float[]> initialLightRatioCachePerMode = new Dictionary<int, float[]>();

        // maps backlight mode to most recently set light intensities
        private readonly Dictionary<int, float[]> lightRatioCachePerMode = new Dictionary<int, float[]>();

        public override int[] CalibrationOffset
        {
            get
            {
                InitializeBacklightModule();
                if (_leiaBacklightInstance == null)
                {
                    return base.CalibrationOffset;
                }

                int[] temp = _leiaBacklightInstance.Call<int[]> ("getXYCalibration");
                return new [] { temp[1], temp[2] };
            }
            set
            {
                this.Warning("Setting calibration from Unity Plugin is not supported anymore - use relevant app instead.");
            }
        }

        private void InitializeBacklightModule()
        {
            if ((_leiaBacklightClass == null ||
                _leiaBacklightInstance == null))
            {
                try
                {
                    if (_leiaBacklightClass == null)
                    {
                        _leiaBacklightClass = new AndroidJavaClass("android.leia.LeiaBacklight");
                        _isLeiaDevice = _leiaBacklightClass.CallStatic<bool>("isLeiaDevice");

                        if (_isLeiaDevice)
                        {
                            _leiaBacklightClass.CallStatic("start", "LeiaDisplay", false);
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (_isLeiaDevice)
                    {
                        _leiaBacklightInstance = _leiaBacklightClass.GetStatic<AndroidJavaObject>("instance");

                        // populate the backlight ratio cache. each supported backlight mode has an associated 2D/3D light ratio
                        for (int i = 0; i < supportedRatioCount; ++i)
                        {
                            // get the result of object.method
                            AndroidJavaObject resultObj = _leiaBacklightInstance.Call<AndroidJavaObject>("getRatio", i);

                            // get a C# pointer to the result. should be an array
                            System.IntPtr rawPtr = resultObj.GetRawObject();

                            // recruit ConvertFromJNIArray to move data from Java to C#
                            float[] result = AndroidJNIHelper.ConvertFromJNIArray<float[]>(rawPtr);

                            // for 2D and 3D light ratios, store in initial reference as well
                            initialLightRatioCachePerMode[i] = result;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    this.Error("Unable to get response from backlight service. Using default profile stub:" + _profileStubName);
                    this.Error(e.ToString());

                }

            }
        }

        public AndroidLeiaDevice(string stubName)
        {
            this.Debug("ctor");
            string displayType = stubName;

            if (!string.IsNullOrEmpty(displayType))
            {
                _profileStubName = displayType;
                this.Trace("displayType " + displayType);
            }
            else
            {
                this.Debug("No displayType received, using stub: " + stubName);
            }
            InitializeBacklightModule();
        }

        /// <summary>
        /// See LeiaDisplaySDK / androidsdk-display/src/main/java/com/leia/android/lights/LeiaLightsManagerV8.java#L8 for values.
        /// ... 2 = 2D, 3 = 3D, ... 5 = immersive
        /// </summary>
        /// <param name="modeId">The backlight mode to take on. In current version of the displaySDK, supports 2D, 3D, 5 (immersive)</param>
        public override void SetBacklightMode(int modeId)
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null)
            {
                this.Trace("SetBacklightMode" + modeId);
                _leiaBacklightInstance.Call ("setBacklightMode", modeId);

                // mode 5 is ImmersiveMode in Java DisplaySDK. this mode will never support backlight transition because it needs
                // to have 3D compensation disabled and re-setting light balance triggers 3D compensation
                const int transitionID = 5;
                if (lightRatioCachePerMode.ContainsKey(modeId) && modeId != transitionID)
                {
                    SetBacklightTransition(lightRatioCachePerMode[modeId][0], lightRatioCachePerMode[modeId][1], true);
                }
            }
        }

        public float[] GetLightRatio()
        {
            try
            {
                // get the result of object.method
                AndroidJavaObject resultObj = _leiaBacklightInstance.Call<AndroidJavaObject>("getRatio");

                // get a C# pointer to the result. should be an array
                System.IntPtr rawPtr = resultObj.GetRawObject();

                // recruit ConvertFromJNIArray to move data from Java to C#
                float[] result = AndroidJNIHelper.ConvertFromJNIArray<float[]>(rawPtr);

                return result;
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "When trying to retrieve {0} got error {1}", "getRatio", e);
                return default(float[]);
            }
        }

        /// <summary>
        /// Sets the current Lightfield mode (on / 2D OR off / 3D) to be equivalent to an intermediate mode between 2D and 3D. When the device is set to this Lightfield mode again, the last intermediate mode is restored.
        /// </summary>
        /// <param name="alpha">The 3D backlight intensity. 0 is very weak, 1 is fully active</param>
        public void SetDisplayLightBalance(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            // calculate an intensity for the 2D light that is on the spectrum from 0.95f to 0.0f
            float ratio2D = Mathf.Lerp(initialLightRatioCachePerMode[2][0], initialLightRatioCachePerMode[3][0], alpha);

            // calculate an intensity for the 3D light that is on the spectrum from 0.0f to 2.0f
            float ratio3D = Mathf.Lerp(initialLightRatioCachePerMode[2][1], initialLightRatioCachePerMode[3][1], alpha);

            SetBacklightTransition(ratio2D, ratio3D, true);
        }

        /// <summary>
        /// Calls display-sdk API to set backlight ratios.
        ///
        /// If user sets their backlight ratios without updating the light ratio cache, then calls SetBacklightMode(mode), the backlight ratios will use the last known light ratio values, which may be default values.
        ///
        /// Not ignoring the cache is useful if users want to transition from 3D to 2D, but on re-entering 3D, want to snap back to full 3D immediately.
        /// </summary>
        /// <param name="ratio2D">Intensity of 2D light</param>
        /// <param name="ratio3D">Intensity of 3D backlight</param>
        /// <param name="updateCache">Whether to update cache's record of this mode's latest ratios</param>
        public void SetBacklightTransition(float ratio2D, float ratio3D, bool updateCache)
        {
            int mode = GetBacklightMode();

            if (updateCache)
            {
                // update the light cache
                lightRatioCachePerMode[mode] = new[] { ratio2D, ratio3D };
            }

            // set 2D / standard display light's intensity to the value represented by $ratio2D,
            // set 3D / backlight intensity to the value represented by $ratio3D
            _leiaBacklightInstance.Call("setBacklightTransition", ratio2D, ratio3D);
        }

        /// <summary>
        /// Updates light balance and light balance cache to the SDK's known default light balance for current mode.
        /// </summary>
        public void ResetLightBalance()
        {
            int mode = GetBacklightMode();

            float light0 = initialLightRatioCachePerMode[mode][0];
            float light1 = initialLightRatioCachePerMode[mode][1];
            
            SetBacklightTransition(light0, light1, true);
        }

        /// <summary>
        /// See LeiaDisplaySDK / androidsdk-display/src/main/java/com/leia/android/lights/LeiaLightsManagerV8.java#L8 for values.
        /// ... 2 = 2D, 3 = 3D, ... 5 = immersive
        /// </summary>
        /// <param name="modeId">The backlight mode to take on. In current version of the displaySDK, supports 2D, 3D, 5 (immersive)</param>
        /// <param name="delay">Passes a setBacklightMode request with delay to displaySDK</param>
        public override void SetBacklightMode(int modeId, int delay)
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null)
            {
                this.Trace("SetBacklightMode" + modeId);
                _leiaBacklightInstance.Call ("setBacklightMode", modeId, delay);

                // mode 5 is ImmersiveMode in Java DisplaySDK. this mode will never support backlight transition because it needs
                // to have 3D compensation disabled and re-setting light balance triggers 3D compensation
                const int transitionID = 5;
                if (lightRatioCachePerMode.ContainsKey(modeId) && modeId != transitionID)
                {
                    SetBacklightTransition(lightRatioCachePerMode[modeId][0], lightRatioCachePerMode[modeId][1], true);
                }
            }
        }

        public override void RequestBacklightMode(int modeId)
        {
        }

        public override void RequestBacklightMode(int modeId, int delay)
        {
        }
        public override int GetBacklightMode()
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null)
            {
                int mode =_leiaBacklightInstance.Call<int>("getBacklightMode");
                return mode;
            }

            return 2;
        }

        public override DisplayConfig GetDisplayConfig()
        {
            LogUtil.Log(LogLevel.Info, "LeiaSDK GetDisplayConfig");

            if (_displayConfig != null)
            {
                return _displayConfig;
            }

            _displayConfig = new DisplayConfig();

            if (!_isLeiaDevice)
            {
                LogUtil.Log(LogLevel.Info, "LeiaSDK GetDisplayConfig Not LeiaDevice");

                base.ApplyOfflineConfigValues();

                base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);
                Debug.Log("Non supported device");
                return _displayConfig;
            }

            try
            {
                _displayConfig = GetDisplayConfigFromService();
                if (_displayConfig != null)
                {
                    return _displayConfig;
                }

                _displayConfig = GetDisplayConfigFromJson();
                if (_displayConfig != null) {
                    return _displayConfig;
                }

                _displayConfig = new DisplayConfig();

                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
                }

                AndroidJavaObject javaConfig = _leiaBacklightInstance.Call<AndroidJavaObject>("getDisplayConfig");

                _displayConfig.Gamma = javaConfig.Call<float>("getGamma");
                _displayConfig.Beta = javaConfig.Call<float>("getBeta");
                
                // The following params
                // DotPitchInMm, PanelResolution, NumViews, AlignmentOffset, DisplaySizeInMm, and ViewResolution
                // are reported as
                // (value for device's portrait mode width, value for device's portrait mode height) rather than
                // (value for device's wide side, value for device's long side) as we would expect.
                // See DisplayConfig :: UserOrientationIsLandscape

                _displayConfig.PixelPitchInMM = GetXyPairFromAndroid<float>(javaConfig, "getDotPitchInMm", "floatValue");
                _displayConfig.PanelResolution = GetXyPairFromAndroid<int>(javaConfig, "getPanelResolution", "intValue");
                _displayConfig.NumViews = GetXyPairFromAndroid<int>(javaConfig, "getNumViews", "intValue");
                _displayConfig.AlignmentOffset = GetXyPairFromAndroid<float>(javaConfig, "getAlignmentOffset", "floatValue");
                _displayConfig.DisplaySizeInMm = GetXyPairFromAndroid<int>(javaConfig, "getDisplaySizeInMm", "intValue");
                _displayConfig.ViewResolution = GetXyPairFromAndroid<int>(javaConfig, "getViewResolution", "intValue");

                _displayConfig.ActCoefficients = GetXyPairOfListsOfTFromAndroid<float>(javaConfig, "getViewSharpeningCoefficients", "floatValue");

                // since we now use slanted pathway to render all content, we no longer need to retrieve these flags
                _displayConfig.Slant = false;
                _displayConfig.isSlanted = true;
                _displayConfig.isSquare = false;

                _displayConfig.InterlacingMatrixLandscape = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixLandscape");
                _displayConfig.InterlacingMatrixLandscape180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixLandscape180");
                _displayConfig.InterlacingMatrixPortrait = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixPortrait");
                _displayConfig.InterlacingMatrixPortrait180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixPortrait180");

                _displayConfig.InterlacingVectorLandscape = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorLandscape");
                _displayConfig.InterlacingVectorLandscape180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorLandscape180");
                _displayConfig.InterlacingVectorPortrait = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorPortrait");
                _displayConfig.InterlacingVectorPortrait180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorPortrait180");

                _displayConfig.SystemDisparityPercent = _leiaBacklightInstance.Call<float>("getSystemDisparityPercent");
                _displayConfig.SystemDisparityPixels = _leiaBacklightInstance.Call<float>("getSystemDisparityPixels");

                _displayConfig.n = 1.47f;
                _displayConfig.theta = 0.0f;
                _displayConfig.cameraCenterX = -37.88f;
                _displayConfig.cameraCenterY = 165.57f;
                _displayConfig.colorSlant = 0;
                _displayConfig.colorInversion = false;
                _displayConfig.p_over_du = 3.0f;
                _displayConfig.p_over_dv = 1.0f;
                _displayConfig.s = 11.0f;
                _displayConfig.d_over_n = 0.5f;
                _displayConfig.cameraThetaX = 0.0f;
                _displayConfig.cameraThetaY = 0.0f;
                _displayConfig.cameraThetaZ = 0.0f;
                _displayConfig.CenterViewNumber = 2f;

                _displayConfig.status = DisplayConfig.Status.FailedToLoadFromDevice;

                return _displayConfig;
            }
            catch (System.Exception e)
            {
                Debug.Log( "While loading data from Android DisplayConfig from firmware, encountered error {0}"+ e.ToString());
                LogUtil.Log(LogLevel.Error, "While loading data from Android DisplayConfig from firmware, encountered error {0}", e);
                
                //OverrideLeiaDevice device = new OverrideLeiaDevice("DisplayConfig_Override.json");

                _displayConfig.n = 1.59f;
                _displayConfig.theta = 0.0f;
                _displayConfig.cameraCenterX = -37.88f;
                _displayConfig.cameraCenterY = 165.57f;
                _displayConfig.colorSlant = 0;
                _displayConfig.colorInversion = false;
                _displayConfig.p_over_du = 3.0f;
                _displayConfig.p_over_dv = 1.0f;
                _displayConfig.s = 7.45f;
                _displayConfig.d_over_n = 0.485f;
                _displayConfig.cameraThetaX = 0.0f;
                _displayConfig.cameraThetaY = 0.0f;
                _displayConfig.cameraThetaZ = 0.0f;
                _displayConfig.CenterViewNumber = 2f;
                _displayConfig.Gamma = 2.0f;
                _displayConfig.Beta = 2.0f;
                _displayConfig.PanelResolution = new XyPair<int>(2560,1600);
                _displayConfig.NumViews = new XyPair<int>(9,1);
                _displayConfig.Slant = false;
                _displayConfig.isSlanted = true;
                _displayConfig.isSquare = false;
                _displayConfig.PixelPitchInMM = new XyPair<float>(.10389f,.10389f);
                
                _displayConfig.ViewResolution = new XyPair<int>(960,600);

                List<float> xlist = new List<float>();
                xlist.Add(.08f);
                xlist.Add(.01f);
                xlist.Add(.01f);
                xlist.Add(.01f);
                List<float> ylist =  new List<float>();
                ylist.Add(.08f);
                ylist.Add(.01f);
                ylist.Add(.01f);
                ylist.Add(.01f);

                _displayConfig.ActCoefficients = new XyPair<List<float>>(xlist,ylist);
                _displayConfig.SystemDisparityPixels = 4;

                return _displayConfig;
            }

            // populate _displayConfig from FW with developer-tuned values
            base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);

            return _displayConfig;
        }

        public override DisplayConfig GetUnmodifiedDisplayConfig()
        {
            return _displayConfigUnmodified;
        }

        /// <summary>
        /// Defaulting LP1 values for testing Face Tracking
        /// </summary>
        private DisplayConfig GetDisplayConfig_LP1_DCv2()
        {
            _displayConfig = new DisplayConfig();

            //values Referenced from LeiaWindowsDevice.GetDisplayConfig - may need revision for LP1
            _displayConfig.cameraCenterX = -37.88f;
            _displayConfig.cameraCenterY = 165.57f;

            //values referenced from https://docs.google.com/spreadsheets/d/1eJIYVg3BjOEPkHuLgMklKp5gZyPfQX1OsBf3qWMTGIs/edit?usp=sharing
            _displayConfig.n = 1.59f;
            _displayConfig.theta = 0.0f;
            _displayConfig.colorSlant = 0;
            _displayConfig.colorInversion = false;
            _displayConfig.p_over_du = 3.0f;
            _displayConfig.p_over_dv = 1.0f;
            _displayConfig.s = 7.45f;
            _displayConfig.d_over_n = 0.485f;
            _displayConfig.CenterViewNumber = 4.0f;
            _displayConfig.Beta = 2.0f;
            _displayConfig.Gamma = 2.0f;

            //values referenced from Assets/LeiaLoft/Resources/DisplayConfig_LumePad.json
            _displayConfig.ActCoefficientsX = new[] { 0.05f, 0.015f };
            _displayConfig.ActCoefficientsY = new[] { 0.032f, 0.02f };
            _displayConfig.ActCoefficients = new XyPair<List<float>>(new List<float>(_displayConfig.ActCoefficientsX), new List<float>(_displayConfig.ActCoefficientsY));
            _displayConfig.AlignmentOffset = new XyPair<float>(0, 0);
            _displayConfig.DisplaySizeInMm = new XyPair<int>(0, 0);
            _displayConfig.PixelPitchInMM = new XyPair<float>(0.10389f, 0.10389f);
            _displayConfig.InterlacingMatrix = new[] { 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 640f, 0f, 0f, 0f };
            _displayConfig.InterlacingMatrixLandscape = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingMatrixLandscape180 = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingMatrixPortrait = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingMatrixPortrait180 = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingVector = new[] { 0f, 0f, 0f, 0f };
            _displayConfig.isSlanted = true;
            _displayConfig.isSquare = false;
            _displayConfig.NumViews = new XyPair<int>(4, 1);
            _displayConfig.PanelResolution = new XyPair<int>(2560, 1600);
            _displayConfig.ResolutionScale = 1f;
            _displayConfig.Slant = true;
            _displayConfig.SystemDisparityPercent = 0.125f;
            _displayConfig.SystemDisparityPixels = 8.0f;
            _displayConfig.ViewResolution = new XyPair<int>(640, 400);

            _displayConfig.UserActCoefficients = _displayConfig.ActCoefficients;
            _displayConfig.UserDisplaySizeInMm = _displayConfig.DisplaySizeInMm;
            _displayConfig.UserPixelPitchInMM = _displayConfig.PixelPitchInMM;
            _displayConfig.UserNumViews = _displayConfig.NumViews;
            _displayConfig.UserOrientationIsLandscape = true;
            _displayConfig.UserPanelResolution = _displayConfig.PanelResolution;
            _displayConfig.UserViewResolution = _displayConfig.ViewResolution;
            _displayConfig.UserAspectRatio = _displayConfig.PanelResolution.x / Mathf.Max(1, _displayConfig.PanelResolution.y);
            _displayConfig.UserViewResolution = _displayConfig.ViewResolution;

            //unconfirmed values
            _displayConfig.ViewboxSize = new XyPair<float>(640.0f, 400.0f);
            _displayConfig.cameraThetaX = 0.375f;
            _displayConfig.cameraThetaY = 0.029999999329447748f;
            _displayConfig.cameraThetaZ = 0.029999999329447748f;

            return _displayConfig;
        }
        /// <summary>
         /// Defaulting LP2 values for testing Face Tracking
         /// </summary>
        private DisplayConfig GetDisplayConfig_LP2_DCv2()
        {
            _displayConfig = new DisplayConfig();

            //values Referenced from LeiaWindowsDevice.GetDisplayConfig - may need revision for LP2
            _displayConfig.cameraCenterX = -37.88f;
            _displayConfig.cameraCenterY = 165.57f;

            //values referenced from https://docs.google.com/spreadsheets/d/1eJIYVg3BjOEPkHuLgMklKp5gZyPfQX1OsBf3qWMTGIs/edit?usp=sharing
            _displayConfig.n = 1.59f;
            _displayConfig.theta = 0.0f;
            _displayConfig.colorSlant = 0;
            _displayConfig.colorInversion = false;
            _displayConfig.p_over_du = 3.0f;
            _displayConfig.p_over_dv = 1.0f;
            _displayConfig.s = 7.45f;
            _displayConfig.d_over_n = 0.485f;
            _displayConfig.CenterViewNumber = 4.0f;
            _displayConfig.Beta = 2.0f;
            _displayConfig.Gamma = 2.0f;
            _displayConfig.NumViews = new XyPair<int>(9, 1);
            _displayConfig.ViewResolution = new XyPair<int>(960, 600);

            //values referenced from Assets/LeiaLoft/Resources/DisplayConfig_LumePad.json
            _displayConfig.ActCoefficientsX = new[] { 0.05f, 0.015f };
            _displayConfig.ActCoefficientsY = new[] { 0.032f, 0.02f };
            _displayConfig.ActCoefficients = new XyPair<List<float>>(new List<float>(_displayConfig.ActCoefficientsX), new List<float>(_displayConfig.ActCoefficientsY));
            _displayConfig.AlignmentOffset = new XyPair<float>(0, 0);
            _displayConfig.DisplaySizeInMm = new XyPair<int>(0, 0);
            _displayConfig.PixelPitchInMM = new XyPair<float>(0.10389f, 0.10389f);
            _displayConfig.InterlacingMatrix = new[] { 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 960f, 0f, 0f, 0f };
            _displayConfig.InterlacingMatrixLandscape = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingMatrixLandscape180 = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingMatrixPortrait = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingMatrixPortrait180 = _displayConfig.InterlacingMatrix;
            _displayConfig.InterlacingVector = new[] { 0f, 0f, 0f, 0f };
            _displayConfig.isSlanted = true;
            _displayConfig.isSquare = false;
            _displayConfig.PanelResolution = new XyPair<int>(2560, 1600);
            _displayConfig.ResolutionScale = 1f;
            _displayConfig.Slant = true;
            _displayConfig.SystemDisparityPercent = 0.125f;
            _displayConfig.SystemDisparityPixels = 8.0f;

            _displayConfig.UserActCoefficients = _displayConfig.ActCoefficients;
            _displayConfig.UserDisplaySizeInMm = _displayConfig.DisplaySizeInMm;
            _displayConfig.UserPixelPitchInMM = _displayConfig.PixelPitchInMM;
            _displayConfig.UserNumViews = _displayConfig.NumViews;
            _displayConfig.UserOrientationIsLandscape = true;
            _displayConfig.UserPanelResolution = _displayConfig.PanelResolution;
            _displayConfig.UserViewResolution = _displayConfig.ViewResolution;
            _displayConfig.UserAspectRatio = _displayConfig.PanelResolution.x / Mathf.Max(1, _displayConfig.PanelResolution.y);
            _displayConfig.UserViewResolution = _displayConfig.ViewResolution;

            //unconfirmed values
            _displayConfig.ViewboxSize = new XyPair<float>(960.0f, 600.0f);
            _displayConfig.cameraThetaX = 0.375f;
            _displayConfig.cameraThetaY = 0.029999999329447748f;
            _displayConfig.cameraThetaZ = 0.029999999329447748f;

            return _displayConfig;
        }
        private DisplayConfig GetDisplayConfigFromService()
        {
            DisplayConfig displayConfig = new DisplayConfig();

#if UNITY_ANDROID && !UNITY_EDITOR
            if (LeiaDisplay.Instance.CNSDK == null)
            {
                return null;
            }

            Leia.Config sdkConfig;
            System.Int32 error = LeiaDisplay.Instance.CNSDK.GetConfig(out sdkConfig);
            if (error != 0)
            {
                LogUtil.Log(LogLevel.Error, "LeiaSDK Failed to load config from service. Error: {0}", error);
                return null;
            }
            else
            {
                LeiaDisplay.Instance.sdkConfig = sdkConfig;
            }
            

            displayConfig.n = sdkConfig.n;

            //Hack negating until we upgrade blink libs
            displayConfig.cameraCenterX = -sdkConfig.cameraCenterX;             //-displayConfigJson.trackingCamera.cameraT[0];
            displayConfig.cameraCenterY = sdkConfig.cameraCenterY;              //displayConfigJson.trackingCamera.cameraT[1];
            displayConfig.cameraCenterZ = sdkConfig.cameraCenterZ;              //displayConfigJson.trackingCamera.cameraT[2];
            displayConfig.cameraThetaX = sdkConfig.cameraThetaX;                //displayConfigJson.trackingCamera.cameraR[0];
            displayConfig.cameraThetaY = sdkConfig.cameraThetaY;                //displayConfigJson.trackingCamera.cameraR[1];
            displayConfig.cameraThetaZ = sdkConfig.cameraThetaZ;                //displayConfigJson.trackingCamera.cameraR[2];

            DisplayConfig.CameraStreamParams_ cameraStreamParams = new DisplayConfig.CameraStreamParams_();
            cameraStreamParams.width = sdkConfig.cameraWidth;                    //displayConfigJson.trackingCamera.width;
            cameraStreamParams.height = sdkConfig.cameraHeight;                  //displayConfigJson.trackingCamera.height;
            cameraStreamParams.fps = sdkConfig.cameraFps;                        //displayConfigJson.trackingCamera.fps;
            cameraStreamParams.binningFactor = sdkConfig.cameraBinningFactor;    //displayConfigJson.trackingCamera.focalLength;
            displayConfig.CameraStreamParams = cameraStreamParams;

            //note: this seems unused, for now this whole function is temporary anyway
            //displayConfig.PredictParams = sdkConfig.facePredictAlphaX;          //displayConfigJson.trackingCamera.predictParams;

            displayConfig.n = sdkConfig.n;                                          //displayConfigJson.displayGeometry.n;
            displayConfig.theta = sdkConfig.theta;                                          //displayConfigJson.displayGeometry.theta;
            displayConfig.colorSlant = sdkConfig.colorSlant;                                          //displayConfigJson.displayGeometry.colorSlant;
            displayConfig.colorInversion = sdkConfig.colorInversion == 1;                                          //displayConfigJson.displayGeometry.colorInversion;
            displayConfig.p_over_du = sdkConfig.p_over_du;                                          //displayConfigJson.displayGeometry.p_over_du;
            displayConfig.p_over_dv = sdkConfig.p_over_dv;                                          //displayConfigJson.displayGeometry.p_over_dv;
            displayConfig.s = sdkConfig.s;                                          //displayConfigJson.displayGeometry.s;
            displayConfig.d_over_n = sdkConfig.d_over_n;                                          //displayConfigJson.displayGeometry.d_over_n;
            displayConfig.CenterViewNumber = sdkConfig.centerViewNumber;                                          //displayConfigJson.displayGeometry.centerView;
            displayConfig.Beta = sdkConfig.act_beta;                                          //displayConfigJson.actCoefficients.beta;
            displayConfig.Gamma = sdkConfig.act_gamma;                                          //displayConfigJson.actCoefficients.gamma;
            displayConfig.NumViews = new XyPair<int>(sdkConfig.numViews[0], 1);
            displayConfig.ViewResolution = new XyPair<int>(sdkConfig.viewResolution[0], sdkConfig.viewResolution[1]);

            if (sdkConfig.sharpeningKernelXSize != 4)
            {
                LogUtil.Log(LogLevel.Error, "LeiaSDK found wrong number of ACT Coefficients: {0}", sdkConfig.sharpeningKernelXSize);
                return null;
            }
            // TODO: validate the order
            displayConfig.ActCoefficientsX = new[] { sdkConfig.sharpeningKernelX[0], sdkConfig.sharpeningKernelX[1] };
            displayConfig.ActCoefficientsY = new[] { sdkConfig.sharpeningKernelX[2], sdkConfig.sharpeningKernelX[3] };
            displayConfig.ActCoefficients = new XyPair<List<float>>(new List<float>(displayConfig.ActCoefficientsX), new List<float>(displayConfig.ActCoefficientsY));

            displayConfig.AlignmentOffset = new XyPair<float>(0, 0);
            displayConfig.DisplaySizeInMm = new XyPair<int>(0, 0);

            // TODO: validate
            displayConfig.PixelPitchInMM = new XyPair<float>(sdkConfig.dotPitchInMM[0], sdkConfig.dotPitchInMM[1]);

            // TODO: deprecated?
            displayConfig.InterlacingMatrix = new[] { 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 960f, 0f, 0f, 0f };
            displayConfig.InterlacingMatrixLandscape = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingMatrixLandscape180 = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingMatrixPortrait = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingMatrixPortrait180 = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingVector = new[] { 0f, 0f, 0f, 0f };
            displayConfig.isSlanted = true;
            displayConfig.isSquare = false;

            displayConfig.PanelResolution = new XyPair<int>(sdkConfig.panelResolution[0], sdkConfig.panelResolution[1]);

            displayConfig.ResolutionScale = 1f;
            displayConfig.Slant = true;

            displayConfig.SystemDisparityPixels = sdkConfig.systemDisparityPixels;
            displayConfig.SystemDisparityPercent = 1.0f / displayConfig.SystemDisparityPixels;

            displayConfig.UserActCoefficients = displayConfig.ActCoefficients;

            displayConfig.UserDisplaySizeInMm = displayConfig.DisplaySizeInMm;
            displayConfig.UserPixelPitchInMM = displayConfig.PixelPitchInMM;
            displayConfig.UserNumViews = displayConfig.NumViews;
            displayConfig.UserOrientationIsLandscape = true;
            displayConfig.UserPanelResolution = displayConfig.PanelResolution;
            displayConfig.UserViewResolution = displayConfig.ViewResolution;
            displayConfig.UserAspectRatio = displayConfig.PanelResolution.x / Mathf.Max(1, displayConfig.PanelResolution.y);
            displayConfig.UserViewResolution = displayConfig.ViewResolution;

            //unconfirmed values
            displayConfig.ViewboxSize = new XyPair<float>(960.0f, 600.0f); // tileWidth, tileHeight?
            
            displayConfig.status = DisplayConfig.Status.SuccessfullyLoadedFromDevice;

            LogUtil.Log(LogLevel.Info, "LeiaSDK loaded config from service");
#endif

            _displayConfigUnmodified = DisplayConfig.CopyDisplayConfig(displayConfig);

            return displayConfig;
        }
        private DisplayConfig GetDisplayConfigFromJson()
        {
            AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            string javaConfigString = _leiaBacklightInstance.Call<string>("getDisplayConfig", currentActivity);
            if (string.IsNullOrEmpty(javaConfigString))
            {
                return null;
            }
            Json.DisplayConfig displayConfigJson = JsonUtility.FromJson<Json.DisplayConfig>(javaConfigString);

            DisplayConfig displayConfig = new DisplayConfig();

            if (displayConfigJson.trackingCamera.cameraT.Length != 3 ||
                displayConfigJson.trackingCamera.cameraR.Length != 3)
            {
                return null;
            }

            //HACK til george fixes the Gpu blink wrapper, remove negation !
            displayConfig.cameraCenterX = -displayConfigJson.trackingCamera.cameraT[0];
            displayConfig.cameraCenterY = displayConfigJson.trackingCamera.cameraT[1];
            displayConfig.cameraCenterZ = displayConfigJson.trackingCamera.cameraT[2];
            displayConfig.cameraThetaX = displayConfigJson.trackingCamera.cameraR[0];
            displayConfig.cameraThetaY = displayConfigJson.trackingCamera.cameraR[1];
            displayConfig.cameraThetaZ = displayConfigJson.trackingCamera.cameraR[2];

            DisplayConfig.CameraStreamParams_ cameraStreamParams = new DisplayConfig.CameraStreamParams_();
            cameraStreamParams.width = displayConfigJson.trackingCamera.width;
            cameraStreamParams.height = displayConfigJson.trackingCamera.height;
            cameraStreamParams.fps = displayConfigJson.trackingCamera.fps;
            cameraStreamParams.binningFactor = displayConfigJson.trackingCamera.focalLength;
            displayConfig.CameraStreamParams = cameraStreamParams;

            displayConfig.PredictParams = displayConfigJson.trackingCamera.predictParams;

            displayConfig.n = displayConfigJson.displayGeometry.n;
            displayConfig.theta = displayConfigJson.displayGeometry.theta;
            displayConfig.colorSlant = displayConfigJson.displayGeometry.colorSlant;
            displayConfig.colorInversion = displayConfigJson.displayGeometry.colorInversion;
            displayConfig.p_over_du = displayConfigJson.displayGeometry.p_over_du;
            displayConfig.p_over_dv = displayConfigJson.displayGeometry.p_over_dv;
            displayConfig.s = displayConfigJson.displayGeometry.s;
            displayConfig.d_over_n = displayConfigJson.displayGeometry.d_over_n;
            displayConfig.CenterViewNumber = displayConfigJson.displayGeometry.centerView;
            displayConfig.Beta = displayConfigJson.actCoefficients.beta;
            displayConfig.Gamma = displayConfigJson.actCoefficients.gamma;
            displayConfig.NumViews = new XyPair<int>(displayConfigJson.numViews.value, 1);
            displayConfig.ViewResolution = new XyPair<int>(displayConfigJson.tileWidth.value, displayConfigJson.tileHeight.value);

            if (displayConfigJson.actCoefficients.value.Length != 4)
            {
                return null;
            }
            // TODO: validate the order
            displayConfig.ActCoefficientsX = new[] { displayConfigJson.actCoefficients.value[0], displayConfigJson.actCoefficients.value[1] };
            displayConfig.ActCoefficientsY = new[] { displayConfigJson.actCoefficients.value[2], displayConfigJson.actCoefficients.value[3] };
            displayConfig.ActCoefficients = new XyPair<List<float>>(new List<float>(displayConfig.ActCoefficientsX), new List<float>(displayConfig.ActCoefficientsY));

            displayConfig.AlignmentOffset = new XyPair<float>(0, 0);
            displayConfig.DisplaySizeInMm = new XyPair<int>(0, 0);

            // TODO: validate
            displayConfig.PixelPitchInMM = new XyPair<float>(displayConfigJson.displayGeometry.pixelPitch, displayConfigJson.displayGeometry.pixelPitch);

            // TODO: deprecated?
            displayConfig.InterlacingMatrix = new[] { 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 960f, 0f, 0f, 0f };
            displayConfig.InterlacingMatrixLandscape = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingMatrixLandscape180 = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingMatrixPortrait = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingMatrixPortrait180 = displayConfig.InterlacingMatrix;
            displayConfig.InterlacingVector = new[] { 0f, 0f, 0f, 0f };
            displayConfig.isSlanted = true;
            displayConfig.isSquare = false;

            displayConfig.PanelResolution = new XyPair<int>(displayConfigJson.screenWidth.value, displayConfigJson.screenHeight.value);

            displayConfig.ResolutionScale = 1f;
            displayConfig.Slant = true;

            displayConfig.SystemDisparityPixels = displayConfigJson.systemDisparity.value;
            displayConfig.SystemDisparityPercent = 1.0f / displayConfig.SystemDisparityPixels;

            displayConfig.UserActCoefficients = displayConfig.ActCoefficients;
            displayConfig.UserDisplaySizeInMm = displayConfig.DisplaySizeInMm;
            displayConfig.UserPixelPitchInMM = displayConfig.PixelPitchInMM;
            displayConfig.UserNumViews = displayConfig.NumViews;
            displayConfig.UserOrientationIsLandscape = true;
            displayConfig.UserPanelResolution = displayConfig.PanelResolution;
            displayConfig.UserViewResolution = displayConfig.ViewResolution;
            displayConfig.UserAspectRatio = displayConfig.PanelResolution.x / Mathf.Max(1, displayConfig.PanelResolution.y);
            displayConfig.UserViewResolution = displayConfig.ViewResolution;

            //unconfirmed values
            displayConfig.ViewboxSize = new XyPair<float>(960.0f, 600.0f); // tileWidth, tileHeight?
            
            displayConfig.status = DisplayConfig.Status.SuccessfullyLoadedFromDevice;

            _displayConfigUnmodified = DisplayConfig.CopyDisplayConfig(displayConfig);

            return displayConfig;
        }
        public override bool IsConnected()
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null && _isLeiaDevice)
            {
                return _leiaBacklightInstance.Call<bool>("isConnected");
            }
            return false;
        }

        public override RuntimePlatform GetRuntimePlatform()
        {
            return RuntimePlatform.Android;
        }

        /// <summary>
        /// Android devices may have screen or height greater at any moment.
        ///
        /// Due to an issue on Lumepad where Screen.Orientation can be Portrait or Landscape in wrong cases,
        /// have to use Screen.width/height to determine if device is landscape or not.
        /// </summary>
        /// <returns>True if device screen is wider than it is tall</returns>
        public override bool IsScreenOrientationLandscape()
        {
            return Screen.width > Screen.height;
        }

        /// <summary>
        /// Helper function for moving XyPair<List<T>> from Java into C#"
        /// </summary>
        /// <typeparam name="T">Type parameter of XyPair<List<T>></typeparam>
        /// <param name="javaConfig">A reference to the Java-side DisplayConfig in the LeiaLights DisplaySDK</param>
        /// <param name="methodName">A method on the Java-side DisplayConfig to call to get an XyPair<List<T>></param>
        /// <param name="javaTypeName">The name of the method of the Java XyPair to call to convert a T into a primitive type which can be moved from Java to C#</param>
        /// <returns>an XyPair of Lists from Java</returns>
        private static XyPair<List<T>> GetXyPairOfListsOfTFromAndroid<T>(AndroidJavaObject javaConfig, string methodName, string javaTypeName)
        {
            const string sizeMethodName = "size";
            const string getMethodName = "get";
            AndroidJavaObject javaPairObj = javaConfig.Call<AndroidJavaObject>(methodName);
            // assume that this returned a java-side XyPair<List<T>> as an AndroidJavaObject

            List<List<T>> items = new List<List<T>>();

            // iterate through x and y List child objects of the XyPair
            foreach (string xy in new [] { "x", "y"})
            {
                items.Add(new List<T>());
                // iterate through elements in the XyPair.x or XyPair.y
                for (int i = 0; i < javaPairObj.Get<AndroidJavaObject>(xy).Call<int>(sizeMethodName); ++i)
                {
                    // call List.Get(i), convert into a C# object
                    T element = javaPairObj.Get<AndroidJavaObject>(xy).Call<AndroidJavaObject>(getMethodName, i).Call<T>(javaTypeName);
                    items[items.Count - 1].Add(element);
                }
            }

            XyPair<List<T>> pair = new XyPair<List<T>>(items[0], items[1]);
            return pair;
        }

        /// <summary>
        /// Helper function for moving XyPair from Java to C#
        /// </summary>
        /// <typeparam name="T">Type parameter of XyPair</typeparam>
        /// <param name="javaConfig">A reference to the Java-side DisplayConfig in the LeiaLights DisplaySDK</param>
        /// <param name="methodName">A method on the Java-side DisplayConfig to call to get an XyPairOfT</param>
        /// <param name="javaTypeName">The name of the method of the Java XyPair to call to convert a T into a primitive type which can be moved from Java to C#</param>
        /// <returns>An XyPair from Java</returns>
        private static XyPair<T> GetXyPairFromAndroid<T>(AndroidJavaObject javaConfig, string methodName, string javaTypeName)
        {
            AndroidJavaObject javaPairObj = javaConfig.Call<AndroidJavaObject>(methodName);
            T item1 = javaPairObj.Get<AndroidJavaObject>("x").Call<T>(javaTypeName);
            T item2 = javaPairObj.Get<AndroidJavaObject>("y").Call<T>(javaTypeName);
            XyPair<T> pair = new XyPair<T>(item1, item2);

            return pair;
        }

        /// <summary>
        /// Helper function for moving array from Java to C#
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="javaObj">A Java object which supports javaObj.methodName()</param>
        /// <param name="methodName">A method to call</param>
        /// <returns>Returns the T[] on the Java object into C#</returns>
        private static T[] GetArrayFromAndroid<T>(AndroidJavaObject javaObj, string methodName)
        {
            try
            {
                // get the result of object.method
                AndroidJavaObject resultObj = javaObj.Call<AndroidJavaObject>(methodName);

                // get a C# pointer to the result. should be an array
                System.IntPtr rawPtr = resultObj.GetRawObject();

                // recruit ConvertFromJNIArray to move data from Java to C#
                T[] result = AndroidJNIHelper.ConvertFromJNIArray<T[]>(rawPtr);

                return result;
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "When trying to retrieve {0} got error {1}", methodName, e);
                return default(T[]);
            }
        }
    }
}

namespace LeiaLoft
{
    namespace Json
    {
        // Auto-generated using Visual Studio's "Edit - Paste Special - Pase JSON as classes" from content of displayConfig_LP2_json.js

        [System.Serializable]
        public class DisplayConfig
        {
            public Actcoefficients actCoefficients;
            public Antialiasing antiAliasing;
            public Backlightratio2d backlightRatio2D;
            public Colorcorrection colorCorrection;
            public Configinfo configInfo;
            public Displayinfo displayInfo;
            public Displaygeometry displayGeometry;
            public Numviews numViews;
            public Screenheight screenHeight;
            public Screenwidth screenWidth;
            public Systemdisparity systemDisparity;
            public Tileheight tileHeight;
            public Tilewidth tileWidth;
            public Trackingcamera trackingCamera;
            public Rendering rendering;
        }

        [System.Serializable]
        public class Actcoefficients
        {
            public int beta;
            public string description;
            public float gamma;
            public float[] value;
        }

        [System.Serializable]
        public class Antialiasing
        {
            public string description;
            public float[][] coordinates;
            public float[] weights;
        }

        [System.Serializable]
        public class Backlightratio2d
        {
            public string description;
            public float value;
        }

        [System.Serializable]
        public class Colorcorrection
        {
            public string description;
        }

        [System.Serializable]
        public class Configinfo
        {
            public string versionNum;
            public string lastUpdated;
        }

        [System.Serializable]
        public class Displayinfo
        {
            public string displayClass;
            public string displaySN;
            public string displayMfgDate;
        }

        [System.Serializable]
        public class Displaygeometry
        {
            public float centerView;
            public bool colorInversion;
            public int colorSlant;
            public float d_over_n;
            public string description;
            public float n;
            public int p_over_du;
            public int p_over_dv;
            public float pixelPitch;
            public float s;
            public float theta;
        }

        [System.Serializable]
        public class Numviews
        {
            public string description;
            public int value;
        }

        [System.Serializable]
        public class Screenheight
        {
            public string description;
            public int value;
        }

        [System.Serializable]
        public class Screenwidth
        {
            public string description;
            public int value;
        }

        [System.Serializable]
        public class Systemdisparity
        {
            public string description;
            public int value;
        }

        [System.Serializable]
        public class Tileheight
        {
            public string description;
            public int value;
        }

        [System.Serializable]
        public class Tilewidth
        {
            public string description;
            public int value;
        }

        [System.Serializable]
        public class Trackingcamera
        {
            public float[] cameraT;
            public float[] cameraR;
            public float[] predictParams;
            public int width;
            public int height;
            public int fps;
            public float focalLength;
            public string description;
        }

        [System.Serializable]
        public class Rendering
        {
            public int fps;
        }
    }
}
