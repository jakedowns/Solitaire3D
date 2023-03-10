
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using LeiaLoft;
using UnityEngine.UI;

public class BlinkTrackingUnityPlugin : Singleton<BlinkTrackingUnityPlugin>
{
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
    public LeiaHeadTracking.Engine.Result trackingResult = new LeiaHeadTracking.Engine.Result();
    private LeiaHeadTracking.Engine headTrackingEngine = null;
#endif
/// </remove_from_public>

    [SerializeField] private LeiaCamera leiaCamera;
    private float predictedFaceX = 0, predictedFaceY = 0, predictedFaceZ = 600;

    public Vector3 GetPredictedViewerPosition()
    {
        return new Vector3(predictedFaceX, predictedFaceY, predictedFaceZ);
    }

    private float nonPredictedFaceX = 0, nonPredictedFaceY = 0, nonPredictedFaceZ = 600;

    public Vector3 GetNonPredictedViewerPosition()
    {
        return new Vector3(nonPredictedFaceX, nonPredictedFaceY, nonPredictedFaceZ);
    }

    private float faceX = 0, faceY = 0;
    private float _faceZ = 600;
    public float faceZ
    {
        get
        {
            return _faceZ;
        }
        set
        {
            _faceZ = value;
        }
    }

    public Vector3 GetFacePosition()
    {
        return new Vector3(faceX, faceY, faceZ);
    }

    private float blinkProcessingTime = 0.0f;
    private float frameDelay; //Time between exposure and now (render)

    public float GetFrameDelay()
    {
        return frameDelay;
    }
    public float GetBlinkProcessingTime()
    {
        return blinkProcessingTime;
    }

    private bool isTracking = false;

    private float eyeDistanceThreshold = 200f;

    private double old_time_stamp = 0.0;

    public Leia.FaceDetectorBackend faceTrackingBackend;

    LeiaDisplay _leiaDisplay;
    LeiaDisplay leiaDisplay
    {
        get
        {
            if (_leiaDisplay == null)
            {
                _leiaDisplay = FindObjectOfType<LeiaDisplay>();
            }

            return _leiaDisplay;
        }
    }

    private bool _cameraConnectedPrev;
    private bool _cameraConnected;
    public bool CameraConnected
    {
        get
        {
            return _cameraConnected;
        }
    }

    [SerializeField] private Text debugLabel;

    LeiaVirtualDisplay _leiaVirtualDisplay;
    LeiaVirtualDisplay leiaVirtualDisplay
    {
        get
        {
            if (_leiaVirtualDisplay == null)
            {
                _leiaVirtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();
            }
            return _leiaVirtualDisplay;
        }
    }

/// <remove_from_public>
#if UNITY_STANDALONE_WIN
    FaceChooser faceChooser;
#endif
/// </remove_from_public>
    bool spawnTestDummyViewerHead = false;

    RunningFloatAverage runningAverageFaceX;
    RunningFloatAverage runningAverageFaceY;
    RunningFloatAverage runningAverageFaceZ;

#if UNITY_ANDROID
    private bool isTrackingStartedAndroid = false;
#endif


    void Start()
    {
#if UNITY_ANDROID
        if (Instance != null && Instance == this)
        {
            DontDestroyOnLoad(gameObject);
            LeiaDisplay.Instance.tracker = this;
        }

        Application.targetFrameRate = 60;
#endif
        runningAverageFaceX = new RunningFloatAverage(60);
        runningAverageFaceY = new RunningFloatAverage(60);
        runningAverageFaceZ = new RunningFloatAverage(60);

        GameObject debugLabelGameObject = GameObject.Find("BlinkDebugText");
        if (debugLabelGameObject != null)
        {
            debugLabel = debugLabelGameObject.GetComponent<Text>();
        }
        if (spawnTestDummyViewerHead)
        {
            GameObject head = Instantiate(
                Resources.Load("Prefabs/TestDummyViewerHead", typeof(GameObject))) as GameObject;
        }
        leiaCamera = FindObjectOfType<LeiaCamera>();

/// <remove_from_public>
#if UNITY_STANDALONE_WIN
        faceChooser = new FaceChooser();
#endif
/// </remove_from_public>

        UpdateCameraConnectedStatus();

        lastAvgFaces = new Leia.Vector3[maxAvgFaces];
    }

