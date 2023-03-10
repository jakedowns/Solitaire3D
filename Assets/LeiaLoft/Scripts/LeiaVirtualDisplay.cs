using UnityEngine;
using LeiaLoft;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LeiaVirtualDisplay : MonoBehaviour
{
    [SerializeField] private float height = 5f;
    [SerializeField] private bool ShowAtRuntime = false;

    public float width
    {
        get
        {
            return this.height * Screen.width / Screen.height;
        }
    }

    float convergenceSmoothed;

    public void SetHeight(float height)
    {
        this.height = height;
    }
    
    public float Width
    {
        get
        {
            return this.width;
        }
    }

    public float Height
    {
        get
        {
            return this.height;
        }
        set
        {
            this.height = Mathf.Clamp(value, .0001f, float.MaxValue);
        }
    }

    [HideInInspector]
    public Transform[] corners;
    [HideInInspector,SerializeField] private Transform[] sides;
    [HideInInspector,SerializeField] private Transform logo;

    Transform _model;
    Transform model
    {
        get
        {
            if (_model == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).name.Contains("LeiaVirtualDisplayModel"))
                    {
                        _model = transform.GetChild(i);
                        _model.transform.parent = transform;
                        _model.localPosition = Vector3.zero;
                        break;
                    }
                }
                if (_model == null)
                {
                    GameObject LeiaVirtualDisplayModel = Instantiate(Resources.Load("LeiaVirtualDisplayModel")) as GameObject;
                    _model = LeiaVirtualDisplayModel.transform;
                    _model.transform.parent = transform;
                    _model.localPosition = Vector3.zero;
                }
            }

            return _model;
        }
    }

    public enum ControlMode { DrivesLeiaCamera, DrivenByLeiaCamera };

    private ControlMode _controlMode = ControlMode.DrivesLeiaCamera;

    public ControlMode controlMode
    {
        get
        {
            return _controlMode;
        }
        set
        {
            _controlMode = value;
        }
    }

    ControlMode controlModePrev = ControlMode.DrivesLeiaCamera;

    LeiaCamera _leiaCamera;
    public LeiaCamera leiaCamera
    {
        get
        {
            if (_leiaCamera == null)
            {
                _leiaCamera = GetComponentInChildren<LeiaCamera>();
                if (_leiaCamera != null)
                {
                    _controlMode = ControlMode.DrivesLeiaCamera;
                }
                else
                {
                    _leiaCamera = transform.parent.GetComponent<LeiaCamera>();
                    _controlMode = ControlMode.DrivenByLeiaCamera;
                    if (_leiaCamera == null)
                    {
                        _leiaCamera = FindObjectOfType<LeiaCamera>();
                    }
                }
            }

            return _leiaCamera;
        }
    }

    private float timerForBacklightSwitch = 0;
    private float targetTimeForBacklightSwitch = 3.0f;
    void Start()
    {
        /*
        if (LeiaDisplay.Instance != null)
        {
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            float displayHeightMM = config.PanelResolution.y * config.DotPitchInMm.y;
            baselineScaleStereo = 63 * height / displayHeightMM;
        }*/

        UpdateDisplayGizmos();
    }

    void UpdateDisplayGizmos()
    {
        if (model != null)
        {
            if (logo == null)
            {
                logo = model.Find("LeiaLogo");
            }

            if (sides == null || sides.Length == 0 || sides[0] == null)
            {
                sides = new Transform[4];
                sides[0] = model.Find("Side1");
                sides[1] = model.Find("Side2");
                sides[2] = model.Find("Side3");
                sides[3] = model.Find("Side4");

                if (sides[0] == null)
                {
                    sides[0] = model.GetChild(4);
                    sides[1] = model.GetChild(5);
                    sides[2] = model.GetChild(6);
                    sides[3] = model.GetChild(7);
                }
            }
            if (corners == null || corners.Length == 0 || corners[0] == null)
            {
                corners = new Transform[4];
                corners[0] = model.Find("Corner1");
                corners[1] = model.Find("Corner2");
                corners[2] = model.Find("Corner3");
                corners[3] = model.Find("Corner4");

                if (corners[0] == null)
                {
                    corners[0] = model.GetChild(0);
                    corners[1] = model.GetChild(1);
                    corners[2] = model.GetChild(2);
                    corners[3] = model.GetChild(3);
                }
            }
        }

        if (Application.isPlaying)
        {
            LeiaDisplay.Instance.cameraDriven = (this.controlMode == ControlMode.DrivenByLeiaCamera);
        }

        if (height < .0001f)
        {
            height = .0001f;
        }

        if (this.controlMode == ControlMode.DrivenByLeiaCamera)
        {
            height = Mathf.Tan((leiaCamera.Camera.fieldOfView / 2f) / Mathf.Rad2Deg) * (2f * leiaCamera.ConvergenceDistance);
            transform.localPosition = new Vector3(0, 0, leiaCamera.ConvergenceDistance);
        }

        if (this.controlMode == ControlMode.DrivesLeiaCamera)
        {
            if (transform.localScale.x != 1)
            {
                this.height *= transform.localScale.x;
                transform.localScale = Vector3.one;
            }
            if (transform.localScale.y != 1)
            {
                this.height *= transform.localScale.y;
                transform.localScale = Vector3.one;
            }
            if (transform.localScale.z != 1)
            {
                this.height *= transform.localScale.z;
                transform.localScale = Vector3.one;
            }
        }
        float thickness = this.height * .05f;

        if (Application.isPlaying)
        {
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            float displayHeightMM = config.PanelResolution.y * config.PixelPitchInMM.y;

            if (LeiaDisplay.Instance.tracker.faceTransitionState == BlinkTrackingUnityPlugin.FaceTransitionState.ReducingBaseline)
            {
                leiaCamera.eyeTrackingAnimatedBaselineScalar += (0 - leiaCamera.eyeTrackingAnimatedBaselineScalar) * Mathf.Min((Time.deltaTime * 5f), 1f);
                if (leiaCamera.eyeTrackingAnimatedBaselineScalar < .1f)
                {
                    LeiaDisplay.Instance.tracker.faceTransitionState = BlinkTrackingUnityPlugin.FaceTransitionState.SlidingCameras;
#if UNITY_ANDROID
                    timerForBacklightSwitch += Time.deltaTime;
                    if (timerForBacklightSwitch >= targetTimeForBacklightSwitch)
                        LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
#endif
                }
            }
            else if (LeiaDisplay.Instance.tracker.faceTransitionState == BlinkTrackingUnityPlugin.FaceTransitionState.SlidingCameras)
            {
                leiaCamera.eyeTrackingAnimatedBaselineScalar = 0;
            }
            else if (LeiaDisplay.Instance.tracker.faceTransitionState == BlinkTrackingUnityPlugin.FaceTransitionState.IncreasingBaseline)
            {
#if UNITY_ANDROID
                timerForBacklightSwitch = 0;
                LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
#endif
                leiaCamera.eyeTrackingAnimatedBaselineScalar += (1 - leiaCamera.eyeTrackingAnimatedBaselineScalar) * Mathf.Min((Time.deltaTime * 5f), 1f);

                if (Mathf.Abs(leiaCamera.eyeTrackingAnimatedBaselineScalar - 1) < .1f)
                {
                    LeiaDisplay.Instance.tracker.faceTransitionState = BlinkTrackingUnityPlugin.FaceTransitionState.FaceLocked;
                }
            }
            else
            {
                leiaCamera.eyeTrackingAnimatedBaselineScalar = 1;
            }

            if (this.controlMode == ControlMode.DrivesLeiaCamera)
            {
                if (Application.isEditor)
                    return;
                //Set camera Z position based on eye tracking
                float d = LeiaDisplay.Instance.tracker.faceZ * (height) / displayHeightMM;
                convergenceSmoothed += (d - convergenceSmoothed) * Mathf.Min((Time.deltaTime * 15f), 1f);
                if (leiaCamera.cameraZaxisMovement)
                {
                    //Set camera z-position
                    leiaCamera.ConvergenceDistance = convergenceSmoothed;
                }
                else
                {
                    leiaCamera.ConvergenceDistance = LeiaDisplay.Instance.displayConfig.ConvergenceDistance * (height) / displayHeightMM;
                }

                leiaCamera.transform.localPosition = new Vector3(0, 0, -leiaCamera.ConvergenceDistance);

            }
            else
            {
                //LeiaDisplay.Instance.tracker.faceTransitionState = BlinkTrackingUnityPlugin.FaceTransitionState.FaceLocked;
            }
        }

        if (this.controlMode == ControlMode.DrivesLeiaCamera)
        {
            //Set the camera's field of view
            leiaCamera.Camera.fieldOfView = 2f * Mathf.Atan(
                (height) /
                (2f * leiaCamera.ConvergenceDistance)
                ) * Mathf.Rad2Deg;

            if (!Application.isPlaying)
            {
                //If app not playing then reset the camera to default position
                leiaCamera.transform.localPosition = new Vector3(0, 0, -1) * this.height;
                leiaCamera.ConvergenceDistance = this.height;
            }
        }

        //Update the virtual display model
        if (model != null)
        {
            sides[0].localPosition = new Vector3(0, height / 2f + .5f * thickness, 0);
            sides[1].localPosition = new Vector3(0, -height / 2f - .5f * thickness, 0);
            sides[2].localPosition = new Vector3(width / 2f + .5f * thickness, 0, 0);
            sides[3].localPosition = new Vector3(-width / 2f - .5f * thickness, 0, 0);

            sides[0].localScale = new Vector3(width, thickness, thickness);
            sides[1].localScale = new Vector3(width, thickness, thickness);
            sides[2].localScale = new Vector3(thickness, height, thickness);
            sides[3].localScale = new Vector3(thickness, height, thickness);

            corners[0].localPosition = new Vector3(width / 2f + .5f * thickness, height / 2f + .5f * thickness, 0);
            corners[1].localPosition = new Vector3(-width / 2f - .5f * thickness, height / 2f + .5f * thickness, 0);
            corners[2].localPosition = new Vector3(width / 2f + .5f * thickness, -height / 2f - .5f * thickness, 0);
            corners[3].localPosition = new Vector3(-width / 2f - .5f * thickness, -height / 2f - .5f * thickness, 0);

            corners[0].localScale = new Vector3(thickness, thickness, thickness);
            corners[1].localScale = new Vector3(thickness, thickness, thickness);
            corners[2].localScale = new Vector3(thickness, thickness, thickness);
            corners[3].localScale = new Vector3(thickness, thickness, thickness);

            corners[0].localRotation = Quaternion.Euler(0, 0, 270);
            corners[1].localRotation = Quaternion.Euler(0, 0, 0);
            corners[2].localRotation = Quaternion.Euler(0, 0, 180);
            corners[3].localRotation = Quaternion.Euler(0, 0, 90);

            logo.localPosition = new Vector3(0, -height / 2f - .5f * thickness, -thickness / 2f);
            logo.localScale = new Vector3(1f, 1f, 1f) * thickness / 2f;

            for (int i = 0; i < sides.Length; i++)
            {
                sides[i].GetComponent<MeshRenderer>().enabled = !Application.isPlaying || ShowAtRuntime;

#if UNITY_EDITOR
                if (Selection.Contains(sides[i].gameObject))
                {
                    Selection.objects = new GameObject[] { gameObject };
                }
#endif
            }
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i].GetComponent<MeshRenderer>().enabled = !Application.isPlaying || ShowAtRuntime;

#if UNITY_EDITOR
                if (Selection.Contains(corners[i].gameObject))
                {
                    Selection.objects = new GameObject[] { gameObject };
                }
#endif
            }

            if (Application.isPlaying)
            {
                MeshRenderer[] logoMeshRenderers = logo.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < logoMeshRenderers.Length; i++)
                {
                    logoMeshRenderers[i].enabled = ShowAtRuntime;
                }
            }
#if UNITY_EDITOR
            if (Selection.Contains(logo.gameObject))
            {
                Selection.objects = new GameObject[] { gameObject };
            }
#endif
        }
    }

    void LateUpdate()
    {
        UpdateDisplayGizmos();
    }
}
