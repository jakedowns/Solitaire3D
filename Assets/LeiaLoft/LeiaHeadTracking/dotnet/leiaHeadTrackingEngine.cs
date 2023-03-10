#if UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;

namespace LeiaHeadTracking
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector2d
    {
        public double x, y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector3
    {
        public float x, y, z;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct KalmanCoeffs
    {
        public Vector3 a;
        public Vector3 b;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FilterProperties
    {
        public float survivalTimeMs;
        public float velDampingTime;
        public float minFitDist;
        public float maxFitDist;
        public float angleYMax;
        public float interocularDistanceMin;
        public float interocularDistanceMax;
        public float bufferLim;
        public Vector3 mainFaceSkew;
        public Vector3 mainFaceScale;
        public KalmanCoeffs kalman;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DetectedFace
    {
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 angle;

        public float x
        {
            get
            {
                return pos.x;
            }
        }
        public float y
        {
            get
            {
                return pos.y;
            }
        }
        public float z
        {
            get
            {
                return pos.z;
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Timestamp
    {
        public enum Space
        {
            System = 0,
            Unknown,
        };
        Space space;

        public double ms;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Pose
    {
        public Vector3 position;
        public Vector3 angle;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct EyeScreenCoords
    {
        public Vector2d right;
        public Vector2d left;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct EyePoints
    {
        public Vector3 right;
        public Vector3 left;
    }

    public class Engine : IDisposable
    {
        public enum LogLevel
        {
            Default = 0,
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Critical,
            Off,
        }
        public class NativeCallFailedException : Exception
        {
            public Status Status;
            public NativeCallFailedException(Status status) : base()
            {
                Status = status;
            }
            public override string Message
            {
                get => Status.ToString();
            }
        }

        static Frame currentFrame;
        public static Frame GetCurrentFrame()
        {
            return currentFrame;
        }

        private static void OnFrameWrap(IntPtr frame, IntPtr handle)
        {
            currentFrame = new Frame(frame);
            (GCHandle.FromIntPtr(handle).Target as Engine)._onFrame(new Frame(frame));
        }
        public delegate void OnFrameDelegate(Frame frame);
        private OnFrameDelegate _onFrame;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct InitArgs {
            public Int32 cameraWidth;
            public Int32 cameraHeight;
            public Int32 cameraFps;
            public float cameraBinningFactor;

            public Int32 detectorMaxNumOfFaces;

            public LogLevel logLevel;

            public FilterProperties filterProperties;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void on_frame_callback(IntPtr frame, IntPtr handle);
            public on_frame_callback frameListener;
            public IntPtr frameListenerUserData;

            public Int32 enablePolling;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void virtual_face_hook(Timestamp timestamp, IntPtr implementMe, IntPtr handle);
            public virtual_face_hook virtualFaceHook;
            public IntPtr virtualFaceHookUserData;
        }
        public Engine(ref InitArgs initArgs, OnFrameDelegate onFrame)
        {
            _onFrame = onFrame;
            if (_onFrame != null)
            {
                _callbackHandle = GCHandle.Alloc(this);

                initArgs.frameListener = OnFrameWrap;
                initArgs.frameListenerUserData = GCHandle.ToIntPtr(_callbackHandle);
            }

            initArgs.virtualFaceHook = null;
            initArgs.virtualFaceHookUserData = IntPtr.Zero;

            HandleNativeCall(leiaHeadTrackingEngineInitArgs(initArgs, out _engine));
        }
        public Engine(string configPath)
        {
            HandleNativeCall(leiaHeadTrackingEngineInit(configPath, out _engine));
        }
        public void Dispose()
        {
            if (_engine != IntPtr.Zero)
            {
                leiaHeadTrackingEngineRelease(_engine);
                _engine = IntPtr.Zero;
            }

            if (_callbackHandle.IsAllocated)
            {
                _callbackHandle.Free();
            }
        }
        public void StartTracking()
        {
            HandleNativeCall(leiaHeadTrackingEngineStartTracking(_engine));
        }
        public void StopTracking()
        {
            HandleNativeCall(leiaHeadTrackingEngineStopTracking(_engine));
        }
        public void SetFilterProperties(in FilterProperties filterProperties)
        {
            HandleNativeCall(leiaHeadTrackingEngineSetFilterProperties(_engine, in filterProperties));
        }
        public void SetTrackedEyes(bool leftEye, bool rightEye)
        {
            HandleNativeCall(leiaHeadTrackingEngineSetTrackedEyes(_engine, leftEye ? 1 : 0, rightEye ? 1 : 0));
        }
        public void SetCameraTransform(Vector3 position, Vector3 rotation)
        {
            HandleNativeCall(leiaHeadTrackingEngineSetCameraTransform(_engine, position, rotation));
        }
        public void SetCameraPosition(float x, float y, float z)
        {
            HandleNativeCall(leiaHeadTrackingEngineSetCameraPosition(_engine, x, y, z));
        }
        public void SetCameraRotation(
            float r00, float r01, float r02,
            float r10, float r11, float r12,
            float r20, float r21, float r22)
        {
            HandleNativeCall(leiaHeadTrackingEngineSetCameraRotation(_engine,
                r00, r01, r02,
                r10, r11, r12,
                r20, r21, r22));
        }
        public void GetMaxNumOfDetectedFacesLimit(out Int32 maxNumOfDetectedFacesLimit)
        {
            HandleNativeCall(leiaHeadTrackingEngineGetMaxNumOfDetectedFacesLimit(_engine, out maxNumOfDetectedFacesLimit));
        }
        public void SetMaxNumOfDetectedFaces(Int32 maxNumOfDetectedFaces)
        {
            HandleNativeCall(leiaHeadTrackingEngineSetMaxNumOfDetectedFaces(_engine, maxNumOfDetectedFaces));
        }
        public const int MAX_NUM_FACES = 3;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Result
        {
            public Int32 numDetectedFaces;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = MAX_NUM_FACES)]
            public DetectedFace[] detectedFaces;

            public Timestamp timestamp;

            public Int32 jumpFlag;
        }
        public void GetTrackingResult(out Result trackingResult)
        {
            HandleNativeCall(leiaHeadTrackingEngineGetTrackingResult(_engine, out trackingResult));
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Measurements
        {
            public Int32 numDetectedFaces;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = MAX_NUM_FACES)]
            public EyeScreenCoords[] detectedEyeScreenCoords;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = MAX_NUM_FACES)]
            public Pose[] detectedPoses;

            public Int32 numRawFaces;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = MAX_NUM_FACES)]
            public EyePoints[] rawEyePoints;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = MAX_NUM_FACES)]
            public Vector3[] rawTrackingPoints;
        }

        public enum Status
        {
            Success = 0,
            ErrorInvalidInstance,
            ErrorUnknown,
        };
        private static void HandleNativeCall(int nativeStatus)
        {
            var status = (Status)nativeStatus;
            if (status != Status.Success)
            {
                throw new NativeCallFailedException(status);
            }
        }

        private GCHandle _callbackHandle;
        private IntPtr _engine;

        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineInit(string config_path, out IntPtr engine);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineInitArgs(in InitArgs init_args, out IntPtr engine);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineStartTracking(IntPtr engine);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineStopTracking(IntPtr engine);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineSetFilterProperties(IntPtr engine, in FilterProperties filterProperties);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineSetTrackedEyes(IntPtr engine, Int32 left_eye, Int32 right_eye);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineSetCameraTransform(IntPtr engine, Vector3 position, Vector3 rotation);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineSetCameraPosition(IntPtr engine, float x, float y, float z);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineSetCameraRotation(IntPtr engine,
            float r00, float r01, float r02,
            float r10, float r11, float r12,
            float r20, float r21, float r22);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineGetMaxNumOfDetectedFacesLimit(IntPtr engine, out Int32 maxNumOfDetectedFacesLimit);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineSetMaxNumOfDetectedFaces(IntPtr engine, Int32 maxNumOfDetectedFaces);
        [DllImport("leiaHeadTracking")]
        private static extern Int32 leiaHeadTrackingEngineGetTrackingResult(IntPtr engine, out Result result);
        [DllImport("leiaHeadTracking")]
        private static extern void leiaHeadTrackingEngineRelease(IntPtr engine);

        public class Frame
        {
            public Frame(IntPtr frame)
            {
                _frame = frame;
            }
            public void GetTrackingResult(out Result result)
            {
                HandleNativeCall(leiaHeadTrackingEngineFrameGetTrackingResult(_frame, out result));
            }
            public void GetMeasurements(out Measurements measurements)
            {
                HandleNativeCall(leiaHeadTrackingEngineFrameGetMeasurements(_frame, out measurements));
            }
            public void GetDetectionTimeNs(out long detectionTimeNs) //Blink process time (divide by a million to get MS)
            {
                HandleNativeCall(leiaHeadTrackingEngineFrameGetDetectionTimeNs(_frame, out detectionTimeNs));
            }

            private IntPtr _frame;

            [DllImport("leiaHeadTracking")]
            private static extern Int32 leiaHeadTrackingEngineFrameGetTrackingResult(IntPtr frame, out Result result);
            [DllImport("leiaHeadTracking")]
            private static extern Int32 leiaHeadTrackingEngineFrameGetMeasurements(IntPtr frame, out Measurements measurements);
            [DllImport("leiaHeadTracking")]
            private static extern Int32 leiaHeadTrackingEngineFrameGetDetectionTimeNs(IntPtr frame, out long detectionTimeNs);
        }
    }
}
#endif