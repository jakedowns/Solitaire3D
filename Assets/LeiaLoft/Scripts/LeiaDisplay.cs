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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeiaLoft
{
    /// <summary>
    /// Presents single display object, has state and settings (decorators) that determine rendering mode.
    /// </summary>
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/unity-sdk-components#leiadisplay-component")]
    public partial class LeiaDisplay : Singleton<LeiaDisplay>, IFaceTrackingDisplay
    {
        public enum ACTMODE { SINGLETAP = 0, MULTIVIEW = 1, OFF = 2 };
        public ACTMODE ActMode = ACTMODE.SINGLETAP;

        public float SimulatedFaceX = 0;
        public float SimulatedFaceY = 0;
        public float SimulatedFaceZ = 600;

        public float SWBrightness = 1;
        public bool eyeTrackingStatusBarEnabled = true;
        public void SetEyeTrackingStatusBarEnabled(bool showEyeTrackingStatusBar)
        {
            this.eyeTrackingStatusBarEnabled = showEyeTrackingStatusBar;
        }
        public void SetDisplayBrightness(char brightness)
        {
            this.LeiaDevice.SetBrightnessLevel(brightness);
        }
        public bool UsingSimulatedFacePosition
        {
            set
            {
                UsingSimulatedFacePosition = value;
            }
            get
            {
                return !this.tracker.enabled;
            }
        }
        public float minView;
        public float maxView;
        public float range;
        public bool CameraShiftEnabled = true;
        public void SetCameraShiftEnabled(bool ACTEnabled)
        {
            this.CameraShiftEnabled = ACTEnabled;
        }
        public bool ShaderShiftEnabled = true; //Used for view peeling / LF mode, not used in tracked stereo mode
        public void SetShaderShiftEnabled(bool ACTEnabled)
        {
            this.ShaderShiftEnabled = ACTEnabled;
        }
        public bool ACTEnabled = true;
        public void SetACTEnabled(bool ACTEnabled)
        {
            this.ACTEnabled = ACTEnabled;
        }
        public bool ShowR0Test = false;
        public void SetR0TestEnabled(bool R0TestEnabled)
        {
            this.ShowR0Test = R0TestEnabled;
        }
        public bool PerPixelCorrectionEnabled = true;
        public void SetPerPixelCorrectionEnabled(bool PerPixelCorrectionEnabled)
        {
            this.PerPixelCorrectionEnabled = PerPixelCorrectionEnabled;
        }
#pragma warning disable CS0618 // Type or member is obsolete
        private static string VersionFileName { get { return "VERSION"; } }
        public bool IsMosaicDisplay
        {
            get
            {
                return this.displayConfig.colorSlant != 0;
            }
        }

        //If Auto Render Technique is set to True, then the LeiaDisplay will automatically choose whether
        //to render in Stereo or Lightfield mode based on whether the eye tracking camera is connected or not
        public bool AutoRenderTechnique = true;

        //Whether black views are temporarily enabled or disabled based on eye tracking
        public bool blackViewsTemp = true;

        public void SetTrackerEnabled(bool trackerEnabled)
        {
            this.tracker.enabled = trackerEnabled;
        }

        public void SetBacklightEnabled(bool backlightEnabled)
        {
            if (backlightEnabled)
            {
#if UNITY_ANDROID
                _leiaDevice.SetBacklightMode(3);
#else
                this.DesiredLightfieldMode = LightfieldMode.On;
#endif
            }
            else
            {
#if UNITY_ANDROID
                _leiaDevice.SetBacklightMode(0);
#else
                this.DesiredLightfieldMode = LightfieldMode.Off;
#endif
            }

        }

        private bool _blackViews = false;
        public bool blackViews
        {
            get
            {
                return _blackViews;
            }
            set
            {
                _blackViews = value;
            }
        }

        private bool _closeRangeSafety = true;
        public bool CloseRangeSafety
        {
            get
            {
                return _closeRangeSafety;
            }
            set
            {
                _closeRangeSafety = value;
            }
        }

        public bool viewPeeling
        {
            get
            {
                //When in LF mode, always do view peeling. When in Stereo, always turn off view peeling.
                return this.DesiredRenderTechnique == RenderTechnique.Multiview 
                /// <remove_from_public>
                    || this.DesiredRenderTechnique == RenderTechnique.NoCorrection
                /// </remove_from_public>
                ;
            }
        }

        private bool _dynamicReconvergence = true;
        public bool dynamicReconvergence
        {
            get
            {
                return _dynamicReconvergence;
            }
            set
            {
                _dynamicReconvergence = value;
            }
        }

        private LeiaSettings _settings = null;
        [SerializeField] private bool _isDirty = false;
        private bool detachCallbackForSceneChange;
        // Serialize m_LightfieldMode directly on LeiaDisplay. Future-compatible
        [SerializeField] private LightfieldMode m_DesiredLightfieldMode = LightfieldMode.On;
        [SerializeField] private LightfieldMode m_ActualLightfieldMode = LightfieldMode.On;
        private LightfieldMode m_PreviousLightfieldMode = LightfieldMode.On;

        private ILeiaState _leiaState;
        private LeiaStateFactory _stateFactory = new LeiaStateFactory();
        private LeiaDeviceFactory _deviceFactory = new LeiaDeviceFactory();
        private ILeiaDevice _leiaDevice;

#if UNITY_ANDROID && !UNITY_EDITOR
        private class CNSDKHolder
        {
            private static bool _isInitialized = false;
            private static Leia.SDK _cnsdk = null;
            public static Leia.SDK Get()
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;

                    Leia.LogLevel logLevel = Leia.LogLevel.Debug;
                    try
                    {
                        _cnsdk = new Leia.SDK(logLevel);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                        _cnsdk = null;
                    }
                }

                return _cnsdk;
            }
        }
        public Leia.SDK CNSDK { get { return CNSDKHolder.Get(); } }

        public Leia.Config sdkConfig;
#endif

        private const int CalibratingOffsetMin = -16;
        private const int CalibratingOffsetMax = 16;

#if UNITY_ANDROID && !UNITY_EDITOR
        private float _disparityBackup;
        private float _disparityAnimTime = 0;
        private float _disparityAnimDirection = 0;
        private const float BASELINE_ANIM_PEAK_TIME = 0.5f;