    void UpdateCameraConnectedStatus()
    {
        _cameraConnectedPrev = _cameraConnected;
        _cameraConnected = false;
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
#if UNITY_ANDROID
            // TODO: do we need to support realsense on Android?
            if (devices[i].name.Contains("Camera 1"))
#else
            if (devices[i].name.Contains("Intel(R) RealSense(TM)"))
#endif
            {
                _cameraConnected = true;
                break;
            }
        }

#if !UNITY_EDITOR
        if (!_cameraConnected && _cameraConnectedPrev)
        {
            Debug.Log("Camera not connected! Terminating head tracking!");
            TerminateHeadTracking();
        }
        else
        if (_cameraConnected && !_cameraConnectedPrev)
        {
            InitHeadTracking();
        }

#endif

        Invoke("UpdateCameraConnectedStatus", 1f);
    }

    bool ApplicationQuitting;

    private void OnApplicationQuit()
    {
        ApplicationQuitting = true;
#if UNITY_ANDROID
        TerminateHeadTracking();
#endif
    }

    private void OnEnable()
    {
        StartTracking();
    }

    private void OnDisable()
    {
        if (!ApplicationQuitting)
        {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
/*
            if (!ApplicationQuitting)
            {
                if (headTrackingEngine != null)
                {
                    this.headTrackingEngine.StopTracking();
                }
            }
*/
#endif
/// </remove_from_public>
#if UNITY_ANDROID
            if (Instance == this)
            {
                StopTracking();
            }

            faceX = 0;
            faceY = 0;
            faceZ = LeiaDisplay.Instance.GetDisplayConfig().ConvergenceDistance;
#endif
        }
    }
    private void OnApplicationPause(bool pause)
    {
#if UNITY_ANDROID
        if (pause)
        {
            StopTracking();
        }
        else
        {
            if (LeiaDisplay.Instance.DesiredLightfieldMode == LeiaDisplay.LightfieldMode.On)
                StartTracking();
        }
#endif
    }
    void TerminateHeadTracking()
    {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
        HeadTrackingService.Instance.TerminateHeadTracking();
#endif
/// </remove_from_public>

#if UNITY_ANDROID && !UNITY_EDITOR
        if (LeiaDisplay.Instance.CNSDK != null)
        {
            LeiaDisplay.Instance.CNSDK.EnableFacetracking(false);
            isTracking = false;
            isPrimaryFaceSet = false;
        }
#endif
    }

    void InitHeadTracking()
    {
        /// <remove_from_public>
#if UNITY_STANDALONE_WIN
        headTrackingEngine = HeadTrackingService.Instance.Initialize();
#endif
        /// </remove_from_public>

#if UNITY_ANDROID && !UNITY_EDITOR
        Leia.FaceDetectorConfig config = new Leia.FaceDetectorConfig();
        config.backend = faceTrackingBackend;
        config.inputType = Leia.FaceDetectorInputType.Unknown;
        LeiaDisplay.Instance.CNSDK.SetFaceTrackingConfig(config);
        isTracking = LeiaDisplay.Instance.CNSDK.EnableFacetracking(true) == Leia.SDK.Status.Success;
#endif
    }

    public void StopTracking()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (LeiaDisplay.Instance.CNSDK != null)
        {
            LeiaDisplay.Instance.CNSDK.Pause();
            isTracking = false;
            isPrimaryFaceSet = false;
        }
#endif
    }

    public void SetProfilingEnabled(bool enabed)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        LeiaDisplay.Instance.CNSDK.SetProfiling(enabed);
        _isProfilingEnabled = enabed;
#endif
    }

    public void StartTracking()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (LeiaDisplay.Instance.CNSDK != null)
        {
            if(!isTracking)
            {
                LeiaDisplay.Instance.CNSDK.Resume();
                Debug.Log("Called LeiaDisplay.Instance.CNSDK.Resume();");

                isTracking = true;
            }
        }
        else
        {
#if !UNITY_EDITOR
            Debug.LogError("LeiaDisplay.Instance.CNSDK is null!");
#endif
        }
