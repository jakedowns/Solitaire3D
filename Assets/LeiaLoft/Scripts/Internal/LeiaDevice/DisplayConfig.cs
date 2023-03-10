using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Linq;
using System.Reflection;

using PermissionLevel = LeiaLoft.DisplayConfigModifyPermission.Level;

namespace LeiaLoft
{
    /// <summary>
    /// Attribute on DisplayConfig properties. Defines whether the property can be set
    /// when SetPropertyByReflection is passed with a certain permission level.
    /// </summary>
    public class DisplayConfigModifyPermission : Attribute
    {
        /// <summary>
        /// PermissionAttribute has-a collection of levels.
        ///
        /// I.e. this attribute applies to a DisplayConfig field, and defines
        /// whether the field can be modified with a certain permission level.
        /// </summary>
        private HashSet<Level> levels = new HashSet<PermissionLevel>();

        public DisplayConfigModifyPermission(params Level[] _levels)
        {
            levels = new HashSet<PermissionLevel>(_levels);
        }
        public bool Contains(Level providedRank)
        {
            return levels.Contains(providedRank);
        }
        public enum Level
        {
            DeviceSimulation, DeveloperTuned
        };
    }

    public class DisplayConfig
    {
        /// <summary>
        /// Scales View and Panel Resolution. <b>Only use with Square Leia State and if you fully know what you are doing with it.</b> 
        /// </summary>
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float VersionNum { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float timeDelay { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float ResolutionScale { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        private XyPair<int> _panelResolution;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        private XyPair<float> _viewboxSize;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        private XyPair<int> _viewResolution;
        private bool _userOrientationIsLandscape;

        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float n = 1.47f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float theta = 0.0f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float cameraCenterX = -37.88f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float cameraCenterY = 165.57f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float cameraCenterZ = 0.0f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public int colorSlant = 0;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public bool colorInversion = false;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float p_over_du = 3.0f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float p_over_dv = 1.0f; //TODO: grab this from isSlanted flag
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float s = 11.0f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float d_over_n = 0.5f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float cameraThetaX = 0.0f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float cameraThetaY = 0.0f;
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float cameraThetaZ = 0.0f;

        /// <summary>
        /// Method for getting the orientation-appropriate interlacing matrix on all devices.
        /// </summary>
        /// <param name="orientation">The latest screen orientation that was sampled by ILeiaDevice.GetScreenOrientationRGB</param>
        /// <returns>Interlacing matrix that is appropriate for the device's orientation</returns>
        public float[] getInterlacingMatrixForOrientation(ScreenOrientation orientation)
        {
            switch (orientation)
            {
                // if device fails to get gyroscope info, we might end up in "default" case
                default:
                case ScreenOrientation.LandscapeLeft:
                    return InterlacingMatrixLandscape;
                case ScreenOrientation.LandscapeRight:
                    return InterlacingMatrixLandscape180;
                case ScreenOrientation.Portrait:
                    return InterlacingMatrixPortrait;
                case ScreenOrientation.PortraitUpsideDown:
                    return InterlacingMatrixPortrait180;
            }
        }

        /// <summary>
        /// The "InterlacingMatrix" property is now a backward-compatible structure which is superseded by more specific properties.
        ///
        /// We must now distinguish matrix based upon
        /// device orientation = {landscape / portrait}, and
        /// whether device is held such that subpixel views converge for {RGB / BGR}.
        /// </summary>
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingMatrix
        {
            get
            {
                return this.InterlacingMatrixLandscape;
            }
            set
            {
                this.InterlacingMatrixLandscape = value;
            }
        }

        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingMatrixPortrait { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingMatrixPortrait180 { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingMatrixLandscape { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingMatrixLandscape180 { get; set; }

        /// <summary>
        /// Method for getting the orientation-appropriate interlacing vector on all devices.
        /// </summary>
        /// <param name="orientation">The latest screen orientation that was sampled by ILeiaDevice.GetScreenOrientationRGB</param>
        /// <returns>Interlacing vector that is appropriate for the device's orientation</returns>
        public float[] getInterlacingVectorForOrientation(ScreenOrientation orientation)
        {
            switch (orientation)
            {
                // if device fails to get gyroscope info, we might end up in "default" case
                default:
                case ScreenOrientation.LandscapeLeft:
                    return InterlacingVectorLandscape;
                case ScreenOrientation.LandscapeRight:
                    return InterlacingVectorLandscape180;
                case ScreenOrientation.Portrait:
                    return InterlacingVectorPortrait;
                case ScreenOrientation.PortraitUpsideDown:
                    return InterlacingVectorPortrait180;
            }
        }

        /// <summary>
        /// The "InterlacingVector" property is now a backward-compatible structure which is superseded by more specific properties.
        ///
        /// We must now distinguish vector based upon
        /// device orientation = {landscape / portrait}, and
        /// whether device is held such that subpixel views converge for {RGB / BGR}.
        /// </summary>
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingVector
        {
            get
            {
                return this.InterlacingVectorLandscape;
            }
            set
            {
                this.InterlacingVectorLandscape = value;
            }
        }

        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingVectorLandscape { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingVectorLandscape180 { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingVectorPortrait { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public float[] InterlacingVectorPortrait180 { get; set; }

        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float Gamma { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float ConvergenceDistance
        {
            get
            {
                return this.d_over_n * this.p_over_du * this.PanelResolution.x / this.s; //In MM
            }
        }

        public float ViewsPerInterocularAtConvergence()
        {
            float interocular = 63; //In MM
            return interocular * LeiaDisplay.Instance.p_over_du * LeiaDisplay.Instance.d_over_n / LeiaDisplay.Instance.displayConfig.ConvergenceDistance / LeiaDisplay.Instance.displayConfig.PixelPitchInMM.x;
        }

        public float ViewsPerInterocularAtFaceZ(float faceZ)
        {
            float interocular = 63; //In MM
            return interocular * LeiaDisplay.Instance.p_over_du * LeiaDisplay.Instance.d_over_n / faceZ / LeiaDisplay.Instance.displayConfig.PixelPitchInMM.x;
        }

        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float CenterViewNumber { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public bool Slant { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float Beta { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public bool isSquare { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public bool isSlanted { get; set; }
        public static readonly string DefaultRenderMode = "HPO";
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public XyPair<float> _pixelPitchInMm { get; set; }
        public XyPair<float> PixelPitchInMM
        {
            get
            {
                return _pixelPitchInMm;
            }
            set
            {
                if (value.x <= 0 || value.y <= 0)
                {
                    Debug.LogError("_PixelPitchInMM cannot be set to " + value + ", values must be greater than 0.");
                    return;
                }
                _pixelPitchInMm = value;
            }
        }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]

        public XyPair<int> PanelResolution
        {
            get
            {
                return _panelResolution;
            }
            set
            {
                _panelResolution = value;
            }
        }

        public XyPair<float> ViewboxSize
        {
            get
            {
                return _viewboxSize;
            }
            set
            {
                _viewboxSize = value;
            }
        }

        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public XyPair<int> NumViews { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public XyPair<float> AlignmentOffset { get; set; }
        // Set using ActCoefficientsX or ActCoefficientsY
        public XyPair<List<float>> ActCoefficients { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float[] ActCoefficientsX
        {
            get
            {
                if (ActCoefficients == null || ActCoefficients.x == null)
                {
                    return null;
                }
                return ActCoefficients.x.ToArray();
            }
            set
            {
                if (ActCoefficients == null)
                {
                    ActCoefficients = new XyPair<List<float>>(new List<float>(), new List<float>());
                }
                ActCoefficients.x = new List<float>(value);
            }
        }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float[] ActCoefficientsY
        {
            get
            {
                if (ActCoefficients == null || ActCoefficients.y == null)
                {
                    return null;
                }
                return ActCoefficients.y.ToArray();
            }
            set
            {
                if (ActCoefficients == null)
                {
                    ActCoefficients = new XyPair<List<float>>(new List<float>(), new List<float>());
                }
                ActCoefficients.y = new List<float>(value);
            }
        }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float SystemDisparityPercent { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public float SystemDisparityPixels { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation)]
        public XyPair<int> DisplaySizeInMm { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public XyPair<int> ViewResolution
        {
            get
            {
                return new XyPair<int>((int)(_viewResolution.x * ResolutionScale), (int)(_viewResolution.y * ResolutionScale));
            }
            set
            {
                _viewResolution = value;
            }
        }
        public List<string> RenderModes { get; set; }
        [DisplayConfigModifyPermission(PermissionLevel.DeviceSimulation, PermissionLevel.DeveloperTuned)]
        public List<string> InterlacingMaterialEnableKeywords { get; set; }

        public float[] PredictParams { get; set; }
        public struct CameraStreamParams_
        {
            public int width;
            public int height;
            public int fps;
            public float binningFactor;
        };
        public CameraStreamParams_ CameraStreamParams { get; set; }

        /// <summary>
        ///	The User values respect the current display orientation.
        /// </summary>
        /// <value>The User values.</value>

        public XyPair<float> UserPixelPitchInMM { get; set; }
        public XyPair<int> UserPanelResolution { get; set; }
        public XyPair<int> UserNumViews { get; set; }
        public XyPair<List<float>> UserActCoefficients { get; set; }
        public float SingleTapActCoefficient { get; set; }
        public XyPair<int> UserViewResolution { get; set; }
        public XyPair<int> UserDisplaySizeInMm { get; set; }
        public float UserAspectRatio { get; set; }

        public bool UserOrientationIsLandscape
        {
            get
            {
                return _userOrientationIsLandscape;
            }
            set
            {
                _userOrientationIsLandscape = value;

                // UserActCoefficients do not get swizzled depending upon rotation
                UserActCoefficients = new XyPair<List<float>>(new List<float>(ActCoefficients.x), new List<float>(ActCoefficients.y));

                // This comparison catches an issue where all XyPairs from Android firmware are retrieved from AndroidLeiaDevice as
                // (device shorter dimension) x (device longer dimension) rather than the expected order
                // (device wide dimension) x (device height dimension).
                // In general, if Android device is in landscape orientation, devs should expect code flow to go to the "else" clause here.
                // Properties on DisplayConfig which start with User* contain correctly-transposed information.

                if (_userOrientationIsLandscape == PanelResolution.x > PanelResolution.y)
                {
                    UserPixelPitchInMM = new XyPair<float>(PixelPitchInMM.x, PixelPitchInMM.y);
                    UserPanelResolution = new XyPair<int>(PanelResolution.x, PanelResolution.y);
                    UserViewResolution = new XyPair<int>(ViewResolution.x, ViewResolution.y);
                    UserDisplaySizeInMm = new XyPair<int>(DisplaySizeInMm.x, DisplaySizeInMm.y);
                }
                else
                {
                    UserPanelResolution = new XyPair<int>(PanelResolution.y, PanelResolution.x);
                    UserViewResolution = new XyPair<int>(ViewResolution.y, ViewResolution.x);
                }
                UserAspectRatio = (float)UserPanelResolution.x / (float)UserPanelResolution.y;
            }
        }
        public float ActSingleTapCoef;
        public enum Status { SuccessfullyLoadedFromDevice, FailedToLoadFromDevice, DidntLoadYet };
        public Status status = Status.DidntLoadYet;

        //This method returns a new display config which is a copy of the old display config
        public static DisplayConfig CopyDisplayConfig(DisplayConfig DisplayConfigToCopy)
        {
            DisplayConfig result = new DisplayConfig();

            result.VersionNum = DisplayConfigToCopy.VersionNum;
            result.timeDelay = DisplayConfigToCopy.timeDelay;
            result.ResolutionScale = DisplayConfigToCopy.ResolutionScale;
            result.Gamma = DisplayConfigToCopy.Gamma;
            result.Slant = DisplayConfigToCopy.Slant;
            result.Beta = DisplayConfigToCopy.Beta;
            result.isSquare = DisplayConfigToCopy.isSquare;
            result.isSlanted = DisplayConfigToCopy.isSquare;
            result.PixelPitchInMM = new XyPair<float>(DisplayConfigToCopy.PixelPitchInMM.x, DisplayConfigToCopy.PixelPitchInMM.y);
            result.PanelResolution = new XyPair<int>(DisplayConfigToCopy.PanelResolution.x, DisplayConfigToCopy.PanelResolution.y);
            result.NumViews = new XyPair<int>(DisplayConfigToCopy.NumViews.x, DisplayConfigToCopy.NumViews.y);
            result.AlignmentOffset = new XyPair<float>(DisplayConfigToCopy.AlignmentOffset.x, DisplayConfigToCopy.AlignmentOffset.y);
            result.SystemDisparityPercent = DisplayConfigToCopy.SystemDisparityPercent;
            result.SystemDisparityPixels = DisplayConfigToCopy.SystemDisparityPixels;
            result.DisplaySizeInMm = new XyPair<int>(DisplayConfigToCopy.DisplaySizeInMm.x, DisplayConfigToCopy.DisplaySizeInMm.y);
            result.ViewResolution = new XyPair<int>(DisplayConfigToCopy.ViewResolution.x, DisplayConfigToCopy.ViewResolution.y);
            result.ActCoefficients = new XyPair<List<float>>(new List<float>(), new List<float>());
            result.UserActCoefficients = new XyPair<List<float>>(new List<float>(), new List<float>());
            result.ActCoefficients[0] = new List<float>();
            result.UserActCoefficients[0] = new List<float>();
            result.p_over_du = DisplayConfigToCopy.p_over_du;
            result.p_over_dv = DisplayConfigToCopy.p_over_dv;
            result.d_over_n = DisplayConfigToCopy.d_over_n;
            result.colorSlant = DisplayConfigToCopy.colorSlant;
            result.colorInversion = DisplayConfigToCopy.colorInversion;
            result.s = DisplayConfigToCopy.s;
            result.n = DisplayConfigToCopy.n;
            result.theta = DisplayConfigToCopy.theta;
            result.cameraCenterX = DisplayConfigToCopy.cameraCenterX;
            result.cameraCenterY = DisplayConfigToCopy.cameraCenterY;
            result.cameraCenterZ = DisplayConfigToCopy.cameraCenterZ;
            result.cameraThetaX = DisplayConfigToCopy.cameraThetaX;
            result.cameraThetaY = DisplayConfigToCopy.cameraThetaY;
            result.cameraThetaZ = DisplayConfigToCopy.cameraThetaZ;
            result.CenterViewNumber = DisplayConfigToCopy.CenterViewNumber;

            for (int i = 0; i < DisplayConfigToCopy.UserActCoefficients[0].Count; i++)
            {
                result.ActCoefficients[0].Add(DisplayConfigToCopy.ActCoefficients[0][i]);
                result.UserActCoefficients[0].Add(DisplayConfigToCopy.UserActCoefficients[0][i]);
            }
            for (int i = 0; i < DisplayConfigToCopy.UserActCoefficients[1].Count; i++)
            {
                result.ActCoefficients[1].Add(DisplayConfigToCopy.ActCoefficients[1][i]);
                result.UserActCoefficients[1].Add(DisplayConfigToCopy.UserActCoefficients[1][i]);
            }
            result.UserDisplaySizeInMm = new XyPair<int>(DisplayConfigToCopy.UserDisplaySizeInMm.x, DisplayConfigToCopy.UserDisplaySizeInMm.y);
            result.UserPixelPitchInMM = new XyPair<float>(DisplayConfigToCopy.UserPixelPitchInMM.x, DisplayConfigToCopy.UserPixelPitchInMM.y);
            result.UserNumViews = new XyPair<int>(DisplayConfigToCopy.UserNumViews.x, DisplayConfigToCopy.UserNumViews.y);
            result.UserPanelResolution = new XyPair<int>(DisplayConfigToCopy.UserPanelResolution.x, DisplayConfigToCopy.UserPanelResolution.y);
            result.UserViewResolution = new XyPair<int>(DisplayConfigToCopy.UserViewResolution.x, DisplayConfigToCopy.UserViewResolution.y);

            for (int i = 0; i < DisplayConfigToCopy.InterlacingMatrix.Length; i++)
            {
                result.InterlacingMatrix[i] = DisplayConfigToCopy.InterlacingMatrix[i];
                result.InterlacingMatrixLandscape[i] = DisplayConfigToCopy.InterlacingMatrixLandscape[i];
                result.InterlacingMatrixLandscape180[i] = DisplayConfigToCopy.InterlacingMatrixLandscape180[i];
                result.InterlacingMatrixPortrait[i] = DisplayConfigToCopy.InterlacingMatrixPortrait[i];
                result.InterlacingMatrixPortrait180[i] = DisplayConfigToCopy.InterlacingMatrixPortrait180[i];
            }
            
            for (int i = 0; i < DisplayConfigToCopy.InterlacingVector.Length; i++)
            {
                result.InterlacingVector[i] = DisplayConfigToCopy.InterlacingVector[i];
                result.InterlacingVectorLandscape[i] = DisplayConfigToCopy.InterlacingVectorLandscape[i];
                result.InterlacingVectorLandscape180[i] = DisplayConfigToCopy.InterlacingVectorLandscape180[i];
                result.InterlacingVectorPortrait[i] = DisplayConfigToCopy.InterlacingVectorPortrait[i];
                result.InterlacingVectorPortrait180[i] = DisplayConfigToCopy.InterlacingVectorPortrait180[i];
            }

            return result;
        }

        public DisplayConfig()
        {
            timeDelay = 88f;
            ResolutionScale = 1f;
            Gamma = 2.2f;
            Slant = false;
            Beta = 1.4f;
            this.isSquare = true;
            this.isSlanted = false;
            PixelPitchInMM = new XyPair<float>(1, 1);
            PanelResolution = new XyPair<int>(Screen.width, Screen.height);
            NumViews = new XyPair<int>(1, 1);
            AlignmentOffset = new XyPair<float>(0, 0);
            ActCoefficients = new XyPair<List<float>>(new List<float>(), new List<float>());
            SystemDisparityPercent = 0;
            SystemDisparityPixels = 0;
            DisplaySizeInMm = new XyPair<int>(256, 144);
            ViewResolution = new XyPair<int>(Screen.width, Screen.height);
            UserActCoefficients = new XyPair<List<float>>(new List<float>(), new List<float>());
            UserDisplaySizeInMm = new XyPair<int>(256, 144);
            UserPixelPitchInMM = new XyPair<float>(1, 1);
            UserNumViews = new XyPair<int>(1, 1);
            UserPanelResolution = new XyPair<int>(Screen.width, Screen.height);
            UserViewResolution = new XyPair<int>(Screen.width, Screen.height);
            //ConvergenceDistance = 600f;

            // legacy 
            InterlacingMatrix = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

            // matrix
            InterlacingMatrixLandscape = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            InterlacingMatrixLandscape180 = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            InterlacingMatrixPortrait = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            InterlacingMatrixPortrait180 = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

            // legacy
            InterlacingVector = new float[] { 0, 0, 0, 0 };

            // vector
            InterlacingVectorLandscape = new float[] { 0, 0, 0, 0 };
            InterlacingVectorLandscape180 = new float[] { 0, 0, 0, 0 };
            InterlacingVectorPortrait = new float[] { 0, 0, 0, 0 };
            InterlacingVectorPortrait180 = new float[] { 0, 0, 0, 0 };

            RenderModes = new List<string> { "HPO", "2D" };
            InterlacingMaterialEnableKeywords = new List<string>();
        }

        /// <summary>
        /// Gets the contextual info of a property on DisplayConfig.
        /// </summary>
        /// <param name="propertyName">A property to look up info for</param>
        /// <param name="typeCode">The type of the array which the user should provide in order to set/get property data</param>
        /// <param name="arrayLength">The array's length</param>
        /// <returns>True if looking up the info of the property was successful</returns>
        public bool GetModifiableArrayPropertyData(string propertyName, out TypeCode typeCode, out int arrayLength)
        {
            PropertyInfo info = this.GetType().GetProperty(propertyName);
            typeCode = TypeCode.Empty;
            arrayLength = 0;

            if (info == null)
            {
                return false;
            }
            else if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() == typeof(XyPair<>))
            {
                // case: XyPair<GenericType>
                typeCode = Type.GetTypeCode(info.PropertyType.GetGenericArguments()[0]);
                arrayLength = 2;
                return true;
            }
            else if (info.PropertyType.IsArray)
            {
                // case: System.Single[]
                typeCode = Type.GetTypeCode(info.PropertyType.GetElementType());
                arrayLength = ((Array)info.GetValue(this, reflectionIndices)).Length;
                return true;
            }
            else if (info.PropertyType.IsGenericType && info.PropertyType.GetInterfaces().Contains(typeof(IList)))
            {
                typeCode = Type.GetTypeCode(info.PropertyType.GetGenericArguments()[0]);
                arrayLength = ((IList)info.GetValue(this, reflectionIndices)).Count;
                return true;
            }
            else if (info.PropertyType.IsPrimitive)
            {
                // case: a single/bool
                typeCode = Type.GetTypeCode(info.PropertyType);
                arrayLength = 1;
                return true;
            }

            LogUtil.Log(LogLevel.Error, "Param {0} with type {1} does not support getting as Array", propertyName, info.PropertyType);
            return false;
        }

        /// <summary>
        /// Tries to get data in a property. Pushes data into the user-provided array "currentData".
        /// </summary>
        /// <typeparam name="T">Type of array that user provided for storing data in</typeparam>
        /// <param name="propertyName">Name of property on DisplayConfig to look up</param>
        /// <param name="currentData">An array which will have data pushed into it</param>
        /// <returns>True if we knew a way to convert a property on DisplayConfig into an array of data</returns>
        public bool TryGetModifiableArrayProperty<T>(string propertyName, out T[] currentData)
        {
            try
            {
                PropertyInfo info = this.GetType().GetProperty(propertyName);
                if (info == null)
                {
                    currentData = default(T[]);
                    return false;
                }
                Type actualPropertyType = info.PropertyType;

                if (actualPropertyType.IsGenericType && actualPropertyType.GetGenericTypeDefinition() == typeof(XyPair<>))
                {
                    XyPair<T> pairData = (XyPair<T>)info.GetValue(this, reflectionIndices);
                    currentData = new T[] { pairData.x, pairData.y };
                    return true;
                }
                else if (actualPropertyType.IsGenericType && actualPropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    List<T> mList = (List<T>)info.GetValue(this, reflectionIndices);
                    currentData = mList.ToArray();
                    return true;
                }
                else if (info.PropertyType.IsArray)
                {
                    // if propertyType is already an array, we can just do a cast from Array to T[]. also see Array.CreateInstance
                    currentData = (T[])info.GetValue(this, reflectionIndices);
                    return true;
                }
                else if (actualPropertyType.IsPrimitive)
                {
                    currentData = new T[] { (T)info.GetValue(this, reflectionIndices) };
                    return true;
                }
                else
                {
                    LogUtil.Log(LogLevel.Error, "No definition for how to get property {0} as array. Property type {1}", propertyName, typeof(T));
                }


            }
            catch (Exception e)
            {
                LogUtil.Log(LogLevel.Error, "When trying to get property {0} got error {1}", propertyName, e);
            }
            currentData = null;
            return false;
        }

        /// <summary>
        /// Gets modifiable properties on DisplayConfig.
        /// </summary>
        /// <param name="queryModifyPermissionLevel">The modification level which we wish to search for</param>
        /// <returns>A collection of property names</returns>
        public IEnumerable<string> GetModifiablePropertyNames(PermissionLevel queryModifyPermissionLevel)
        {
            IEnumerable<PropertyInfo> properties = this.GetType().GetProperties();
            HashSet<string> modifiablePropertyNames = new HashSet<string>();

            foreach (PropertyInfo prop in properties)
            {
                DisplayConfigModifyPermission[] permissions = (DisplayConfigModifyPermission[])prop.GetCustomAttributes(typeof(DisplayConfigModifyPermission), true);
                if (permissions != null && permissions.Length > 0 && permissions[0].Contains(queryModifyPermissionLevel))
                {
                    modifiablePropertyNames.Add(prop.Name);
                }
            }

            return modifiablePropertyNames;
        }

        /// <summary>
        /// Sets a value on the DisplayConfig.
        ///
        /// Does NOT trigger a state update. Set all your properties, then call LeiaDisplay.UpdateLeiaState.
        /// </summary>
        /// <param name="propertyName">A name of a property on the DisplayConfig</param>
        /// <param name="data">An array of data to be converted into a property on the DisplayConfig</param>
        /// <param name="accessLevel">An edit permission level which is compared to the property's edit permission level</param>
        public void SetPropertyByReflection(string propertyName, Array data, PermissionLevel accessLevel)
        {
            // check property exists
            PropertyInfo propertyInfo = this.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                LogUtil.Log(LogLevel.Error, "No property {0} on DisplayConfig", propertyName);
                return;
            }

            // check property access level
            DisplayConfigModifyPermission propertyPermissionLevel = (DisplayConfigModifyPermission)propertyInfo.GetCustomAttributes(typeof(DisplayConfigModifyPermission), true)[0];
            if (propertyPermissionLevel == null)
            {
                LogUtil.Log(LogLevel.Error, "Property {0} has no DisplayConfigModifyPermission", propertyName);
                return;
            }

            SetPropertyByContext(propertyInfo, propertyName, data);
        }

        private static readonly object[] reflectionIndices = new object[] { };

        /// <summary>
        /// Sets a property on DisplayConfig using reflection.
        /// </summary>
        /// <param name="info">Contextual information about the property being set</param>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="data">An array of data. Elements will be passed to the property's setter</param>
        private void SetPropertyByContext(PropertyInfo info, string propertyName, Array data)
        {
            if (info.PropertyType.IsArray)
            {
                // Set ActCoefficientsX / ActCoefficientsY, which converts into XyPair<List<float>> :: ActCoefficients
                info.SetValue(this, data, reflectionIndices);
            }
            else if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() == typeof(XyPair<>))
            {
                // Set property XyPair<T> where T is the property's type
                Type XyPairProcedural = (typeof(XyPair<>)).MakeGenericType(info.PropertyType.GetGenericArguments()[0]);
                // instantiates a new XyPair<T>(data[0], data[1])
                object xyPairGeneric = Activator.CreateInstance(XyPairProcedural, data.GetValue(0), data.GetValue(1));

                info.SetValue(this, xyPairGeneric, reflectionIndices);
            }
            else if (info.PropertyType.IsPrimitive)
            {
                // Set a primitive
                info.SetValue(this, data.GetValue(0), reflectionIndices);
            }
            else if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listProcedural = (typeof(List<>)).MakeGenericType(info.PropertyType.GetGenericArguments()[0]);
                object listGeneric = Activator.CreateInstance(listProcedural, data);
                info.SetValue(this, listGeneric, reflectionIndices);
            }
            else
            {
                LogUtil.Log(LogLevel.Warning, "No definition for how to assign property {0} with type {1}", propertyName, info.PropertyType);
            }
        }

        string ListToString(List<float> list)
        {
            string result = "";

            int count = list.Count;

            for (int i = 0; i < count; i++)
            {
                result += list[i] + ", ";
            }

            if (result == "")
                result = "Empty List";

            return result;
        }

        public string ToStringV2()
        {
            return
                "\nStatus: " + status.ToString() + "\n" +
                "\nVersion: " + VersionNum.ToString() + "\n" +

                "\n-- DISPLAY PARAMETERS --" + "\n" +
                "CenterViewNumber = " + CenterViewNumber + "\n" +
                "n = " + n + "\n" +
                "theta = " + theta + "\n" +
                "ResolutionScale = " + ResolutionScale + "\n" +
                "_panelResolution = " + _panelResolution + "\n" +
                "_viewboxSize = " + _viewboxSize + "\n" +
                "_viewResolution = " + _viewResolution + "\n" +
                "_userOrientationIsLandscape = " + _userOrientationIsLandscape + "\n" +
                "numViews = " + NumViews + "\n" +
                "colorInversion = " + colorInversion + "\n" +
                "d_over_n = " + d_over_n + "\n" +
                "p_over_du = " + p_over_du + "\n" +
                "p_over_dv = " + p_over_dv + "\n" +
                "colorSlant = " + colorSlant + "\n" +
                "Beta = " + Beta + "\n" +
                "Gamma = " + Gamma + "\n" +
                "s = " + s + "\n" +
                "PixelPitchInMM = " + this.PixelPitchInMM + "\n" +
                "UserPixelPitchInMM = " + this.UserPixelPitchInMM + "\n" +
                "DisplaySizeInMm = " + this.DisplaySizeInMm + "\n" +
                "UserDisplaySizeInMm = " + this.UserDisplaySizeInMm + "\n" +

                "\n\n-- ACT PARAMETERS --" + "\n" +
                "\nACTX: " + ListToString(this.UserActCoefficients[0]) +

                "\n\n-- CAMERA PARAMETERS --" + "\n" +
                "cameraCenterX = " + cameraCenterX + "\n" +
                "cameraCenterY = " + cameraCenterY + "\n" +
                "cameraCenterZ = " + cameraCenterZ + "\n" +
                "cameraThetaX = " + cameraThetaX + "\n" +
                "cameraThetaY = " + cameraThetaY + "\n" +
                "cameraThetaZ = " + cameraThetaZ + "\n" +
                ""
                ;
        }

        /// <summary>
        /// Creates a lightly formatted string containing DisplayConfig info
        /// </summary>
        /// <returns>A DisplayConfig header, plus some notable properties of the DisplayConfig</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("DisplayConfig: {0}\n", this.GetHashCode()));

            foreach (string propertyName in this.GetModifiablePropertyNames(PermissionLevel.DeviceSimulation))
            {
                Array t = Array.CreateInstance(typeof(Nullable), 0);
                TypeCode code = TypeCode.Empty;
                int len = -1;

                this.GetModifiableArrayPropertyData(propertyName, out code, out len);
                if (code == TypeCode.String)
                {
                    string[] stringArr = new string[0];
                    TryGetModifiableArrayProperty(propertyName, out stringArr);
                    t = stringArr;
                }
                else if (code == TypeCode.Single)
                {
                    float[] floatArr = new float[0];
                    TryGetModifiableArrayProperty(propertyName, out floatArr);
                    t = floatArr;
                }
                else if (code == TypeCode.Int16 || code == TypeCode.Int32 || code == TypeCode.Int64)
                {
                    int[] intArr = new int[0];
                    TryGetModifiableArrayProperty(propertyName, out intArr);
                    t = intArr;
                }
                else if (code == TypeCode.Boolean)
                {
                    bool[] boolArr = new bool[0];
                    TryGetModifiableArrayProperty(propertyName, out boolArr);
                    t = boolArr;
                }
                else
                {
                    LogUtil.Log(LogLevel.Error, "For DisplayConfig {0} property {1} could not get array of type {2}", this.GetHashCode(), propertyName, code);
                }

                sb.Append(string.Format("\t{0}", propertyName));
                foreach (var elem in t)
                {
                    sb.AppendFormat("\t{0}", elem);
                }
                if (t == null || t.Length == 0)
                {
                    sb.AppendFormat(" is empty");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public class XyPair<T> : IEnumerable<T>
    {
        public T x { get; set; }
        public T y { get; set; }

        public XyPair(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public static explicit operator XyPair<T>(T[] tArr)
        {
            const int minLen = 2;
            if (tArr == null || tArr.Length < minLen)
            {
                LogUtil.Log(LogLevel.Error, "Cannot construct XyPair<{0}> from array. It is null or length less than {1}", typeof(T), minLen);
                return new XyPair<T>(default(T), default(T));
            }
            return new XyPair<T>(tArr[0], tArr[1]);
        }

        public override string ToString()
        {
            return string.Format("x: {0}. y: {1}", x, y);
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return x;
            yield return y;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return x;
            yield return y;
        }

        public T this[string index]
        {
            get
            {
                if (index.ToLowerInvariant() == "x") return this[0];
                return this[1];
            }
            set
            {
                if (index.ToLowerInvariant() == "x") this[0] = value;
                else this[1] = value;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index == 0) return x;
                return y;
            }
            set
            {
                if (index == 0) x = value;
                else y = value;
            }
        }
    }
}
