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
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
#if LEIA_HDRP_DETECTED
using UnityEngine.Rendering.HighDefinition;
#endif

#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
using UnityEngine.Rendering.Universal;
#endif

namespace LeiaLoft
{
    /// <summary>
    /// Contains some parameters of Unity camera that should be tracked and transferred to leia view cameras.
    /// </summary>
    public struct UnityCameraParams
    {
        public float FieldOfView { get; set; }
        public float Near { get; set; }
        public float Far { get; set; }
        public float Depth { get; set; }
        public int CullingMask { get; set; }
        public CameraClearFlags ClearFlags { get; set; }
        public Color BackgroundColor { get; set; }
        public bool AllowHDR { get; set; }
        public bool Orthographic { get; set; }
        public float OrthographicSize { get; set; }
        public Rect ViewportRect { get; set; }
        public RenderingPath RenderingPath { get; set; }
        public bool UseOcclusionCulling { get; set; }
#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
        public int ScriptableRendererIndex { get; set; }
        public AntialiasingMode AntialiasingMode { get; set; }
        public AntialiasingQuality AntialiasingQuality { get; set; }
        public bool Dithering { get; set; }
        public bool RenderPostProcessing { get; set; }
        public bool RenderShadows { get; set; }
        public CameraOverrideOption RequiresColorOption { get; set; }
        public bool RequiresColorTexture { get; set; }
        public CameraOverrideOption RequiresDepthOption { get; set; }
        public bool RequiresDepthTexture { get; set; }
        public bool StopNaN { get; set; }
        public LayerMask VolumeLayerMask { get; set; }
        public Transform VolumeTrigger { get; set; }
#endif
    }

    /// <summary>
    /// Script turns regular Unity camera into a Leia holographic camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/unity-sdk-components#leiacamera-component")]
    [AddComponentMenu("LeiaLoft/Leia Camera")]
    public class LeiaCamera : MonoBehaviour, IEnumerable
    {
        public bool cameraZaxisMovement = false;
        public void SetCameraZaxisMovementEnabled(bool cameraZaxisMovement)
        {
            this.cameraZaxisMovement = cameraZaxisMovement;
        }

        LeiaVirtualDisplay _virtualDisplay;
        public LeiaVirtualDisplay virtualDisplay
        {
            get
            {
                if (_virtualDisplay == null)
                {
                    InitializeVirtualDisplay();
                }
                return _virtualDisplay;
            }
        }

        void InitializeVirtualDisplay()
        {
            if (transform.parent != null)
            {
                _virtualDisplay = transform.parent.GetComponent<LeiaVirtualDisplay>();
            }
            if (_virtualDisplay == null)
            {
                GameObject virtualDisplayGameObject = new GameObject("LeiaVirtualDisplay");
                virtualDisplayGameObject.transform.parent = transform;
                _virtualDisplay = virtualDisplayGameObject.AddComponent<LeiaVirtualDisplay>();
            }
        }

        public float eyeTrackingAnimatedBaselineScalar; //smoothly animates to 0 when no faces detected, and to 1 when faces detected, used to scale baseline

        public float FinalBaselineScaling
        {
            get
            {
                return eyeTrackingAnimatedBaselineScalar * BaselineScaling;
            }
        }

        bool CopyLayersToChildCameras = true; //Default is true. Set to false if you want to control each child camera's culling layers independently.
        public enum ViewportRectFillTechnique
        {
            TruncatedRectOfFullRenderTexture, // when Unity is rendering into a sub-section of a full-size renderTexture
            FullRectOfTruncatedRenderTexture // when Unity is rendering into a smaller renderTexture, then stretching pixels into a full-size renderTexture
        }

        // devs should typically use ViewportRectFill. This serialized var just helps with saving components
        [SerializeField] private ViewportRectFillTechnique mViewportRectFillTechnique;

        /// <summary>
        /// User needs to set this technique depending upon which ViewportRectFillTechnique Unity is using
        /// </summary>
        public ViewportRectFillTechnique ViewportRectFill
        {
            get
            {
                return mViewportRectFillTechnique;
            }
            set
            {
                // if _isDirty is not already true, and we changed our fill technique, set _isDirty to true
                _isDirty = _isDirty || !value.Equals(mViewportRectFillTechnique);
                mViewportRectFillTechnique = value;
            }
        }