#endif
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
        faceZ = LeiaDisplay.Instance.displayConfig.ConvergenceDistance;

        if (headTrackingEngine != null)
        {
            this.headTrackingEngine.StartTracking();
        }
#endif
/// </remove_from_public>
    }

    enum TrackingState { FaceTracking, NotFaceTracking };
    TrackingState priorRequestedState = TrackingState.NotFaceTracking;
    TrackingState currentState = TrackingState.NotFaceTracking;
    TrackingState requestedState = TrackingState.NotFaceTracking;
    int numInRow = 0;

    void AddTestFace(float x = 0, float y = 0, float z = 800) //A useful method for adding a virtual test face, which can be used for multi-face testing when you don't have other people available to help you test
    {
        //Currently only supported on Windows
        //TODO: Implement for Android
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
        if (trackingResult.numDetectedFaces < LeiaHeadTracking.Engine.MAX_NUM_FACES)
        {
            LeiaHeadTracking.DetectedFace face = new LeiaHeadTracking.DetectedFace();
            face.pos.x = 0;
            face.pos.y = 0;
            face.pos.z = 800;
            face.vel.x = 0;
            face.vel.y = 0;
            face.vel.z = 0;
            trackingResult.detectedFaces[trackingResult.numDetectedFaces] = face;
            trackingResult.numDetectedFaces++;
        }
#endif
/// </remove_from_public>
    }

    AndroidJavaClass javaClockClass = null;
    double GetSystemTimeMs()
    {
        if (javaClockClass == null)
        {
            javaClockClass = new AndroidJavaClass("android.os.SystemClock");
        }
        long timeNs = javaClockClass.CallStatic<long>("elapsedRealtimeNanos");
        return (double)(timeNs) * 1e-6;
    }

    public enum FaceTransitionState { NoFace, FaceLocked, ReducingBaseline, SlidingCameras, IncreasingBaseline };

    FaceTransitionState _faceTransitionState = FaceTransitionState.NoFace;

    public FaceTransitionState faceTransitionState
    {
        get
        {
            return _faceTransitionState;
        }
        set
        {
            _faceTransitionState = value;
        }
    }

    int chosenFaceIndex;
    int chosenFaceIndexPrev;

    float largestFaceChangeDistance;

    int currAvgFaceIdx = 0;
    const int maxAvgFaces = 7;
    Leia.Vector3[] lastAvgFaces;
    public bool isPrimaryFaceSet = false;
    public Leia.Vector3 primaryFacePosition;

    public bool nonPredFaceFound = false;
    public Leia.Vector3 nonPredictedFace;

    private bool _isProfilingEnabled = true;

#if UNITY_ANDROID
    long GetSystemTimeNs()
    {
        if (javaClockClass == null)
        {
            javaClockClass = new AndroidJavaClass("android.os.SystemClock");
        }
        long timeNs = javaClockClass.CallStatic<long>("elapsedRealtimeNanos");
        return timeNs;
    }
