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
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace LeiaLoft
{

    public class WindowsLeiaDevice : AbstractLeiaDevice
    {
        // readonly objects which wrap some DLLs
        private readonly LeiaDisplayParamsWrapper paramsWrapper = new LeiaDisplayParamsWrapper();
        private readonly LeiaDisplaySdkCppWrapper cppWrapper = new LeiaDisplaySdkCppWrapper();

        const int xyPairLen = 2;
        const int matrixDim = 4;

        /// <summary>
        /// Returns number of horizontal views on the display
        /// </summary>
        /// <returns>Count of horizontal views</returns>
        public override int GetDisplayViewcount()
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't GetDisplayViewcount!");
                return 1;
            }

            int[] viewXY = paramsWrapper.Get1DArray<int>("getNumViews", xyPairLen);
            if (viewXY != null && viewXY.Length > 0)
            {
                return viewXY[0];
            }
            return 1;
        }

        public string GetBoardDisplayClass()
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't GetBoardDisplayClass!");
                return "";
            }

            // will be "Unknown" on 12.3 displays
            return paramsWrapper.GetString("getDisplayClass");
        }

        /// <summary>
        /// A string representing that particular display's ID
        /// </summary>
        /// <returns></returns>
        public string GetBoardDisplayID()
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't GetBoardDisplayID!");
                return "";
            }

            return paramsWrapper.GetString("getDisplayId");
        }

        /// <summary>
        /// Gets firmware version
        /// </summary>
        /// <returns>A series of characters representing a semantic version</returns>
        public string GetBoardPlatformVersion()
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't GetBoardPlatformVersion!");
                return "";
            }

            return paramsWrapper.GetString("getPlatformVersion");
        }

        [Obsolete("Deprecated in 0.6.18. Vestigial call from AndroidLeiaDevice; use constructor instead. Scheduled for removal in 0.6.20.")]
        public void Initialize(string com_name)
        {
            return;
        }

        public WindowsLeiaDevice(string stubName)
        {
            _displayConfig = GetDisplayConfig();
        }

        [Obsolete("Deprecated in 0.6.18. Users should use Set/Get Backlight(2/3). Scheduled for removal in 0.6.20.")]
        public bool ConvertBacklightModeFromInt(int modeId)
        {
            bool want_backlight_on = false;
            switch (modeId)
            {
                case 2: want_backlight_on = false; break;
                case 3: want_backlight_on = true; break;
                default: Debug.Log("BacklightMode Invalid"); break;
            }
            return want_backlight_on;
        }

        [Obsolete("Deprecated in 0.6.18. Users should use Set/Get Backlight(2/3). Scheduled for removal in 0.6.20.")]
        public int ConvertBacklightModeToInt()
        {
            return 3;
        }
        

        public override void SetBrightnessLevel(char brightness) //Values range fom 0 to 256
        {
            char brightnessChar = (char) brightness;
            cppWrapper.SetValue("setBrightnessLevel", brightnessChar);
        }

        public float GetBrightnessLevel()
        {
            return cppWrapper.GetValue<float>("setBrightnessLevel");
        }

        public override void SetBacklightMode(int modeId)
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't set backlight mode {0}!", modeId);
                return;
            }

            if (BacklightEnforcer.appQuitting)
            {
                Debug.LogFormat("App is quitting, so can't set backlight mode {0}!", modeId);
                return;
            }

            if (modeId == 3) { cppWrapper.SetValue("requestBacklightMode", 1); }
            else if (modeId == 2) { cppWrapper.SetValue("requestBacklightMode", 0); }
            else
            {
                LogUtil.Log(LogLevel.Error, "Do not pass int {0} to {1}", modeId, new System.Diagnostics.StackFrame().GetMethod().Name);
            }
            return;
        }

        public float[] GetBacklightRatios(int mode)
        {
            // API for this is broken. for now, return 0,0
            return new[] { 0.0f, 0.0f };
        }

        /// <summary>
        /// <para>Interpolates between display's 2D light and 3D backlight.</para>
        /// 
        /// <para>No state is tracked or preserved from within LeiaLoft Unity SDK. User must track the value that they 
        /// last set light balance to.</para>
        /// 
        /// <para>The display does remember backlight current. For example:
        ///     in 3D, set backlight balance to 50%,
        ///     then switch to 2D,
        ///     then switch back to 3D
        ///     -> display will return to 3D at a 50%-50% balance.</para>
        /// 
        /// </summary>
        /// <param name="alpha">Intensity of 3D backlight; 0 is very weak, 1 is fully active</param>
        public void SetDisplayLightBalance(float alpha)
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't set light balance to {0}!", alpha);
                return;
            }

            const float max = 16f;
            float a16 = max * Mathf.Clamp01(alpha);
            cppWrapper.SetValue("setDisplayLightBalance", max - a16, a16);
        }

        public override void SetBacklightMode(int modeId, int delay)
        {
            SetBacklightMode(modeId);
        }

        public override void RequestBacklightMode(int modeId)
        {
            SetBacklightMode(modeId);
        }

        public override void RequestBacklightMode(int modeId, int delay)
        {
            SetBacklightMode(modeId);
        }

        public override int GetBacklightMode()
        {
            if (!IsConnected())
            {
                Debug.LogFormat("Not connected to a Leia Display, so can't get backlight mode!");
                return 0;
            }

            int backlightMode = cppWrapper.GetValue<int>("getBacklightMode");
            // these are enums in C++, so enum 0 is 2D and enum 1 is 3D
            if (backlightMode == 0) { return 2; }
            if (backlightMode == 1) { return 3; }

            // should not be possible
            LogUtil.Log(LogLevel.Error, "getBacklightMode returned erroneous value {0}", backlightMode);
            return backlightMode;
        }

        /// <summary>
        /// Data from board. Users should use this to track data coming in from board
        /// </summary>
        /// <returns>A DisplayConfig whose properties are populated by the Leia display/monitor</returns>
        private DisplayConfig GetBoardDisplayConfig()
        {
            Debug.Log("GetBoardDisplayConfig");
            _displayConfig = new DisplayConfig();
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

            if (!IsConnected())
            {
                Debug.Log("Not connected to a Leia Display! Returning default config.");

                _displayConfig = DefaultStandaloneConfig();
                _displayConfig.status = DisplayConfig.Status.FailedToLoadFromDevice;
                return _displayConfig;
            }
            // this code is compact enough now to be moved from GetBoardDisplayConfig to GetDisplayConfig
            const float pixelsToPercent = 10.0f;

            _displayConfig.AlignmentOffset = (XyPair<float>)paramsWrapper.Get1DArray<float>("getAlignmentOffset", xyPairLen);
            _displayConfig.Beta = paramsWrapper.GetValue<float>("getBeta");
            _displayConfig.Gamma = paramsWrapper.GetValue<float>("getGamma");
            _displayConfig.CenterViewNumber = paramsWrapper.GetValue<float>("getCenterViewNumber");

            _displayConfig.ViewboxSize = (XyPair<float>)paramsWrapper.Get1DArray<float>("getViewboxSize", xyPairLen);

            // this field may be (0,0) on some displays
            _displayConfig.DisplaySizeInMm = (XyPair<int>)paramsWrapper.Get1DArray<int>("getDisplaySizeInMm", xyPairLen);
            _displayConfig.PixelPitchInMM = (XyPair<float>)paramsWrapper.Get1DArray<float>("getDotPitchInMm", xyPairLen);
            _displayConfig.UserPixelPitchInMM = (XyPair<float>)paramsWrapper.Get1DArray<float>("getDotPitchInMm", xyPairLen);

            // currently inconsistent units across devices. later we have the option of doing [0,-1,0,1] * [1/w, 1/h, 0,1] or [0,-1/h,0,0]
            _displayConfig.InterlacingVector = new float[] { 0, 0, 0, 0 };
            _displayConfig.InterlacingMatrix = paramsWrapper.GetFlatNxMArray<float>("getInterlacingMatrix", matrixDim, matrixDim);

            //_displayConfig.NumViews = new XyPair<int>( 9 , 1);
            _displayConfig.NumViews = (XyPair<int>)paramsWrapper.Get1DArray<int>("getNumViews", xyPairLen);

            Debug.AssertFormat(_displayConfig.NumViews != null && _displayConfig.NumViews[0] > 2, "Expected NumViews to be more than 2. But NumViews was {0}", _displayConfig.NumViews);

            int actMaxLen = 0;
            if (_displayConfig.NumViews != null) { actMaxLen = _displayConfig.NumViews[0] / 2; }
            float[][] actArr = paramsWrapper.GetNxMArray<float>("getViewSharpeningCoefficients", xyPairLen, actMaxLen);
            _displayConfig.ActCoefficients = new XyPair<List<float>>(new List<float>(actArr[0]), new List<float>(actArr[1]));
            _displayConfig.UserActCoefficients = _displayConfig.ActCoefficients;

            string actCoeffsDebug = "ACT-X Coefficiants:\n";
            int count2 = _displayConfig.ActCoefficients.x.Count;

            for (int i = 0; i < count2; i++)
            {
                actCoeffsDebug += _displayConfig.ActCoefficients.x[i] + ", ";
            }

            actCoeffsDebug += "\n\nACT-Y Coefficiants:\n";
            count2 = _displayConfig.ActCoefficients.y.Count;

            for (int i = 0; i < count2; i++)
            {
                actCoeffsDebug += _displayConfig.ActCoefficients.y[i] + ", ";
            }

            Debug.Log(actCoeffsDebug);

            _displayConfig.SystemDisparityPixels = paramsWrapper.GetValue<int>("getSystemDisparity");
            _displayConfig.SystemDisparityPercent = 1.0f / Mathf.Max(1E-5f, _displayConfig.SystemDisparityPixels) / pixelsToPercent;

            _displayConfig.PanelResolution = (XyPair<int>)paramsWrapper.Get1DArray<int>("getPanelResolution", xyPairLen);
            /*
            // need to catch exception by c++ side.
            try
            {
                _displayConfig.ViewResolution = (XyPair<int>)paramsWrapper.Get1DArray<int>("getViewResolution", xyPairLen);
            }
            catch (Exception)
            {
                Debug.LogError("getViewResolution is not supported.");
                _displayConfig.ViewResolution = new XyPair<int>((int)(1.2 * _displayConfig.PanelResolution[0] / Math.Sqrt(_displayConfig.NumViews[0])),
                                                                (int)(1.2 * _displayConfig.PanelResolution[1] / Math.Sqrt(_displayConfig.NumViews[0])));
            }
            */

            _displayConfig.ViewResolution = new XyPair<int>((int)(1.2 * _displayConfig.PanelResolution[0] / Math.Sqrt(_displayConfig.NumViews[0])),
                                                            (int)(1.2 * _displayConfig.PanelResolution[1] / Math.Sqrt(_displayConfig.NumViews[0])));

            /*
            // getViewResolution is not supported on all Windows firmware or standalone displays. avoid calling getViewResolution
            // the view resolution is very different per display. just add cases as we know them. ideally this will be resolved by FW update later

            if (_displayConfig.PanelResolution[0] == 2400 && _displayConfig.PanelResolution[1] == 900)
            {
                // 8V 2400 x 900 display is 1050 x 394
                _displayConfig.ViewResolution = new XyPair<int>(1050, 394);
            }
            else if (_displayConfig.NumViews[0] == 8 || _displayConfig.NumViews[0] == 12)
            {
                // 8V 2160p display is 720p for continuity reasons, even though it should really be 1680 x 945
                // 12V 2160p display is actually 720p
                _displayConfig.ViewResolution = new XyPair<int>(1280, 720);
            }
            else
            {
                // no known view res; use fallback

                // compute the display aspect ratio
                float panelAspectRatio = _displayConfig.PanelResolution[0] * 1.0f / _displayConfig.PanelResolution[1];

                // compute the view resolution necessary if we were to render 1 pixel in a view for each pixel onscreen
                int resolutionMinor = (int)Mathf.Sqrt(_displayConfig.PanelResolution[0] * _displayConfig.PanelResolution[1] * 1.0f
                    / Mathf.Max(1, _displayConfig.NumViews[0])
                    / Mathf.Max(1E-5f, panelAspectRatio)
                    );

                _displayConfig.ViewResolution = new XyPair<int>((int)(resolutionMinor * panelAspectRatio), resolutionMinor);
            }
            */
            _displayConfig.RenderModes = new List<string>(new[] { "HPO", "2D" });

            // hardcoded and deprecated. to change slant, change interlacingMatrix
            _displayConfig.Slant = true;
            _displayConfig.isSquare = false;
            _displayConfig.isSlanted = true;

            // can be computed on the fly. not necessarily display params
            _displayConfig.UserAspectRatio = _displayConfig.PanelResolution.x / Mathf.Max(1, _displayConfig.PanelResolution.y);
            _displayConfig.UserOrientationIsLandscape = true;

            _displayConfig.VersionNum = _displayConfig.InterlacingMatrix[0];

            Debug.Log("_displayConfig versionNum = " + _displayConfig.VersionNum);

            int count = _displayConfig.InterlacingMatrix.Length;

            if (_displayConfig.VersionNum >= 2.0f)
            {
                Debug.Log("versionNum >= 2.0f");
                // We are loading from the interlace matrix
                _displayConfig.CenterViewNumber = _displayConfig.InterlacingMatrix[1];
                _displayConfig.n = _displayConfig.InterlacingMatrix[2];
                _displayConfig.theta = _displayConfig.InterlacingMatrix[3];
                _displayConfig.s = _displayConfig.InterlacingMatrix[4];
                _displayConfig.d_over_n = _displayConfig.InterlacingMatrix[5];
                _displayConfig.p_over_du = _displayConfig.InterlacingMatrix[6];
                _displayConfig.p_over_dv = _displayConfig.InterlacingMatrix[7];
                _displayConfig.colorSlant = ((int)_displayConfig.InterlacingMatrix[8]);
                _displayConfig.colorInversion = Mathf.Abs(_displayConfig.InterlacingMatrix[9]) > 0.001;
                _displayConfig.cameraCenterX = _displayConfig.InterlacingMatrix[10];
                _displayConfig.cameraCenterY = _displayConfig.InterlacingMatrix[11];
                _displayConfig.Beta = _displayConfig.InterlacingMatrix[12];
                _displayConfig.Gamma = _displayConfig.InterlacingMatrix[13];
                _displayConfig.cameraThetaX = _displayConfig.InterlacingMatrix[14];
            }
            else if (_displayConfig.ActCoefficients.y[0] >= 1000.0f)
            {
                Debug.Log("_displayConfig.ActCoefficients.y[0] >= 1000.0");
                // Fallback, old method of storing in ACT Y coeifficents
                // Only valid IF the first element of ACT Y coefficients is greather than or equal to 10000

                _displayConfig.n = _displayConfig.ActCoefficients.y[0] / 1000.0f;
                _displayConfig.theta = _displayConfig.ActCoefficients.y[1];
                _displayConfig.cameraCenterX = _displayConfig.ActCoefficients.y[2];
                _displayConfig.cameraCenterY = _displayConfig.ActCoefficients.y[3];

                // Check if Beta is valid and if so override
                if (_displayConfig.ActCoefficients.y[4] > 1.0f)
                {
                    _displayConfig.Beta = _displayConfig.ActCoefficients.y[4];
                }

                // Check if IO (ViewBoxSize) is valid and override (FB late change)
                float IO = _displayConfig.ViewboxSize.x;
                if (_displayConfig.ActCoefficients.y[5] > 0.0f)
                    IO = _displayConfig.ActCoefficients.y[5];

                // Step 6: Compute from D and IO the d_over_n and s
                float D = _displayConfig.ConvergenceDistance;
                float du = _displayConfig.PixelPitchInMM.x / _displayConfig.p_over_du;
                _displayConfig.d_over_n = du * D / IO;
                _displayConfig.s = (du / IO) * _displayConfig.PanelResolution.x * _displayConfig.p_over_du;
            }

            _displayConfig.cameraCenterZ = 0.0f;
            _displayConfig.cameraThetaY = 0.0f;
            _displayConfig.cameraThetaZ = 0.0f;
            if (_displayConfig.VersionNum >= 1.9f)
            {
                Debug.Log("versionNum >= 1.9");

                _displayConfig.cameraCenterZ = _displayConfig.ActCoefficients.y[0];
                _displayConfig.cameraThetaY = _displayConfig.ActCoefficients.y[1];
                _displayConfig.cameraThetaZ = _displayConfig.ActCoefficients.y[2];
            }
            else
            {
                Debug.Log("versionNum < 2.1");
                // Flip signs of camera x pos and camera theta X
                _displayConfig.cameraCenterX = -_displayConfig.cameraCenterX;
                _displayConfig.cameraThetaX = -_displayConfig.cameraThetaX;
            }
            
            if (_displayConfig.VersionNum < 2.6f)
            {
                _displayConfig.CenterViewNumber -= _displayConfig.p_over_dv * (Screen.height - 1);
                _displayConfig.CenterViewNumber = PeriodicModulo(_displayConfig.CenterViewNumber , _displayConfig.NumViews.x);
            }

            _displayConfig.status = DisplayConfig.Status.SuccessfullyLoadedFromDevice;
            return _displayConfig;
        }

        
        float PeriodicModulo(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }


        /// <summary>
        /// Tries to get displayConfig from json.
        /// If no json. tries to get from board.
        /// If no board, gets from default.
        /// </summary>
        /// <returns></returns>
        public override DisplayConfig GetDisplayConfig()
        {
            if (!IsConnected())
            {
                Debug.Log("Not connected to a Leia Display! Returning default config.");
                return DefaultStandaloneConfig();
            }

            // typically _displayConfig will be populated and updated in ctor of WinLeiaDevice
            if (_displayConfig == null)
            {
                // do some support-checking first. maybe a support check should be implemented by AbstractArtifactWrapper
                string platformString = GetBoardPlatformVersion();
                // this version of the Unity SDK requires usage of LeiaDisplayParams.DLL and LeiaService 1.2
                List<uint> platformInts = Diagnostics.SDKStringData.ParseAsSemantic(GetBoardPlatformVersion());
                List<uint> requiredInts = new List<uint> { 1, 2 };
                // true if every element of platformInts is >= corresponding value in requiredInts
                bool isSupportedLeiaService = Diagnostics.SDKStringData.IsSemanticPlatformGreaterThanOrEqualTo(platformInts, requiredInts);

                if (!isSupportedLeiaService)
                {
                    LogUtil.Log(LogLevel.Error, "Platform {0} is not supported by this verison of the Unity SDK! Require version {1} or higher!", platformString, "1.2");
                }

                _displayConfig = GetBoardDisplayConfig();
                _displayConfigUnmodified = DisplayConfig.CopyDisplayConfig(_displayConfig);

                // populate _displayConfig from FW with developer-tuned values
                base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);
            }

            // we usually want to skip the update process and return the already-updated displayConfig
            return _displayConfig;
        }

        public override DisplayConfig GetUnmodifiedDisplayConfig()
        {
            return _displayConfigUnmodified;
        }

        [Obsolete("No longer supported. Deprecated in 0.6.18")]
        public static DisplayConfig DefaultStandaloneConfig()
        {
            return new DisplayConfig
            {
                ResolutionScale = 1,
                PixelPitchInMM = new XyPair<float>(0.06084f,0.06084f),
                PanelResolution = new XyPair<int>(3840, 2160),
                NumViews = new XyPair<int>(8, 1),
                AlignmentOffset = new XyPair<float>(0, 0),
                ActCoefficients = new XyPair<List<float>>(new List<float> { 0, 0, 0, 0 }, new List<float> { 0.1f, 0, 0.03f, 0 }),
                ViewResolution = new XyPair<int>(1280, 720),
                DisplaySizeInMm = new XyPair<int>(0, 0),
                SystemDisparityPercent = .0125f,
                SystemDisparityPixels = 8,
                UserOrientationIsLandscape = true,
                UserPixelPitchInMM = new XyPair<float>(0.06084f,0.06084f),
                UserPanelResolution = new XyPair<int>(3840, 2160),
                UserViewResolution = new XyPair<int>(1280, 720),
                UserNumViews = new XyPair<int>(8, 1),
                UserActCoefficients = new XyPair<List<float>>(new List<float> { 0, 0, 0, 0 }, new List<float> { 0.1f, 0, 0.03f, 0 }),
                UserDisplaySizeInMm = new XyPair<int>(0, 0),
                UserAspectRatio = 1.7777777910232545f,
                RenderModes = new List<string> { "HPO", "2D" },
                InterlacingMatrix = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1440, 270, 0.375f, 0.0f },
                InterlacingVector = new float[] { 0, 0, 0, 0 },
                Gamma = 2.2f,
                Slant = true,
                Beta = 1.4f,
                isSlanted = true
            };
        }

        /// <summary>
        /// User should make calls like SetBacklightStatus, GetDisplayConfig, etc. The DLL Communication Layer will get data
        /// </summary>
        /// <returns></returns>
        public override bool IsConnected()
        {
            return cppWrapper.GetValue<bool>("isDisplayConnected");
        }

        public override RuntimePlatform GetRuntimePlatform()
        {
            return RuntimePlatform.WindowsPlayer;
        }
    }
}