        private const string MissingLeiaView = "LeiaView gameobjects are autogenerated and required for rendering, removing or disabling is not recommended.";
        private const int LeiaCameraMask = 0;
        private const CameraClearFlags LeiaCameraClearFlags = CameraClearFlags.Depth;

        /// <summary>
        /// First created LeiaCamera
        /// </summary>
        public static LeiaCamera Instance { get; private set; }

        /// <summary>
        /// Caching Unity camera attached
        /// </summary>
        private Camera _camera;
        private UnityCameraParams _previousUnityCameraParams;
        private UnityCameraParams currentUnityCameraParams;

        /// <summary>
        /// Defines whether update is needed at next frame or not.
        /// </summary>
        private bool _isDirty = true;

        private LeiaView[] _leiaViews = null;
        [SerializeField]
        private float _disparityScaling = 1.0f;
        [SerializeField]
        private float _cameraShiftScaling = 1.0f;

        private float _previousDisparityScaling = 1.0f;
        private float _previouscameraShiftScaling = 1.0f;

        [SerializeField]
        private float _convergenceDistance = 10f;
        private float _previousConvergenceDistance = 10f;
        [SerializeField]
        private bool _drawCameraBounds = true;
        [SerializeField]
        private Vector3 _cameraShift = Vector3.zero;
        [SerializeField]
        GameObject _renderObject;

        // this var tracks the root camera's scriptableRendererIndex. used in URP
        private int scriptableRendererIndex;

        // set mLeiaRenderCamera on instantiation. be sure to call LeiaRenderCamera.setLeiaCamera(this)
        private LeiaRenderCamera mLeiaRenderCamera;