#endif

        /// <summary>
        /// This enum defines the LeiaDevice's backlight on/off state,
        /// which determines whether parallax pixel content gives depth cues to the viewer.
        /// </summary>
        public enum LightfieldMode : int
        {
            Off = 0,
            On = 1

            // do not assume that only values will ever be 0 and 1
        };

        /// <summary>
        /// This enum defines the LeiaDevice's render technique,
        /// which determines rendering order of Leia Views.
        /// </summary>
        public enum RenderTechnique
        {
            Stereo = 0,
            Multiview = 1
            /// <remove_from_public>    
            ,
            NoCorrection = 2
            /// </remove_from_public>    
        };

        public static string HPO { get { return "HPO"; } }
        public static string TWO_D { get { return "2D"; } }
        public static string THREE_D { get { return "3D"; } }

        /// <summary>
        /// Occurs when LeiaDisplay has leiaState or decorators changed.
        /// </summary>
        public event System.Action StateChanged = delegate { };

        /// <summary>
        /// Occurs when LeiaDisplay has leiaState or decorators changed.
        /// Accepts parameters LightfieldMode previousState, LightfieldMode currentState
        // Used to subscribe to specific backlight mode changes instead of explicitly checking LeiaDisplay values.
        /// </summary>
        public event System.Action<LeiaDisplay.LightfieldMode, LeiaDisplay.LightfieldMode> BacklightStateChanged = delegate { };

        /// <summary>
        /// Gets current leia device.
        /// </summary>
        public ILeiaDevice LeiaDevice
        {
            get
            {
                return _leiaDevice;
            }
        }

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                // in edit mode when not playing, do not modify _isDirty. When user sets RenderMode from LeiaDisplayEditor :: ShowRenderMode, do not set _isDirty flag
                // setter can only make IsDirty true. In Update(), after state regen _isDirty is assigned false
                _isDirty = Application.isPlaying && (value || _isDirty);
            }
        }

        /// <summary>
        /// Gets current leiaState factory.
        /// </summary>
        public LeiaStateFactory StateFactory
        {
            get
            {
                return _stateFactory;
            }
        }

        /// <summary>
        /// Gets current leiaDevice factory
        /// </summary>
        public LeiaDeviceFactory DeviceFactory
        {
            get
            {
                return _deviceFactory;
            }
        }

        /// <summary>
        /// Gets settings object (where all LeiaDisplay settings aggregated)
        /// </summary>
        public LeiaSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = FindObjectOfType<LeiaSettings>();
                    if (_settings != null)
                    {
                        _settings.gameObject.hideFlags = HideFlags.None;
                    }

                    if (_settings == null)
                    {
                        GameObject _settingsGO = new GameObject(LeiaSettings.GameObjectName);
                        _settings = _settingsGO.AddComponent<LeiaSettings>();
                    }
                    _settings.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }

                return _settings;
            }
        }

        /// <summary>
        /// Allows user to enable/disable Parallax Auto Rotation animation on LeiaDisplay at runtime.
        ///
        /// To set the LeiaDisplay gameObject's initial value on scene load, use the LeiaDisplay gameObject's
        /// Inspector to set the component's Parallax Auto Rotation.
        ///
        /// True: parallax rotation animations can be processed at runtime on device.
        /// False: new parallax rotation animations will not be processed. (Active animation will still complete.)
        /// </summary>
        [System.Obsolete("Parallax auto-rotation not supported in recent Unity SDKs. This feature will be removed in a future Unity SDK release")]
        public bool ParallaxAutoRotationAnimation
        {
            get
            {
                return Decorators.ParallaxAutoRotation;
            }
            set
            {
                LeiaStateDecorators decos = Decorators;
                decos.ParallaxAutoRotation = value;
                Settings.Decorators = decos;
            }
        }

        private bool _antiAliasing = true;
        /// <summary>
        /// Gives access to AntiaAliasing setting, sets dirty flag when changed
        /// </summary>

        public bool AntiAliasing
        {
            get
            {
                return _antiAliasing;
            }
            set
            {
                _antiAliasing = value;
            }
        }

        private float _originalTimeDelay;
        public float OriginalTimeDelay
        {
            get
            {
                return _originalTimeDelay;
            }
            set
            {
                _originalTimeDelay = value;
            }
        }

        private float _originalSingleTapActCoeff;
        public float OriginalSingleTapActCoeff
        {
            get
            {
                return _originalSingleTapActCoeff;
            }
            set
            {
                _originalSingleTapActCoeff = value;
            }
        }

        /// <summary>
        /// Gives access to ProfileStubName setting, creates new factroy (based on new profile), sets dirty flag when changed
        /// </summary>
        public string ProfileStubName
        {
            get
            {
                return Settings.ProfileStubName;
            }
            set
            {
                Settings.ProfileStubName = value;
                _isDirty = true;

                if (Application.isPlaying)
                {
                    _leiaDevice.SetProfileStubName(value);
                    _stateFactory.SetDisplayConfig(GetDisplayConfig());
                }
            }
        }

        /// <summary>
        /// A getter method for checking if the LeiaDisplay desires the LightfieldMode to be on
        /// </summary>
        /// <returns>True if internal state machine thinks LeiaDisplay wants ActualLightfieldMode == LightfieldMode.On</returns>
        public bool IsLightfieldModeActualOn()
        {
            return m_ActualLightfieldMode == LightfieldMode.On;
        }

        /// <summary>
        /// This property provides a getter/setter for changing the LeiaDisplay's actual pixels and backlight from unlit traditional (Off) to lit parallax (On).
        ///
        /// Setting this property without modifying DesiredLightfieldMode causes the display's status to change, but does not save the user's desired value.
        ///
        /// This property should be expected to replace LeiaStateID.
        /// </summary>
        public LightfieldMode ActualLightfieldMode
        {
            get
            {
                return m_ActualLightfieldMode;
            }
            set
            {
                // this setter replaces the functionality of LeiaStateId
                if (value == LightfieldMode.Off)
                {
                    Settings.LeiaStateId = TWO_D;
                    m_ActualLightfieldMode = LightfieldMode.Off;
                }
                else if (value == LightfieldMode.On)
                {
                    Settings.LeiaStateId = HPO;
                    m_ActualLightfieldMode = LightfieldMode.On;
                }
                RequestLeiaStateUpdate();
            }
        }

        /// <summary>
        /// Get the ActualLightfieldMode as if it were an int. 0 = off, 1 = on
        /// </summary>
        public int ActualLightfieldValue
        {
            get
            {
                return (int)m_ActualLightfieldMode;
            }
            set
            {
                ActualLightfieldMode = (LightfieldMode)value;
            }
        }

        /// <summary>
        /// Gets or sets the actual LeiaState of the display. Leia devices can be forced into 2D mode by
        /// backlight shutoff (thermal), or
        /// Android onscreen text.
        /// </summary>
        [Obsolete("Deprecated in 0.6.18. Use ActualLightfieldMode instead. Scheduled for removal in 0.6.20.")]
        public string LeiaStateId
        {
            get
            {
                return Settings.LeiaStateId;
            }
            set
            {
                // LeiaStateID is now merely an adapter for LeiaDisplayActualLightfieldMode
                if (value == TWO_D)
                    ActualLightfieldMode = LightfieldMode.Off;
                else if (value == THREE_D || value == HPO)
                    ActualLightfieldMode = LightfieldMode.On;
            }
        }

        /// <summary>
        /// A getter method for checking if the LeiaDisplay desires the LightfieldMode to be on
        /// </summary>
        /// <returns>True if internal state machine thinks LeiaDisplay wants DesiredLightfieldMode == LightfieldMode.On</returns>
        public bool IsLightfieldModeDesiredOn()
        {
            return m_DesiredLightfieldMode == LightfieldMode.On;
        }

        /// <summary>
        /// This property provides a getter/setter for changing the LeiaDisplay's onscreen pixels and backlight to 2D / 3D.
        ///
        /// This property should be expected to replace DesiredLeiaStateID.
        /// </summary>
        public LightfieldMode DesiredLightfieldMode
        {
            get
            {
                return m_DesiredLightfieldMode;
            }
            set
            {
                // this setter replaces the functionality of DesiredLeiaStateID
                m_PreviousLightfieldMode = m_DesiredLightfieldMode;
                m_DesiredLightfieldMode = value;
                if (value == LightfieldMode.Off)
                {
                    Settings.DesiredLeiaStateID = TWO_D;
                }
                else if (value == LightfieldMode.On)
                {
                    Settings.DesiredLeiaStateID = HPO;
                }
                else
                {
                    LogUtil.Log(LogLevel.Error, "Unsupported LightfieldMode {0} passed to DesiredDisplayHoloMode", value);
                }

                ActualLightfieldMode = value;
            }
        }

        /// <summary>
        /// Get the DesiredLightFieldMode as if it were an int. 0 = off, 1 = on
        /// </summary>
        public int DesiredLightfieldValue
        {
            get
            {
                return (int)DesiredLightfieldMode;
            }
            set
            {
                DesiredLightfieldMode = (LightfieldMode)value;
            }
        }

        /// <summary>
        /// Specifies a desired LeiaState - "2D", or "HPO". Switches both screen content and device backlight status (if device has a backlight)
        /// </summary>
        [Obsolete("Deprecated in 0.6.18. Use DesiredLightfieldMode instead. Scheduled for removal in 0.6.20.")]
        public string DesiredLeiaStateID
        {
            get
            {
                return Settings.DesiredLeiaStateID;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Warning("Provide a value when setting DesiredLeiaStateID");
                    return;
                }

                // map lowercase to uppercase
                // map {"3d", "3D"} to "HPO"
                value = value.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                if (value.Equals(THREE_D))
                {
                    value = HPO;
                }

                // DesiredLeiaStateID is now merely an adapter for DesiredLightfieldMode
                if (value == TWO_D)
                    DesiredLightfieldMode = LightfieldMode.Off;
                else if (value == HPO)
                    DesiredLightfieldMode = LightfieldMode.On;
            }
        }
        public RenderTechnique DesiredRenderTechnique
        {
            get { return Decorators.RenderTechnique; }
            set
            {
                LeiaStateDecorators decos = Decorators;
                decos.RenderTechnique = value;
                Decorators = decos;
                RequestLeiaStateUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the decorators, when changed - recreates LeiaStateFactory and applies leiaState to views
        /// </summary>
        /// <value>The decorators.</value>
        public LeiaStateDecorators Decorators
        {
            get
            {
                return Settings.Decorators;
            }
            set
            {
                if (Settings.Decorators.Equals(value)) { return; }
                Settings.Decorators = value;
#if UNITY_EDITOR
                // when a property on LeiaDisplay.LeiaSettings.Decorators is updated, force the UI for LeiaDisplay to update
                UnityEditor.EditorUtility.SetDirty(this);
#endif

                if (Application.isPlaying)
                {
                    _stateFactory.SetDisplayConfig(GetDisplayConfig());
                    UpdateLeiaState();
                }
            }
        }

        private void LogVersionAndGeneralInfo()
        {
            var version = Resources.Load<TextAsset>(VersionFileName);

            if (version == null)
            {
                return;
            }

            string logData = string.Format(
                "LeiaLoft Unity SDK Version: {0}\nUnity version: {1}\nCurrent platform: {2}\nIs editor? {3}\n",
                version.text, Application.unityVersion, Application.platform, Application.isEditor);

            // log using LogUtil with Debug priority
            this.Debug(logData);

#if !UNITY_EDITOR
            // in builds which have Debug.unityLogger.logEnabled and didn't strip out log commands, also log through Unity's built-in logger
            Debug.Log(logData);
#endif
        }

        public int[] CalibrationOffset
        {
            get
            {
                return _leiaDevice.CalibrationOffset;
            }
            set
            {
                if (Application.isPlaying)
                {
                    value[1] = Mathf.Clamp(value[1], CalibratingOffsetMin, CalibratingOffsetMax);
                    value[0] = Mathf.Clamp(value[0], CalibratingOffsetMin, CalibratingOffsetMax);
                    _leiaDevice.GetDisplayConfig().AlignmentOffset = new XyPair<float>(value[0], value[1]);
                    _isDirty = true;
                }
            }
        }

        private void OnResume()
        {
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() == 3)
            {
                // on resume, if the LeiaLights DisplaySDK state machine thinks this app was in 3D mode on last frame
                // then _getBacklightMode should be 3. Store in the latest3DFrame flag.
                // In BacklightModeChanged we will discard BacklightModeChanged(2D) callbacks which occur recently after
                // OnResume {GetBacklightMode == 3}
                latest3DFrame = Time.frameCount;
            }

            // on return to app, if desired state is HPO and forced state is not HPO,
            // we are allowed to return to HPO
            if (DesiredLeiaStateID.Equals(HPO) && !LeiaStateId.Equals(HPO))
            {
                DesiredLeiaStateID = HPO;
            }
            else
            {
                _isDirty = true;
            }
        }

        private void OnPause()
        {
            // catch a case with DisplaySDK 7.1 where Hydrogen apps which were minimized using moveTaskToBack would not disengage backlight
#if !UNITY_EDITOR
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() != 2)
            {
                _leiaDevice.SetBacklightMode(2);
            }
#endif
        }

        /// <summary>
        /// Called when a standalone platform changes focus ("active window") to this application
        /// </summary>
        /// <param name="focus">True if focusing on app, false if dropping focus on app</param>
        private void OnApplicationFocus(bool focus)
        {
            // check flag, only set if not already set; save double work in case where OnApplicationFocus and OnApplicationPause both trigger
            if (focus && !_isDirty)
            {
                if (DesiredLightfieldMode == LightfieldMode.On)
                    OnResume();
            }
            else
            {
                OnPause();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (DesiredLightfieldMode == LightfieldMode.On)
                    OnResume();
            }
            else
            {
                OnPause();
            }
        }

        bool initialized = false;
        private void OnEnable()
        {
            if (!initialized)
            {
                DontDestroyOnLoad(gameObject);
                initialized = true;
            }
            ShowR0Test = false;
            if (!FindObjectOfType<BacklightEnforcer>()) //!gameObject.GetComponent<BacklightEnforcer>() && 
            {
                GameObject backlightEnforcer = new GameObject("BacklightEnforcer");
                backlightEnforcer.AddComponent<BacklightEnforcer>();
            }
            LogVersionAndGeneralInfo();
            this.Debug("OnEnable");

#if UNITY_ANDROID
            ActMode = ACTMODE.SINGLETAP;
#else
            ActMode = ACTMODE.MULTIVIEW;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            if (CNSDK != null)
            {
                SetTrackerEnabled(true);
            }
            else
            {
                Debug.LogError("CNSDK is null so cannot enable eye tracking");
            }

            System.Int32 error = CNSDK.GetConfig(out sdkConfig);
            if (error != 0)
            {
                LogUtil.Log(LogLevel.Error, "LeiaSDK Failed to load config from service. Error: {0}", error);
            }
#endif

            AbstractLeiaDevice device;
#if LEIALOFT_CONFIG_OVERRIDE
            device = new OverrideLeiaDevice(OverrideLeiaDevice.DefaultOverrideConfigFilename);
#elif UNITY_ANDROID && !UNITY_EDITOR
            device = new AndroidLeiaDevice(ProfileStubName);
            /// <remove_from_public>
#elif UNITY_STANDALONE_LINUX && !UNITY_EDITOR
            device = new LinuxLeiaDevice(ProfileStubName);
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            device = new WindowsLeiaDevice(ProfileStubName);
            /// </remove_from_public>
#else
            device = new OfflineEmulationLeiaDevice(ProfileStubName);
#endif
            _deviceFactory.RegisterLeiaDevice(device);
            UpdateDevice();
            SceneManager.activeSceneChanged += onSceneChange;

            BlinkTrackingUnityPlugin blink = this.tracker;
        }

        /// <summary>
        /// Gets new device from deviceFactory (providing profile stub name in case if device not available).
        /// Gets profile from new device, sends it to leiaStateFactory.
        /// Gets default LeiaStateId if LeiaStateId is empty.
        /// Applies lState.
        /// </summary>
        public void UpdateDevice()
        {
            this.Debug("UpdateDevice");
            _leiaDevice = _deviceFactory.GetDevice(ProfileStubName);
            _stateFactory.SetDisplayConfig(GetDisplayConfig());

            RequestLeiaStateUpdate();
        }

        private void OnDisable()
        {
            this.Debug("OnDisable");
            if (_leiaState != null)
            {
                _leiaState.Release();
            }

            // set flag for OnSceneChange callback to be detached after OnSceneChange is run
            detachCallbackForSceneChange = true;

        }

        /// <summary>
        /// On App quit, sets backlight off.
        /// Handles case where this scene is being deloaded, and no new scene is being loaded.
        /// </summary>
        private void OnApplicationQuit()
        {
            _leiaDevice.SetBacklightMode(2);
        }

        public void QuitApp()
        {
            _leiaDevice.SetBacklightMode(2);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// If device is rotated, animates disparity scaling to 0, sets proper parallax orientation
        /// and then animates disparity scaling back
        /// </summary>
        private void ProcessParallaxRotation()
        {
            if (Decorators.ParallaxAutoRotation &&
                (Decorators.ShouldParallaxBePortrait !=
                 (Input.deviceOrientation == DeviceOrientation.Portrait ||
                  Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)) &&
                _disparityAnimDirection == 0)
            {
                _disparityAnimDirection = 1;
                _disparityBackup = LeiaCamera.Instance.BaselineScaling;
                _disparityAnimTime = Time.realtimeSinceStartup;
            }

            float timeDifference = Time.realtimeSinceStartup - _disparityAnimTime + 0.001f;

            if (_disparityAnimDirection == 1)
            {
                if (timeDifference < BASELINE_ANIM_PEAK_TIME)
                {
                    LeiaCamera.Instance.BaselineScaling = _disparityBackup * (1.0f - timeDifference/BASELINE_ANIM_PEAK_TIME);
                }
                else
                {
                    _disparityAnimTime = Time.realtimeSinceStartup;
                    _disparityAnimDirection = -1;
                    var tmpDecorator = Decorators;
                    tmpDecorator.ShouldParallaxBePortrait =
                    (Input.deviceOrientation == DeviceOrientation.Portrait ||
                    Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown);
                    Decorators = tmpDecorator;
                    _isDirty = true;
                }
            }
            else if (_disparityAnimDirection == -1)
            {
                if (timeDifference < BASELINE_ANIM_PEAK_TIME)
                {
                    LeiaCamera.Instance.BaselineScaling = _disparityBackup * (timeDifference/BASELINE_ANIM_PEAK_TIME);
                }
                else
                {
                    LeiaCamera.Instance.BaselineScaling = _disparityBackup;
                    _disparityAnimDirection = 0;
                }
            }
        }
#endif

        [SerializeField] private bool _isRainbow = false;
        public bool isRainbow
        {
            get
            {
                return _isRainbow;
            }
        }

        /// <summary>
        /// Render in Play mode, check isDirty flag and (re)Set leiaState
        /// </summary>
        private void Update()
        {
            /*
            //TODO: Figure out how to implement this functionality without breaking the new Unity input system (Unity 2020+)
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.R))
            {
                _isRainbow = !_isRainbow;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && (Input.GetKeyDown(KeyCode.S)))
            {
                _blackViews = !_blackViews;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.T))
            {
                // T = show tiles
                var decos = Decorators;
                decos.ShowTiles = !decos.ShowTiles;
                this.Decorators = decos;
            }*/

            if (_leiaDevice != null && _leiaDevice.HasDeviceOrientationChangedSinceLastQuery())
            {
                _isDirty = true;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            ProcessParallaxRotation();
#else
#endif

            if (_isDirty)
            {
                _isDirty = false;
                DisplayConfig displayConfig = GetDisplayConfig();
                // inside GetDisplayConfig, calculate UserOrientationIsLandscape
                _stateFactory.SetDisplayConfig(displayConfig);
                if (!BacklightEnforcer.appQuitting)
                {
                    RequestLeiaStateUpdate();
                }
            }
        }

        // hopefully later have an interface which any eye tracking can fulfill. Should deliver IEnumerable<Faces> with Faces having {left xyz and direction, right xyz and direction}
        private BlinkTrackingUnityPlugin _tracker;
        public BlinkTrackingUnityPlugin tracker
        {
            get
            {
                if (_tracker == null)
                {
                    _tracker = FindObjectOfType<BlinkTrackingUnityPlugin>();
                    if (_tracker == null)
                    {
                        GameObject blinkPrefab = Resources.Load("Prefabs/BlinkEyeTracking", typeof(GameObject)) as GameObject;
                        if (blinkPrefab == null)
                        {
                            Debug.LogError("Blink prefab is null!");
                        }

                        GameObject blinkGameObject = Instantiate(blinkPrefab);// as GameObject;
                        blinkGameObject.transform.parent = transform;
                        _tracker = blinkGameObject.GetComponent<BlinkTrackingUnityPlugin>();
                    }
                }

                return _tracker;
            }

            set
            {
                _tracker = value;
            }
        }

        public float faceX;
        public float faceY;
        public float faceZ;
        /// <summary>
        /// Use leiaState to render final picture
        /// </summary>
        public void RenderImage(LeiaCamera camera)
        {
            DisplayConfig displayConfig = GetDisplayConfig();

            if (this.UsingSimulatedFacePosition)
            {
                viewerPosition = new Vector3(
                    this.SimulatedFaceX,
                    this.SimulatedFaceY,
                    this.SimulatedFaceZ
                );

                viewerPositionNonPredicted = new Vector3(
                    this.SimulatedFaceX,
                    this.SimulatedFaceY,
                    this.SimulatedFaceZ
                );
            }
            else
            {
                if (dynamicReconvergence && tracker != null)
                {
                    tracker.UpdateFacePosition();
                    viewerPosition = tracker.GetPredictedViewerPosition();
#if UNITY_ANDROID
                    viewerPositionNonPredicted = tracker.GetNonPredictedViewerPosition();
#else
                    viewerPositionNonPredicted = tracker.GetFacePosition();
#endif

                    faceX = viewerPosition.x;
                    faceY = viewerPosition.y;
                    faceZ = viewerPosition.z;
                }
                else
                {
                    faceX = 0;
                    faceY = 0;
                    faceZ = displayConfig.ConvergenceDistance;
                    viewerPosition = new Vector3(0, 0, displayConfig.ConvergenceDistance);
                }
            }

            if (enabled)
            {
                _leiaState.DrawImage(camera, Decorators);
            }
        }

        /// <summary>
        /// When in 2D, forces HPO then back to 2D
        /// When in HPO, forces 2D then back to HPO
        ///
        /// Skips backlight switching.
        ///
        /// This method resolves a specific issue with orthographic / perspective cameras. External users should use
        /// DesiredLeiaStateId = ...
        /// </summary>
        public void ForceLeiaStateUpdate()
        {
            string init = LeiaStateId;
            string conj = (LeiaStateId == HPO ? TWO_D : HPO);

            Settings.LeiaStateId = conj;
            _leiaState = _stateFactory.GetState(LeiaStateId);
            UpdateLeiaState();

            Settings.LeiaStateId = init;
            _leiaState = _stateFactory.GetState(LeiaStateId);
            UpdateLeiaState();
        }

        /// <summary>
        /// Requests new state from current LeiaStateFactory, switches backlight,
        /// updates texture. UpdateLeiaState triggers StateChanged() actions.
        /// </summary>
        private void RequestLeiaStateUpdate()
        {
            if (BacklightEnforcer.appQuitting)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }
            if (_leiaState != null)
            {
                _leiaState.Release();
            }

            // set interlacing state based on LeiaStateId
            _leiaState = _stateFactory.GetState(LeiaStateId);

            // Set backlight state based on LeiaStateId
            /*
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() == 3 && !LeiaStateId.Equals(HPO))
            {
                _leiaDevice.SetBacklightMode(_leiaState.GetBacklightMode());
            }
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() != 3 && LeiaStateId.Equals(HPO))
            {
                _leiaDevice.SetBacklightMode(_leiaState.GetBacklightMode());
            }
            */
            if (_leiaDevice != null)
            {
                if (this.DesiredLightfieldMode == LightfieldMode.On)
                {
                    _leiaDevice.SetBacklightMode(3);
                }
                else
                {
                    _leiaDevice.SetBacklightMode(2);
                }
            }

            if (_leiaDevice.GetBacklightMode() == 3)
            {
                latest3DFrame = Time.frameCount;
            }

            // private member RequestLeiaStateUpdate() calls public member UpdateLeiaState
            // UpdateLeiaState is called by LeiaConfigSettingsUI
            UpdateLeiaState();
        }

        public void UpdateLeiaState()
        {
            // this overwrites fields of displayPropertyData. this data represents display physical params like BLU stretch

            _leiaState.UpdateState(Decorators, LeiaDevice);

            // if not dirty, and state matches backlight, then we are ready to trigger StateChanged callback
            bool matchedState = !_isDirty &&
                ((LeiaStateId == HPO && _leiaState.GetBacklightMode() == 3) ||
                (LeiaStateId != HPO && _leiaState.GetBacklightMode() != 3));

            if (matchedState && StateChanged != null)
            {
                StateChanged();
                BacklightStateChanged(m_PreviousLightfieldMode, ActualLightfieldMode);
            }
        }

        /// <summary>
        /// This method is called by the LeiaLights Display SDK when the device's backlight engages or disengages. Do not call this method.
        ///
        /// Examples of reasons for LeiaLights DisplaySDK to call this method:
        ///     thermal callback caused backlight to turn off
        ///     app reopened, and LeiaLights DisplaySDK set backlight back to last known state that it thought the app was in
        ///     user dragged down Android status bar on their device
        /// </summary>
        /// <param name="mode">2D if firmware sets backlight off, 3D if firmware sets backlight on</param>
        private int latest3DFrame = -1;
        private void BacklightModeChanged(string mode)
        {
            if (string.IsNullOrEmpty(mode) || !(mode == THREE_D || mode == TWO_D))
            {
                LogUtil.Log(LogLevel.Warning, "Symbol {0} is not a valid backlight state", mode);
                return;
            }
            mode = mode.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

            if (mode == THREE_D && DesiredLeiaStateID != HPO)
            {
                DesiredLeiaStateID = HPO;
            }
            else if (mode == TWO_D && LeiaStateId == HPO && latest3DFrame <= Time.frameCount - 1)
            {
                // suppress if we recently set to 3D.

                // if not recently set to 3D, then we can switch content to 2D to match device's unlit state
                LeiaStateId = TWO_D;
            }
        }

        /// <summary>
        /// Newer versions of LeiaLights SDK / DisplaySDK AAR 7.x call
        /// LeiaDisplay :: onBacklightModeChanged instead of
        /// LeiaDisplay :: BacklightModeChanged. These calls can be forwarded to BacklightModeChanged
        /// </summary>
        /// <param name="mode">2D if firmware has turned backlight off, 3D if firmware has turned backlight on</param>
        void onBacklightModeChanged(string mode)
        {
            BacklightModeChanged(mode);
        }

        public DisplayConfig displayConfig;

        public void ReloadDisplayConfig()
        {
            displayConfig = null;
            GetDisplayConfig();
        }

        public DisplayConfig GetDisplayConfig()
        {
            if (displayConfig != null)
            {
                return displayConfig;
            }
#if UNITY_EDITOR
            displayConfig = _deviceFactory.GetDevice(ProfileStubName).GetDisplayConfig();
            displayConfig.UserOrientationIsLandscape = _leiaDevice == null ? true : _leiaDevice.IsScreenOrientationLandscape();

            ValidateConfig(displayConfig);
            return displayConfig;
#else
            if (_leiaDevice != null)
            {
                displayConfig = _leiaDevice.GetDisplayConfig();
                displayConfig.UserOrientationIsLandscape = _leiaDevice == null ? true : _leiaDevice.IsScreenOrientationLandscape();
            }
#endif
            if (displayConfig == null)
            {
                displayConfig = new DisplayConfig();
                ValidateConfig(displayConfig);
                return displayConfig;
            }

            ValidateConfig(displayConfig);
            return displayConfig;
        }

        public DisplayConfig GetUnmodifiedDisplayConfig()
        {
            DisplayConfig config = _leiaDevice.GetUnmodifiedDisplayConfig();
            if (config == null)
            {
                return new DisplayConfig();
            }
            return config;
        }

        void ValidateConfig(DisplayConfig config)
        {
            if (config.PanelResolution.y == 0)
            {
                Debug.LogError("config.PanelResolution.y = 0. This should never happen. Check config file.");
            }

            if (config.PixelPitchInMM.y == 0)
            {
                Debug.LogError("config.DotPitchInMm.y = 0. This should never happen. Check config file.");
            }
        }

        public bool IsConnected()
        {
            return LeiaDevice.IsConnected();
        }

        /// <summary>
        /// Updates the views by applying leiaState UpdateViews method and sets renderTextures from texturePool.
        /// </summary>
        public void UpdateViews(LeiaCamera camera)
        {
            if (enabled)
            {
                this.Debug("UpdateViews");
                _leiaState.UpdateViews(camera);
            }
        }

        private void onSceneChange(Scene scene, Scene scene2)
        {
            /*
            bool containsLeia;

            GameObject[] gameObjects = scene2.GetRootGameObjects();
            LeiaDisplay leiaDisp = null;
            for (int i = 0; i < gameObjects.Length; i++)
            {
                leiaDisp = gameObjects[i].GetComponentInChildren<LeiaDisplay>();
                if (leiaDisp != null) { break; }
            }
            containsLeia = (leiaDisp != null);

            if (!containsLeia)
            {
                LeiaDevice.SetBacklightMode(2, 1);
            }
            if (detachCallbackForSceneChange)
            {
                SceneManager.activeSceneChanged -= onSceneChange;
            }

            if (detachCallbackForSceneChange)
            {
                SceneManager.activeSceneChanged -= onSceneChange;
            }
            */
        }
#pragma warning restore CS0618 // Type or member is obsolete

        #region dynamic_interlacing_display

        public Vector3 viewerXYZ
        {
            get
            {
                return viewerPosition;
            }
        }

        public float displayConvergenceDistance
        {
            get
            {
                return this.GetDisplayConfig().ConvergenceDistance;
            }
        }

        public float theta
        {
            get
            {
                return this.GetDisplayConfig().theta;
                //return displayPropertyData.display.theta;
            }
        }

        public float pixelPitch
        {
            get
            {
                return this.GetDisplayConfig().PixelPitchInMM[0];
            }
        }

        public float n
        {
            get
            {
                return this.GetDisplayConfig().n;
            }
        }

        public float s
        {
            get
            {
                return this.GetDisplayConfig().s / (this.GetDisplayConfig().PanelResolution.x * this.GetDisplayConfig().p_over_du);
            }
        }

        public float p_over_du
        {
            get
            {
                return this.GetDisplayConfig().p_over_du;
            }
        }

        public float d_over_n
        {
            get
            {
                return this.GetDisplayConfig().d_over_n;
            }
        }

        [SerializeField] public Vector3 viewerPositionNonPredicted = new Vector3(0, 0, 535.964f);
        [SerializeField] public Vector3 viewerPosition = new Vector3(0, 0, 535.964f);

        // fields updated in UpdateLeiaState()
        /*
        [SerializeField]
        public displayPropertyDataSchema displayPropertyData = new displayPropertyDataSchema()
        {
            display = new displayPhysicalDataSchema();
            display.pixelPitch = this.GetDisplayConfig().DotPitchInMm[0];
            display.n = this.GetDisplayConfig().ActCoefficient.y[0]/1000.0f;
            display.s = this.GetDisplayConfig().UserDotPitchInMM[0] / (3f * this.GetDisplayConfig().UserViewResolution.x);
            display.d_over_n = s * this.GetDisplayConfig().ConvergenceDistance;
        };*/

        [SerializeField] ToggleScaleTranslate stretchTS = new ToggleScaleTranslate(1, 0, ToggleScaleTranslate.ModificationMode.ON);
        public float getStretch()
        {
            return stretchTS * getStretch(0, viewerXYZ.z);
        }

        public float getStretch(int faceIndex, float z)
        {
            // see https://github.com/leaiss/orbital_player/blob/77ef952176ea204dd551499a3a3fae9cd6427a44/orbitalplayer/orbitalplayer.cpp#L2469
            // based on https://leia3d.atlassian.net/wiki/spaces/LIT/pages/2930245711/Independent+variables+to+describe+view+geometry, s is stretch coefficient
            // there is no "local_s"; there is display s, which is already s = pixelPitch / (3.0*ViewboxSize[0])
            float D = d_over_n / s;
            float calculated_stretch = (d_over_n) / z;
            float orig_stretch = (d_over_n) / D;
            return 1.0f - (orig_stretch - calculated_stretch);
        }

        // see calling code for getStretch at https://github.com/leaiss/orbital_player/blob/1045d08db22acc8890fa4742a9de5ad168d94b07/orbitalplayer/orbitalplayer.cpp#L2467

        // needs to be moved to HLSL https://github.com/leaiss/orbital_player/blob/67c329119be68679efe20754b1a7e2a2384f383b/orbitalplayer/shaders/prediction_shader.glsl#L33

        [SerializeField] private bool correctViewIndexForViewerPosition = true;
        [SerializeField] private KeyCode ctrlToggleCorrection = KeyCode.U;

        // defalt scale 0.12, transation 0.0. used to transform the result of getN
        [SerializeField] public ToggleScaleTranslate viewXYOffsetTS = new ToggleScaleTranslate(0.12f, 0.0f, ToggleScaleTranslate.ModificationMode.ON);

        public bool round;

        public void SetRound(bool round)
        {
            this.round = round;
            Debug.Log("round = " + round);
        }

        public bool roundViewPeel = true;

        public void SetRoundViewPeel(bool roundViewPeel)
        {
            this.roundViewPeel = roundViewPeel;
        }

        public float numViews; //FOR DEBUGGING DELETE THIS LATER
        public float No; //FOR DEBUGGING DELETE THIS LATER

        float _displayOffset;
        public float displayOffset
        {
            get
            {
                if (_displayOffset == 0)
                {
                    numViews = GetDisplayConfig().NumViews[0];

                    No = GetDisplayConfig().CenterViewNumber;
                    if (round)
                    {
                        _displayOffset = Mathf.Round((GetDisplayConfig().NumViews[0] - 1f) / 2f - No);
                    }
                    else
                    {
                        _displayOffset = (GetDisplayConfig().NumViews[0] - 1f) / 2f - No;
                    }
                }
                return _displayOffset;
            }
        }

        /// <summary>
        /// Slides views around by a continuous amount depending upon viewer position.
        ///
        /// Might need a scale correction.
        ///
        /// Later user should be able to get ith view offset based upon ith face position
        /// </summary>
        /// <returns></returns>
        // see https://github.com/leaiss/orbital_player/blob/9d6e6bd71f6c3e0236f9c64f164449258bd8f685/orbitalplayer/orbitalplayer.cpp#L3994

        float getPeelOffset()
        {
            if (tracker != null && !tracker.enabled)
            {
                return 0;
            }

            float No = this.GetDisplayConfig().CenterViewNumber;
            float PeelOffset = (getN(viewerPosition.x, viewerPosition.y, viewerPosition.z, 0f, 0f)
                - No);

            if (Display.displays.Length > 1) //Hacky fix for multiple displays view offset issue
            {
                PeelOffset -= 1;
            }
            return PeelOffset;
        }

        public float getPeelOffsetForShader()
        {
            return getPeelOffset(); // + displayOffset;
        }

        public float getLastPeelOffsetForShader()
        {
            float returnValue = lastOffsetForShader;
            lastOffsetForShader = getPeelOffsetForShader();
            return returnValue;
        }

        float lastOffsetForShader;

        public bool cameraDriven;
        //public bool enableZCameraShift;

#if UNITY_ANDROID && !UNITY_EDITOR
        public void UpdateCNSDKConfig()
        {
            System.Int32 error = CNSDK.SetConfig(in sdkConfig);
            if (error != 0)
            {
                LogUtil.Log(LogLevel.Error, "LeiaSDK Failed to set config. Error: {0}", error);
            }
        }
#endif
        public Vector3 getPeelOffsetForCameraShift(LeiaCamera leiacamera)
        {
            if (this.viewPeeling)
            {
                return new Vector3(
                    getPeelOffset(),
                    0,
                    0
                );
            }
            DisplayConfig config = this.GetDisplayConfig();

            float realDisplayWidth = config.PanelResolution.x * config.PixelPitchInMM.x;
            float realDisplayHeight = config.PanelResolution.y * config.PixelPitchInMM.y;

            //Display height virtual over display height real
            float hv_over_hr = leiacamera.virtualDisplay.Height / realDisplayHeight;
            //Display width virtual over display width real
            float wv_over_wr = leiacamera.virtualDisplay.Width / realDisplayWidth;

            float manualZCompensation = 1.0f;

            if (!leiacamera.cameraZaxisMovement)
            {
                manualZCompensation = this.displayConfig.ConvergenceDistance / viewerPositionNonPredicted.z;
            }

            return new Vector3(
                viewerPositionNonPredicted.x * wv_over_wr * manualZCompensation * leiacamera.CameraShiftScaling,
                viewerPositionNonPredicted.y * hv_over_hr * manualZCompensation * leiacamera.CameraShiftScaling,
                0
                );
        }

        /// <summary>
        /// My suspicion: this calculates a view index offset from viewer position, in "view units". IE every 1-unit-increase in getN's return value is +1 view index, 
        /// </summary>
        /// <returns></returns>
        /// original https://github.com/leaiss/orbital_player/blob/98568387e62477a904fac87ad490a955b5e8dad1/orbitalplayer/orbitalplayer.cpp#L3975
        public float getN(float x, float y, float z, float x0, float y0)
        {
            float pixelPitch = this.pixelPitch;
            float fullWidth = GetDisplayConfig().PanelResolution.x;
            float fullHeight = GetDisplayConfig().PanelResolution.y;

            //float theta_n = 0; //displayPropertyData.display.theta;
            float stretch_n = this.s;
            float theta_n = this.theta / (fullHeight * 3.0f);

            //float stretch_n = displayPropertyData.display.s / (fullWidth * 3.0f);
            float No = GetDisplayConfig().CenterViewNumber; // displayPropertyData.display.offsetX; // CenterView !!!!!
                                                            // float theta_c = displayPropertyData.display.theta;

            //float du = this.pixelPitch / 3.0f; // 3.0f probably could be interlacing_matrix[12] * viewCount / panelResolution.x
            //float dv = GetDisplayConfig().InterlacingMatrix[14] > 0.0f ? +pixelPitch : -pixelPitch; // "boolean isPositiveSlant" is probably better represented as a float interlacing_matrix[13] * viewCount / panelResolution.y. currently we assume slant is not 0 and has no scale

            float du = pixelPitch / GetDisplayConfig().p_over_du;
            float dv = pixelPitch / GetDisplayConfig().p_over_dv;

            // note: sign changed on main for this calc

            // assume there are no issues with angular wrap-around of operations like angle - angle
            float dx = s * x0 + (Mathf.Cos(theta_n) - 1.0f) * x0 - Mathf.Sin(theta_n) * y0;
            float dy = s * y0 + (Mathf.Cos(theta_n) - 1.0f) * y0 + Mathf.Cos(theta_n) * x0;

            float n = this.n;
            float denom = Mathf.Sqrt(z * z + (1 - 1.0f / (n * n)) * ((x - x0) * (x - x0) + (y - y0) * (y - y0)));

            float u = dx + d_over_n * (x - x0) / denom;
            float v = dy + d_over_n * (y - y0) / denom;

            float N = No + u / du + v / dv;
            /*
            Debug.LogFormat(
                "pixelPitch = {0}\n" +
                "fullWidth = {1}\n" +
                "fullHeight = {2}\n" +
                "theta_n = {3}\n" +
                "stretch_n = {4}\n" +
                "No = {5}\n" +
                "du = {6}\n" +
                "dv = {7}\n" +
                "n = {8}\n" +
                "d_over_n = {9}\n" +
                "N = {10}\n" +
                "u = {11}\n" +
                "v = {12}\n" +
                "dx = {13}\n" +
                "dy = {14}\n" +
                "cameraCenterX = {15}" +
                "cameraCenterY = {16}" +
                "cameraCenterZ = {17}" +
                "cameraThetaX = {18}" +
                "cameraThetaY = {19}" +
                "cameraThetaZ = {20}" ,
                pixelPitch,
                fullWidth,
                fullHeight,
                theta_n,
                stretch_n,
                No,
                du,
                dv,
                n,
                d_over_n,
                N,
                u,
                v,
                dx,
                dy,
                GetDisplayConfig().cameraCenterX,
                GetDisplayConfig().cameraCenterY,
                GetDisplayConfig().cameraCenterZ,
                GetDisplayConfig().cameraThetaX,
                GetDisplayConfig().cameraThetaY,
                GetDisplayConfig().cameraThetaZ
                );
            */
            return N;
        }

        #endregion
    } // end LeiaDisplay

    /// <summary>
    /// A class for applying scale, translate, and/or zero-ing operations to floats
    /// </summary>
    [System.Serializable]
    public class ToggleScaleTranslate
    {
        public enum ModificationMode
        {
            ON, // F(x) returns x * s + t
            ZERO, // F(x) returns 0
            NOSHIFT // aka identity or pass-through. F(x) returns x
        }

        public ToggleScaleTranslate(float s, float t, ModificationMode m)
        {
            scale = s;
            offset = t;
            mode = m;
        }

        [SerializeField] public float scale = 1.0f;
        [SerializeField] public float offset = 0.0f;

        [SerializeField] public ModificationMode mode;

        public static float operator *(ToggleScaleTranslate left, float right)
        {
            if (left.mode == ModificationMode.ZERO) return 0.0f;
            if (left.mode == ModificationMode.NOSHIFT) return right;

            // case ON: apply scale then translation
            return (left.scale * right + left.offset);
        }
    }


} // end LeiaLoft namespace