#endif

    public void UpdateFacePosition()
    {
        isPrimaryFaceSet = false;
        if (LeiaDisplay.Instance.UsingSimulatedFacePosition)
        {
            faceX = LeiaDisplay.Instance.SimulatedFaceX;
            faceY = LeiaDisplay.Instance.SimulatedFaceY;
            faceZ = LeiaDisplay.Instance.SimulatedFaceZ;
            return;
        }

        if (_cameraConnected && (isTracking || Application.platform != RuntimePlatform.Android))
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (LeiaDisplay.Instance.CNSDK != null)
            {
                isPrimaryFaceSet = LeiaDisplay.Instance.CNSDK.GetPrimaryFace(out primaryFacePosition) == Leia.SDK.Status.Success;
                nonPredFaceFound = LeiaDisplay.Instance.CNSDK.GetNonPredictedPrimaryFace(out nonPredictedFace) == Leia.SDK.Status.Success;

                if (_isProfilingEnabled)
                {
                    LeiaHeadTracking.FrameProfiling frameProfiling;

                    LeiaDisplay.Instance.CNSDK.GetFaceTrackingProfiling(out frameProfiling);
                    blinkProcessingTime = (float)(frameProfiling.faceDetectorEndTime - frameProfiling.faceDetectorStartTime) * 1e-6f;

                    long now = GetSystemTimeNs();
                    frameDelay = (float)(now - frameProfiling.cameraExposureTime) * 1e-6f;
                }

            }
#endif
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
            // TODO: use timestamp to check if we received a new tracking result or the old one
            trackingResult.numDetectedFaces = 0;
            trackingResult.timestamp.ms = -1.0;

            if (headTrackingEngine != null)
            {
                headTrackingEngine.GetTrackingResult(out trackingResult);
            }
#endif
/// </remove_from_public>
#if UNITY_ANDROID
            if (isPrimaryFaceSet)
#endif

/// <remove_from_public>
#if UNITY_STANDALONE_WIN
            if (trackingResult.numDetectedFaces > 0)
#endif
/// </remove_from_public>
            {
                requestedState = TrackingState.FaceTracking;

                DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
                float displayHeightMM = config.PanelResolution.y * config.PixelPitchInMM.y;
                // TODO: delay should be calculated like that:
                //         delay = frame_on_display_presentation_time - trackingResult.timestamp
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                double t = trackingResult.timestamp.ms;
                if (trackingResult.timestamp.ms < 0.0)
                {
                    t = Time.time * 1000;
                }
                frameDelay = (float)(t - old_time_stamp);
                float delay = (float)(t - old_time_stamp + config.timeDelay);
#endif
/// </remove_from_public>
                chosenFaceIndexPrev = chosenFaceIndex;
                chosenFaceIndex = -1;
                if (currentState == TrackingState.FaceTracking)
                {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                    chosenFaceIndex = faceChooser.ChosenFaceIndex(trackingResult);
                    LeiaHeadTracking.DetectedFace chosenFace = trackingResult.detectedFaces[chosenFaceIndex];
#else
/// </remove_from_public>
                    chosenFaceIndex = 0;
/// <remove_from_public>
#endif

#if UNITY_STANDALONE_WIN
                    if (chosenFaceIndexPrev != chosenFaceIndex)
                    {
#elif UNITY_ANDROID
/// </remove_from_public>
                    if (chosenFaceIndexPrev != chosenFaceIndex && isTrackingStartedAndroid)
                    {
/// <remove_from_public>
#endif
/// </remove_from_public>
                        faceTransitionState = FaceTransitionState.ReducingBaseline;
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                        if (runningAverageFaceZ.Average > 0)
                        {
                            faceX = runningAverageFaceX.Average;
                            faceY = runningAverageFaceY.Average;
                            faceZ = runningAverageFaceZ.Average;
                            predictedFaceX = faceX;
                            predictedFaceY = faceY;
                            predictedFaceZ = faceZ;
                        }
#endif
/// </remove_from_public>
                    }
#if UNITY_ANDROID
                    else if (chosenFaceIndexPrev != chosenFaceIndex && !isTrackingStartedAndroid)
                    {
                        isTrackingStartedAndroid = true;
                        faceTransitionState = FaceTransitionState.SlidingCameras;
                    }
#endif
                    if (faceTransitionState == FaceTransitionState.SlidingCameras
                        || faceTransitionState == FaceTransitionState.IncreasingBaseline)
                    {
                        Vector3 currentPos = new Vector3(
                            faceX,
                            faceY,
                            faceZ
                        );
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                        Vector3 targetPos = new Vector3(
                            chosenFace.pos.x,
                            chosenFace.pos.y,
                            chosenFace.pos.z
                        );
                        if (chosenFace.pos.z > 0)
                        {
#endif
/// </remove_from_public>
#if UNITY_ANDROID
                        Vector3 targetPos = new Vector3(
                            primaryFacePosition.x,
                            primaryFacePosition.y,
                            primaryFacePosition.z
                        );
                        if (primaryFacePosition.z > 0)
                        {
#endif
                            faceX += (targetPos.x - currentPos.x) * Mathf.Min((Time.deltaTime * 5f), 1f);
                            faceY += (targetPos.y - currentPos.y) * Mathf.Min((Time.deltaTime * 5f), 1f);
                            faceZ += (targetPos.z - currentPos.z) * Mathf.Min((Time.deltaTime * 5f), 1f);
                            predictedFaceX = faceX;
                            predictedFaceY = faceY;
                            predictedFaceZ = faceZ;
                        }
                        if (faceTransitionState == FaceTransitionState.SlidingCameras
                            && Vector3.Distance(currentPos, targetPos) < 10f)
                        {
                            faceTransitionState = FaceTransitionState.IncreasingBaseline;
                        }
                    }

                    if (faceTransitionState == FaceTransitionState.FaceLocked)
                    {
#if UNITY_ANDROID
                        Leia.Vector3 avgFaces;
                        avgFaces.x = avgFaces.y = avgFaces.z = 0.0f;
                        predictedFaceX = primaryFacePosition.x;
                        predictedFaceY = primaryFacePosition.y;
                        predictedFaceZ = primaryFacePosition.z;
                        lastAvgFaces[currAvgFaceIdx].x = predictedFaceX;
                        lastAvgFaces[currAvgFaceIdx].y = predictedFaceY;
                        lastAvgFaces[currAvgFaceIdx].z = predictedFaceZ;

                        currAvgFaceIdx++;
                        if (currAvgFaceIdx >= maxAvgFaces)
                            currAvgFaceIdx = 0;
                        for (int i = 0; i < maxAvgFaces; ++i)
                        {
                            avgFaces.x += lastAvgFaces[i].x;
                            avgFaces.y += lastAvgFaces[i].y;
                            avgFaces.z += lastAvgFaces[i].z;
                        }
                        avgFaces.x /= maxAvgFaces;
                        avgFaces.y /= maxAvgFaces;
                        avgFaces.z /= maxAvgFaces;
                        faceX = avgFaces.x;
                        faceY = avgFaces.y;
                        faceZ = avgFaces.z;

#endif
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                        Vector3 facePosPrev = new Vector3(
                            faceX,
                            faceY,
                            faceZ
                        );
                        Vector3 facePosNext = new Vector3(
                            chosenFace.pos.x,
                            chosenFace.pos.y,
                            chosenFace.pos.z
                        );

                        float distance = Mathf.Abs(facePosPrev.z - facePosNext.z);

                        if (distance > largestFaceChangeDistance)
                        {
                            largestFaceChangeDistance = Vector3.Distance(facePosPrev, facePosNext);
                        }

                        if ((distance < 50 || facePosPrev == Vector3.zero) && chosenFace.pos.z > 0)
                        {
                            faceX = chosenFace.pos.x;
                            faceY = chosenFace.pos.y;
                            faceZ = chosenFace.pos.z;
                            predictedFaceX = chosenFace.pos.x + chosenFace.vel.x * delay;
                            predictedFaceY = chosenFace.pos.y + chosenFace.vel.y * delay;
                            predictedFaceZ = chosenFace.pos.z + chosenFace.vel.z * delay;
                            runningAverageFaceX.AddSample(faceX);
                            runningAverageFaceY.AddSample(faceY);
                            runningAverageFaceZ.AddSample(faceZ);
                        }
                        else
                        {
                            if (runningAverageFaceZ.Average > 0)
                            {
                                faceX = runningAverageFaceX.Average;
                                faceY = runningAverageFaceY.Average;
                                faceZ = runningAverageFaceZ.Average;
                            }
                            faceTransitionState = FaceTransitionState.ReducingBaseline;
                        }
#endif
/// </remove_from_public>
                    }
                }
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                // Store old timestamp
                old_time_stamp = t;
#endif
/// </remove_from_public>
            }
            else
            {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
                faceTransitionState = FaceTransitionState.ReducingBaseline;
#endif
/// </remove_from_public>
#if UNITY_ANDROID
                if (isTrackingStartedAndroid)
                {
                    faceTransitionState = FaceTransitionState.ReducingBaseline;
                }
#endif
                requestedState = TrackingState.NotFaceTracking;
            }

#if UNITY_ANDROID
            if (nonPredFaceFound)
            {
                nonPredictedFaceX = nonPredictedFace.x;
                nonPredictedFaceY = nonPredictedFace.y;
                nonPredictedFaceZ = nonPredictedFace.z;
            }
#endif
            if (currentState != requestedState)
            {
                if (requestedState == priorRequestedState)
                {
                    numInRow++;
                    if (numInRow > 20)
                    {
                        currentState = requestedState;
                        numInRow = 0;
                    }
                }
            }
            priorRequestedState = requestedState;
        }
    }

    float lowestFPSInterval = 10.0f;
    int lowestFPS = 120;
    void Update()
    {
        if (leiaCamera == null)
        {
            leiaCamera = FindObjectOfType<LeiaCamera>();
        }

        if (LeiaDisplay.Instance.AutoRenderTechnique)
        {
            if (CameraConnected)
            {
                if (LeiaDisplay.Instance.DesiredRenderTechnique != LeiaDisplay.RenderTechnique.Stereo)
                {
                    LeiaDisplay.Instance.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Stereo;
                }
            }
            else
            {
                if (LeiaDisplay.Instance.DesiredRenderTechnique != LeiaDisplay.RenderTechnique.Multiview)
                {
                    LeiaDisplay.Instance.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Multiview;
                }
            }
        }

        //UpdateFacePosition();

        //debugLabel.text = "faceTransitionState = " + faceTransitionState.ToString();

        int fps = (int)(1.0f / Time.deltaTime);

        lowestFPSInterval -= Time.deltaTime;


        if (lowestFPSInterval < 0.0f)
        {
            lowestFPSInterval = 10;
            lowestFPS = fps;
        }


        if (fps < lowestFPS)
        {
            lowestFPS = fps;
        }

        if (faceTransitionState == FaceTransitionState.FaceLocked)
        {
            LeiaDisplay.Instance.blackViewsTemp = true;
        }
        else
        {
            LeiaDisplay.Instance.blackViewsTemp = false;
        }
        if (debugLabel != null && debugLabel.isActiveAndEnabled)
        {
            debugLabel.text =
                "faceX: " + faceX + "\n" +
                "faceY: " + faceY + "\n" +
                "faceZ: " + faceZ + "\n" +
                "leiaCamera.CameraShift: " + leiaCamera.CameraShift + "\n" +
                "leiaVirtualDisplay.height: " + this.leiaVirtualDisplay.Height + "\n" +
                "chosenFaceIndex = " + chosenFaceIndex + "\n"
                + "NumFaces = " + NumFaces + "\n"
                + "leiaCamera.BaselineScaling = " + leiaCamera.BaselineScaling + "\n"
                + "leiaCamera.FinalBaselineScaling: " + leiaCamera.FinalBaselineScaling + "\n"
                + "leiaCamera.ConvergenceDistance = " + leiaCamera.ConvergenceDistance + "\n"
                + "leiaCamera.transform.localPosition = " + leiaCamera.transform.localPosition + "\n"
                + "leiaCamera.leiaCamera.transform.position = " + leiaCamera.transform.position + "\n"
                + "getPeelOffsetForShader = " + LeiaDisplay.Instance.getPeelOffsetForShader() + "\n"
                + "getPeelOffsetForCameraShift = " + LeiaDisplay.Instance.getPeelOffsetForCameraShift(leiaCamera) + "\n"
                + "fps: " + 1.0f / Time.deltaTime + "\n"
                + "faceTransitionState = " + faceTransitionState.ToString() + "\n"
                + "priorRequestedState = " + priorRequestedState.ToString() + "\n"
                + "currentState = " + currentState.ToString() + "\n"
                + "requestedState = " + requestedState.ToString() + "\n"
                + "minView: " + LeiaDisplay.Instance.minView + "\n"
                + "maxView: " + LeiaDisplay.Instance.maxView + "\n"
                + "range: " + LeiaDisplay.Instance.range + "\n"
                + "LeiaDisplay.Instance.CameraShiftScale: " + leiaCamera.CameraShiftScaling + "\n"
                + "displayConfig.ConvergenceDistance: " + LeiaDisplay.Instance.displayConfig.ConvergenceDistance + "\n"
                + "lowest fps: " + lowestFPS + "\n";
        }
        if (fps < 59)
        {
            debugLabel.text += "!!! FRAME DROPPED !!!\n";
        }
    }

    public int NumFaces
    {
        get
        {
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
            return trackingResult.numDetectedFaces;
#else
/// </remove_from_public>
            return isPrimaryFaceSet ? 1 : 0;
/// <remove_from_public>
#endif
/// </remove_from_public>
        }
    }
}