        /// <summary>
        /// Returns Unity camera (not used for actual rendering, but parameters affect leia views)
        /// </summary>
        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }
                return _camera;
            }
        }

        /// <summary>
        /// Introduces perspective distortion in two possible directions which produces focal plane tilting
        /// </summary>
        public Vector3 CameraShift
        {
            get
            {
                return _cameraShift;
            }
            set
            {
                this.Trace(string.Format("Set CameraShift: {0}", value));
                _cameraShift = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Determines if view frustum, leiaviews and focus plane should be highlighted in Editor (Scene tab)
        /// </summary>
        public bool DrawCameraBounds
        {
            get
            {
                return _drawCameraBounds;
            }
            set
            {
                _drawCameraBounds = value;
            }
        }

        /// <summary>
        /// Distance to a point where picture converges between all views (becomes focused)
        /// </summary>
        public float ConvergenceDistance
        {
            get
            {
                return _convergenceDistance;
            }
            set
            {
                value = Mathf.Max(1.0e-5f, value);

                if (Math.Abs(_convergenceDistance - value) < 1e-6)
                {
                    return;
                }
                this.Trace(string.Format("Set ConvergenceDistance: {0}", value));

                _convergenceDistance = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Affects distance between LeiaViews
        /// </summary>
        public float BaselineScaling
        {
            get
            {
                return _disparityScaling;
            }
            set
            {
                value = Mathf.Max(1.0e-5f, value);

                if (Math.Abs(_disparityScaling - value) < 1e-6)
                {
                    return;
                }
                this.Trace(string.Format("Set BaselineScaling: {0}", value));

                _disparityScaling = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Affects distance between LeiaViews
        /// </summary>
        public float CameraShiftScaling
        {
            get
            {
                return _cameraShiftScaling;
            }
            set
            {
                value = Mathf.Max(1.0e-5f, value);

                if (Math.Abs(_cameraShiftScaling - value) < 1e-6)
                {
                    return;
                }
                this.Trace(string.Format("Set CameraShiftScaling: {0}", value));

                _cameraShiftScaling = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Causes copying of all the post effects to the leiaviews with all parameters.
        /// Previous leiaview's effects and parameters will be discarded.
        /// </summary>
        public void UpdateEffects()
        {
            if (GetComponent<LeiaPostEffectsController>())
            {
                GetComponent<LeiaPostEffectsController>().ForceUpdate();
            }
        }

        /// <summary>
        /// Prepares array of empty LeiaViews, discarding previous if any
        /// Makes LeiaViews follow top-level Unity cameras "enabled" flag state
        /// </summary>
        public void SetViewCount(int count, bool forceUpdate = false)
        {
            const string leiaViewHorizontalDescriptionToken = "_LeiaViewHorizontalDescription";

            // store some view count info in a vector4 which is accessible in all shaders
            // hopefully later we will store display horizontal view count in this structure at position [0]
            Vector4 leiaCameraShaderDescription = new Vector4(
                0, count,
                +1 * Convert.ToSingle(count % 2 == 0) / 2.0f,
                -1 * Convert.ToSingle(count % 2 == 0) / 2.0f + count / 2
                );

            // when view count is even and our LeiaViewIDs are [0 1 2 3] we want offsets [-1.5 -0.5 +0.5 +1.5]
            // when view count is odd and 1 we want to map [0] to [0]
            // when view count is odd and greater than 1, e.g. [0 1 2 3 4] we want to map to [-2 -1 0 +1 +2]

            // export to all shaders
            Shader.SetGlobalVector(leiaViewHorizontalDescriptionToken, leiaCameraShaderDescription);

            if (GetViewCount() == count && !forceUpdate)
            {
                return;
            }

            ClearViews();
            DisableUnnecessaryViews(count);

            _leiaViews = new LeiaView[count];

            for (var i = 0; i < count; i++)
            {
                var viewsParams = GetUnityCameraParams();

                _leiaViews[i] = new LeiaView(gameObject, viewsParams);
                // additional updates to LeiaViews occur in AbstractLeiaStateTemplate :: UpdateViews
            }

            ToggleLeiaViews(enabled);
        }

        /// <summary>
        /// Gets leiaview from array of LeiaViews by index
        /// </summary>
        public LeiaView GetView(int index)
        {
            if (_leiaViews == null || index >= GetViewCount())
            {
                LogUtil.Log(LogLevel.Error, "Tried to access LeiaCamera._leiaViews[{0}] but this index was not accessible", index);
                return null;
            }
            return _leiaViews[index];
        }

        /// <summary>
        /// In Edit mode destroys all LeiaViews, in Play mode disables and puts into a pool for later reuse.
        /// </summary>
        public void ClearViews()
        {
            this.Debug("ClearViews");

            if (_leiaViews != null)
            {
                for (int i = 0; i < _leiaViews.Length; i++)
                {
                    var view = _leiaViews[i];

                    if (view != null)
                    {
                        _leiaViews[i].Release();
                        _leiaViews[i] = null;
                    }
                }
            }
        }

        /// <summary>
        /// In Edit mode destroys unnecessary LeiaViews, in Play mode disables and puts into a pool for later reuse.
        /// </summary>
        public void DisableUnnecessaryViews(int viewsCount)
        {
            this.Debug(string.Format("DisableUnnecessaryViews( {0})", viewsCount));

            var childrenCameras = GetComponentsInChildren<Camera>(true);
            int viewGameObjects = 0;

            for (int i = 0; i < childrenCameras.Length; i++)
            {
                var childCamera = childrenCameras[i];

                if (childCamera.transform.parent != transform || childCamera.GetComponent<LeiaCamera>() != null) { continue; }

                viewGameObjects++;

                if (!Application.isPlaying && viewGameObjects > viewsCount)
                {
                    DestroyImmediate(childCamera.gameObject);
                }
                else if (childCamera.gameObject.name == LeiaView.ENABLED_NAME)
                {
                    childCamera.gameObject.name = LeiaView.DISABLED_NAME;
                    // preserve the hide flag set in DisableUnnecessaryViews
                    // SetViewCount calls: Clear, Disable, Toggle.
                    // views will be toggled back on (potentially off) in Toggle.
                    childCamera.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    childCamera.gameObject.SetActive(true);
                    childCamera.enabled = false;
                }
            }
        }

        /// <summary>
        /// Gets the count of leia views (0 if leiaView array is empty).
        /// </summary>
        public int GetViewCount()
        {
            return _leiaViews == null ? 0 : _leiaViews.Length;
        }

        /// <summary>
        /// Checks if some leiaView is missing or inactive (which can be caused by external manipulation)
        /// and in such case calls FixMissingOrInactiveLeiaviews()
        /// </summary>
        private void CheckMissingOrInactiveLeiaviews()
        {
            if (_leiaViews == null || Application.isPlaying)
            {
                return;
            }

            foreach (var view in _leiaViews)
            {
                if (view != null)
                {
                    if (!view.Object || !view.Object.activeInHierarchy ||
                        !view.Object.GetComponent<Camera>())
                    {
                        FixMissingOrInactiveLeiaviews();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Recreates views and shows warning
        /// </summary>
        private void FixMissingOrInactiveLeiaviews()
        {
            SetViewCount(_leiaViews.Length, true);
            OnStateChanged();

            UpdateEffects();

            this.Warning(MissingLeiaView);
        }

        /// <summary>
        /// Enables or disables all LeiaViews
        /// </summary>
        private void ToggleLeiaViews(bool enable)
        {
            if (_leiaViews == null) { return; }

            foreach (var view in _leiaViews)
            {
                if (view != null)
                {
                    view.Enabled = enable;
                    if (enable)
                    {
                        // reveal this cam
                        view.Object.hideFlags = HideFlags.None;
                    }
                    else
                    {
                        // hide this cam
                        view.Object.hideFlags = HideFlags.HideInHierarchy;
                        view.Release();
                    }
                }
            }
        }

        /// <summary>
        /// A shortcut to Unity camera near clip plane setting
        /// </summary>
        public float NearClipPlane
        {
            get
            {
                return _camera.nearClipPlane;
            }
        }

        /// <summary>
        /// A shortcut to Unity camera far clip plane setting
        /// </summary>
        public float FarClipPlane
        {
            get
            {
                return _camera.farClipPlane;
            }
        }

        /// <summary>
        /// A shortcut to Unity camera field of view setting
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return _camera.fieldOfView;
            }
        }

        /// <summary>
        /// Issues structure with a slice of current Unity camera params, useful for later comparison
        /// </summary>
        private UnityCameraParams GetUnityCameraParams()
        {
            bool hdr;
#if UNITY_5_6_OR_NEWER
            hdr = _camera.allowHDR;
#else
			hdr = _camera.hdr;
#endif
#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
            UniversalAdditionalCameraData urpData = _camera.GetUniversalAdditionalCameraData();
#endif

            return new UnityCameraParams
            {
                FieldOfView = GetLeiaViewFOV(),
                Near = _camera.nearClipPlane,
                Far = _camera.farClipPlane,
                Depth = _camera.depth,
                CullingMask = _camera.cullingMask,
                ClearFlags = _camera.clearFlags,
                BackgroundColor = _camera.backgroundColor,
                Orthographic = _camera.orthographic,
                OrthographicSize = _camera.orthographicSize,
                ViewportRect = _camera.rect,
                AllowHDR = hdr,
                RenderingPath = _camera.renderingPath,
                UseOcclusionCulling = _camera.useOcclusionCulling
#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
                ,
                ScriptableRendererIndex = this.scriptableRendererIndex,
                AntialiasingMode = urpData.antialiasing,
                AntialiasingQuality = urpData.antialiasingQuality,
                Dithering = urpData.dithering,
                RenderPostProcessing = urpData.renderPostProcessing,
                RenderShadows = urpData.renderShadows,
                RequiresColorOption = urpData.requiresColorOption,
                RequiresColorTexture = urpData.requiresColorTexture,
                RequiresDepthOption = urpData.requiresDepthOption,
                RequiresDepthTexture = urpData.requiresDepthTexture,
                StopNaN = urpData.stopNaN,
                VolumeLayerMask = urpData.volumeLayerMask,
                VolumeTrigger = urpData.volumeTrigger
#endif
            };
        }

        float GetLeiaViewFOV()
        {
            //Calculate virtual display height based on LeiaCamera convergence and field of view
            float virtualDisplayHeight = Mathf.Tan((_camera.fieldOfView / 2f) / Mathf.Rad2Deg) * (2f * ConvergenceDistance);

            //Calculate LeiaViews field of view based on virtual display height and camera shift from eye tracking
            float newFieldOfView = 2f * Mathf.Atan(
                (virtualDisplayHeight) /
                (2f * (ConvergenceDistance - CameraShift.z))
                ) * Mathf.Rad2Deg;

            return newFieldOfView;
        }

        private void FillUnityCameraParams(ref UnityCameraParams ucp)
        {
            bool hdr;
#if UNITY_5_6_OR_NEWER
            hdr = _camera.allowHDR;
#else
			            hdr = _camera.hdr;
#endif
#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
                        UniversalAdditionalCameraData urpData = _camera.GetUniversalAdditionalCameraData();
#endif

            ucp.FieldOfView = GetLeiaViewFOV();
            ucp.Near = _camera.nearClipPlane;
            ucp.Far = _camera.farClipPlane;
            ucp.Depth = _camera.depth;
            if (CopyLayersToChildCameras)
            {
                ucp.CullingMask = _camera.cullingMask;
            }
            else
                Debug.Log("Not copying layers to child views");
            ucp.ClearFlags = _camera.clearFlags;
            ucp.BackgroundColor = _camera.backgroundColor;
            ucp.Orthographic = _camera.orthographic;
            ucp.OrthographicSize = _camera.orthographicSize;
            ucp.ViewportRect = _camera.rect;
            ucp.AllowHDR = hdr;
            ucp.RenderingPath = _camera.renderingPath;
            ucp.UseOcclusionCulling = _camera.useOcclusionCulling;
#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
                 
                ucp.ScriptableRendererIndex = this.scriptableRendererIndex;
                ucp.AntialiasingMode = urpData.antialiasing;
                ucp.AntialiasingQuality = urpData.antialiasingQuality;
                ucp.Dithering = urpData.dithering;
                ucp.RenderPostProcessing = urpData.renderPostProcessing;
                ucp.RenderShadows = urpData.renderShadows;
                ucp.RequiresColorOption = urpData.requiresColorOption;
                ucp.RequiresColorTexture = urpData.requiresColorTexture;
                ucp.RequiresDepthOption = urpData.requiresDepthOption;
                ucp.RequiresDepthTexture = urpData.requiresDepthTexture;
                ucp.StopNaN = urpData.stopNaN;
                ucp.VolumeLayerMask = urpData.volumeLayerMask;
                ucp.VolumeTrigger = urpData.volumeTrigger;
#endif

        }

        /// <summary>
        /// Memorize previous parameters, subscribe to LeiaDisplay's StateChanged event
        /// </summary>
        private void Awake()
        {
            // for now this _camera needs to be set in Awake(). Later this responsibility should be transferred to a ComponentCache
            _camera = GetComponent<Camera>();

            if (_virtualDisplay == null)
            {
                InitializeVirtualDisplay();
            }

            _previousUnityCameraParams = GetUnityCameraParams();

            if (Instance == null)
            {
                Instance = this;
            }

            LeiaDisplay.Instance.StateChanged -= OnStateChanged;
            LeiaDisplay.Instance.StateChanged += OnStateChanged;

            _camera.enabled = false;

            if (_renderObject == null)
            {
                CreateRenderObject();
                UpdateRenderCameraDepth();
            }
        }

        /// <summary>
        /// Invokes initial update to initialize views
        /// </summary>
        private void OnEnable()
        {
            this.Debug("OnEnable");
            this.Trace(gameObject.name);
            this.Trace("ConvergenceDistance: " + ConvergenceDistance);
            this.Trace("BaselineScaling: " + BaselineScaling);

            ToggleLeiaViews(true);
            _isDirty = true;
            CheckPropertiesModificationAndUpdate();

#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
            // this runs a reflection query. we wish to not run this every frame
            scriptableRendererIndex = _camera.GetUniversalAdditionalCameraData().GetRendererIndex();
#endif
        }

        [HideInInspector] public Vector4 peelControls = new Vector4(1, 1, 0, 0);

        void Update()
        {
            float peelZ = peelControls.z;
            // user can control peeling params from this peelControls property on EyeTrackingCameraShift. but data is ultimately passed to SlantedLeiaStateTemplate.peel_ScaleTranslate
            if (LeiaDisplay.Instance.viewPeeling)
            {
                peelZ = Mathf.RoundToInt(peelControls.z);
            }

            AbstractLeiaStateTemplate.peel_ScaleTranslate = new ToggleScaleTranslate(
                Mathf.RoundToInt(peelControls.x),
                peelZ,
                ToggleScaleTranslate.ModificationMode.ON
                );

            LeiaDisplay.Instance.UpdateLeiaState();

            CheckPropertiesModificationAndUpdate();
        }

        /// <summary>
        /// Removes listener from StateChanged event, disables views
        /// </summary>
        private void OnDisable()
        {
            ToggleLeiaViews(false);
        }

        /// <summary>
        /// Removes listener (to avoid errors inside Unity Editor)
        /// </summary>
        private void OnDestroy()
        {
            foreach (LeiaView lv in this)
            {
                lv.Release();
            }

            if (!LeiaDisplay.InstanceIsNull)
            {
                LeiaDisplay.Instance.StateChanged -= OnStateChanged;
            }
        }

        /// <summary>
        /// Method used to subscribe to LeiaDisplays StateChanged event
        /// </summary>
        private void OnStateChanged()
        {
            if (isActiveAndEnabled)
            {
                UpdatePostEffectsController();
                LeiaDisplay.Instance.UpdateViews(this);
            }
        }

        bool AntiAliasing = false;

        /// <summary>
        /// Asks LeiaDisplay to update all LeiaViews if required (if something changed in params)
        /// </summary>
        private void CheckPropertiesModificationAndUpdate()
        {
            CheckMissingOrInactiveLeiaviews();

            FillUnityCameraParams(ref currentUnityCameraParams);

            bool camOrthoMatch = true;
            if (!currentUnityCameraParams.Equals(_previousUnityCameraParams))
            {
                camOrthoMatch = _previousUnityCameraParams.Orthographic == currentUnityCameraParams.Orthographic;
                _previousUnityCameraParams = currentUnityCameraParams;
                _isDirty = true;
                UpdateRenderCameraDepth();
            }

            if (Math.Abs(_convergenceDistance - _previousConvergenceDistance) > 1e-6)
            {
                _isDirty = true;
                _previousConvergenceDistance = _convergenceDistance;
            }

            if (Math.Abs(_disparityScaling - _previousDisparityScaling) > 1e-6)
            {
                _isDirty = true;
                _previousDisparityScaling = _disparityScaling;
            }
#if LEIA_HDRP_DETECTED
    for (int i = 0; i < GetViewCount(); i++)
    {
        HDAdditionalCameraData cameraData = GetView(i).gameObject.GetComponent<HDAdditionalCameraData>();

        if (cameraData == null)
        {
            cameraData = GetView(i).gameObject.AddComponent<HDAdditionalCameraData>();

            HDAdditionalCameraData hdrpData = _camera.GetComponent<HDAdditionalCameraData>();

            System.Reflection.FieldInfo[] fields = cameraData.GetType().GetFields();

            foreach (System.Reflection.FieldInfo field in fields) 
            {
                field.SetValue(cameraData, field.GetValue(hdrpData));
            }
        }
    }
#endif


            AntiAliasing = LeiaDisplay.Instance.AntiAliasing;
#if LEIA_HDRP_DETECTED

                for (int i = 0; i < GetViewCount(); i++)
                {
                    if (GetView(i).gameObject.GetComponent<HDAdditionalCameraData>() != null)
                    {
                        if (LeiaDisplay.Instance.AntiAliasing)
                        {
                            GetView(i).gameObject.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        }
                        else
                        {
                            GetView(i).gameObject.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                        }
                    }
                }

#endif


            if (_isDirty)
            {


                this.Debug("IsDirty detected");
                LeiaDisplay.Instance.UpdateViews(this);

                // while most LeiaCamera properties require just a view update when a camera param changes,
                // when Camera switches ortho <-> perspective, this requires a LeiaState / interlacing update
                if (!camOrthoMatch)
                {
                    LeiaDisplay.Instance.ForceLeiaStateUpdate();
                }

                mLeiaRenderCamera.Camera.rect = new Rect(Vector2.zero, Vector2.one);
                // when rendering into subsection of a LeiaView, we do not need to carry over the the ViewportRect from the root Camera to the RenderCam
                // The LeiaViews are already truncated / square pixels that we can interlace directly into the back buffer
                if (ViewportRectFill == ViewportRectFillTechnique.TruncatedRectOfFullRenderTexture)
                {
                    mLeiaRenderCamera.Camera.rect = new Rect(Vector2.zero, Vector2.one);
                }

                // when rendering into the full space of a smaller LeiaView, we carry over the the ViewportRect from the root Camera to the RenderCam.
                // effectively: LeiaViews render square pixels into smaller RTs, then those square pixels get copied into a chunk of the back buffer
                if (ViewportRectFill == ViewportRectFillTechnique.FullRectOfTruncatedRenderTexture)
                {
                    mLeiaRenderCamera.Camera.rect = currentUnityCameraParams.ViewportRect;
                }

                RefreshViewsParams();
                _isDirty = false;
            }
        }

        private void UpdateRenderCameraDepth()
        {
            const float depthStep = 0.1f;
            mLeiaRenderCamera.Camera.depth = _camera.depth + depthStep;
        }

        /// <summary>
        /// Creates Object that signals rendering
        /// </summary>
        private void CreateRenderObject()
        {
            _renderObject = new GameObject("RenderCamera");
            _renderObject.transform.parent = this.transform;
            _renderObject.transform.localPosition = Vector3.zero;
            Camera _cam = _renderObject.AddComponent<Camera>();
            _cam.CopyFrom(transform.GetComponent<Camera>());

            // when rendering into subsection of a LeiaView, we do not need to carry over the the RenderCam's ViewportRect from the root Camera.
            // The LeiaViews are already truncated
            if (ViewportRectFill == ViewportRectFillTechnique.TruncatedRectOfFullRenderTexture)
            {
                _cam.rect = new Rect(Vector2.zero, Vector2.one);
            }

            // force culling mask to be nothing. the interlacing process will supply all pixels (not rendered layers) that we desire
            _cam.cullingMask = 0x000000000;
            _cam.tag = this.tag;

            mLeiaRenderCamera = _renderObject.AddComponent<LeiaRenderCamera>();
            mLeiaRenderCamera.setLeiaCamera(this);
        }

        public void RefreshViewsParams()
        {
            var viewsParams = GetUnityCameraParams();

            foreach (LeiaView view in this)
            {
                if (view != null)
                {
                    view.RefreshParameters(viewsParams);
                }
            }
        }

        /// <summary>
        /// Updates post effects (copying them to leia views)
        /// </summary>
        private void UpdatePostEffectsController()
        {
            if (enabled && !LeiaDisplay.InstanceIsNull)
            {
                var controller = GetComponent<LeiaPostEffectsController>();

                if (controller == null)
                {
                    controller = gameObject.AddComponent<LeiaPostEffectsController>();
                }
                controller.ForceUpdate();
            }
        }

        /// <summary>
        /// Method for getting LeiaViews inside the LeiaCamera.
        /// </summary>
        /// <returns>
        /// Yields nothing if the LeiaCamera contains no LeiaViews (frame 0).
        /// Returns a single LeiaView in 2D mode.
        /// Yields LeiaViews in HPO mode.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            if (_leiaViews != null && GetViewCount() > 0)
            {
                for (int i = 0; i < GetViewCount(); i++)
                {
                    yield return _leiaViews[i];
                }
            }
        }
    }
}
