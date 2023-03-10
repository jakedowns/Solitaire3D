#if UNITY_STANDALONE_WIN
using LeiaLoft;
using UnityEngine;

public class HeadTrackingService : MonoBehaviour
{
    public static HeadTrackingService _instance;
    public static HeadTrackingService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("HeadTrackingService").AddComponent<HeadTrackingService>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public LeiaHeadTracking.Engine headTrackingEngine;
    bool initialized;
    bool terminated;

    public LeiaHeadTracking.Engine Initialize()
    {
        if (!initialized)
        {
            initialized = true;

            try
            {
                DisplayConfig displayConfig = LeiaDisplay.Instance.GetDisplayConfig();

                LeiaHeadTracking.Engine.InitArgs initArgs = new LeiaHeadTracking.Engine.InitArgs();
                initArgs.enablePolling = 1;
                initArgs.cameraWidth = displayConfig.CameraStreamParams.width;
                initArgs.cameraHeight = displayConfig.CameraStreamParams.height;
                initArgs.cameraFps = displayConfig.CameraStreamParams.fps;
                initArgs.cameraBinningFactor = displayConfig.CameraStreamParams.binningFactor;
#if DEVELOPMENT_BUILD
                initArgs.logLevel = LeiaHeadTracking.Engine.LogLevel.Trace;
#endif
                if (Application.platform == RuntimePlatform.Android)
                {
                    initArgs.detectorMaxNumOfFaces = 1;
                }
                else
                {
                    initArgs.detectorMaxNumOfFaces = 3;
                }

                headTrackingEngine = null;

#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
            headTrackingEngine = new LeiaHeadTracking.Engine(ref initArgs, null);

            LeiaHeadTracking.Vector3 cameraPosition;
            cameraPosition.x = displayConfig.cameraCenterX;
            cameraPosition.y = displayConfig.cameraCenterY;
            cameraPosition.z = displayConfig.cameraCenterZ;
            LeiaHeadTracking.Vector3 cameraRotation;
            cameraRotation.x = displayConfig.cameraThetaX;
            cameraRotation.y = displayConfig.cameraThetaY;
            cameraRotation.z = displayConfig.cameraThetaZ;
            headTrackingEngine.SetCameraTransform(cameraPosition, cameraRotation);
            headTrackingEngine.StartTracking();
#endif
            }
            catch (LeiaHeadTracking.Engine.NativeCallFailedException e)
            {
                Debug.LogError("Failed to init head tracking: " + e.ToString());
                headTrackingEngine = null;
            }
        }

        return headTrackingEngine;
    }
    
    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
        TerminateHeadTracking();
#endif
    }
    

    public void TerminateHeadTracking()
    {
        if (terminated)
        {
            Debug.Log("TerminateHeadTracking called but it has already been terminated");
            return;
        }

        terminated = true;
#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
        if (headTrackingEngine != null)
        {
            headTrackingEngine.Dispose();
            headTrackingEngine = null;
        }
#endif
    }
}
#endif