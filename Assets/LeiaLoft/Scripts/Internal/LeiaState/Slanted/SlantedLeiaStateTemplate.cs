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
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// ILeiaState implementation for Square-type displays
    /// </summary>
    public class SlantedLeiaStateTemplate : AbstractLeiaStateTemplate
    {

        // Need to replace these with proper shaders
        public static string OpaqueShaderName { get { return "LeiaLoft_Slanted_8V"; } }
        public static string OpaqueShaderNameLimitedViews { get { return "LeiaLoft_Slanted_8V"; } }
        public static string TransparentShaderName { get { return "LeiaLoft_Slanted_8V_Blending"; } }
        public static string TransparentShaderNameLimitedViews { get { return "LeiaLoft_Slanted_8V_Blending"; } }

        //Sharpening
        public static string SharpeningShaderName { get { return "LeiaLoft_ViewSharpening"; } }
        private Material _sharpening;

#if UNITY_ANDROID
        List<float> actCoeffs = new List<float>(new float[4]);
#endif
 /// <remove_from_public>
#if UNITY_STANDALONE_WIN
        List<float> actCoeffs;
        private bool isInitialzed = false;
#endif
/// </remove_from_public>
        public SlantedLeiaStateTemplate(DisplayConfig displayConfig) : base(displayConfig)
        {
            // this method was left blank intentionally
        }

        protected override Material CreateMaterial(bool alphaBlending)
        {
            if (_shaderName == null)
            {
                if (_viewsHigh * _viewsWide <= 8)
                {
                    SetShaderName(OpaqueShaderNameLimitedViews, TransparentShaderNameLimitedViews);
                }
                else
                {
                    SetShaderName(OpaqueShaderName, TransparentShaderName);
                }
            }

            return base.CreateMaterial(alphaBlending);
        }

        public override void DrawImage(LeiaCamera camera, LeiaStateDecorators decorators)
        {
            base.DrawImage(camera, decorators);
            if (LeiaDisplay.Instance.ACTEnabled)
            {
                Graphics.Blit(interlacedAlbedoTexture, Camera.current.activeTexture, _sharpening);
            }
            else
            {
                Graphics.Blit(interlacedAlbedoTexture, Camera.current.activeTexture);
            }
        }

        public void UpdateSharpeningParameters()
        {
            if (_sharpening == null)
            {
                _sharpening = new Material(Resources.Load<Shader>(SharpeningShaderName));
            }

            //////////////NEW CODE COPIED FROM UNREAL
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();

/// <remove_from_public>
#if UNITY_STANDALONE_WIN
            if (!isInitialzed && config.UserActCoefficients[0].Count > 0)
            {
                actCoeffs = new List<float>(new float[config.UserActCoefficients[0].Count]);
                isInitialzed = true;
            }
#endif
/// </remove_from_public>

            // Get input ACT coefficients.
#if UNITY_ANDROID
            if (config.UserActCoefficients[0].Count > 0)
            {
                actCoeffs[0] = config.UserActCoefficients[0][0];
                actCoeffs[1] = config.UserActCoefficients[0][1];
            }
            if (config.UserActCoefficients[1].Count > 0)
            {
                actCoeffs[2] = config.UserActCoefficients[1][0];
                actCoeffs[3] = config.UserActCoefficients[1][1];
            }
#endif
/// <remove_from_public>
#if UNITY_STANDALONE_WIN
            if (config.UserActCoefficients[0].Count > 0 && isInitialzed)
            {
                for(int i = 0; i < config.UserActCoefficients[0].Count; i++)
                {
                    actCoeffs[i] = config.UserActCoefficients[0][i];
                }
            }
#endif
/// </remove_from_public>
            int actCoeffsCount = actCoeffs.Count;

            // Compute view step rates.
            int p_over_du = (int)config.p_over_du;
            int p_over_dv = (int)config.p_over_dv;

            int numViews = _displayConfig.NumViews.x;

            bool trackerIsOn = LeiaDisplay.Instance.tracker != null && LeiaDisplay.Instance.tracker.enabled && LeiaDisplay.Instance.tracker.CameraConnected;

            int numFaces = 0;


            int tapIndex = (int)Mathf.Floor(numViews / 2);

            float singleTapCoef = 0;
            float beta = 0;

#if UNITY_ANDROID
#if !UNITY_EDITOR
            singleTapCoef = LeiaDisplay.Instance.sdkConfig.act_singleTapCoef;
            beta = LeiaDisplay.Instance.sdkConfig.act_beta;
#endif        
/// <remove_from_public>
#else
            singleTapCoef = config.ActSingleTapCoef;
            beta = config.Beta;
/// </remove_from_public>
#endif
            if (LeiaDisplay.Instance.tracker != null)
            {
                numFaces = LeiaDisplay.Instance.tracker.NumFaces;
            }

            // Compute normalizer from all act values and beta.
            float normalizer = 1.0f;
            if (LeiaDisplay.Instance.ActMode == LeiaDisplay.ACTMODE.MULTIVIEW)
            {
                for (int i = 0; i < actCoeffsCount; i++)
                    normalizer -= beta * actCoeffs[i];
            }
            else if (LeiaDisplay.Instance.ActMode == LeiaDisplay.ACTMODE.SINGLETAP)
            {
                normalizer -= .5f * beta * singleTapCoef;
            }
            else if (LeiaDisplay.Instance.ActMode == LeiaDisplay.ACTMODE.OFF)
            {
                return;
            }

            List<Vector4> sharpeningVectors = new List<Vector4>();

            // Compute normalized sharpening shader values (OfsX, OfsY, Weight)

            for (int i = 1; i <= actCoeffsCount; i++)
            {
                float x0 = Mathf.Floor((float)i / (float)p_over_du);
                float x1 = -x0;
                float y0 = p_over_dv * (i % p_over_du);
                float y1 = -y0;
                float z = actCoeffs[i - 1] / normalizer;

                // Add two sharpening values.

                sharpeningVectors.Add(new Vector4(x0, y0, z, 0));
                sharpeningVectors.Add(new Vector4(x1, y1, z, 0));
            }

            // when game engine is already in linear color space, it is as if gamma is 1
            float correctedGamma = QualitySettings.activeColorSpace == ColorSpace.Linear ? 1f : _displayConfig.Gamma;

            const string gammaToken = "_gamma";
            const string sharpeningCenterToken = "_sharpeningCenter";
            const string sharpeningXYToken = "_sharpeningXY";
            const string sharpeningXYLengthToken = sharpeningXYToken + "_Length";
            const string sharpeningColorSlant = "_colorSlant";
            const string sharpeningPOverDU = "_p_over_du";
            const string tapIndexToken = "_tapIndex";
            const string singleTapCoefToken = "_singleTapCoef";

            // export data to shader
            _sharpening.SetFloat(gammaToken, correctedGamma);
            _sharpening.SetFloat(sharpeningCenterToken, 1.0f / normalizer);


            _sharpening.SetInt(tapIndexToken, tapIndex);
            _sharpening.SetFloat(singleTapCoefToken, singleTapCoef);

            /*
            Debug.Log("_tapIndex: " + tapIndex);
            Debug.Log("_sharpeningXY.Length: " + sharpeningVectors.Count);

            Debug.Log("_sharpeningXY[2 * (_tapIndex - 1)].xy: "
                + sharpeningVectors[2 * (tapIndex - 1)].x + " " +
                sharpeningVectors[2 * (tapIndex - 1)].y);

            Debug.Log("sharpening Center: " + 1.0f / normalizer);
            */

            int count = sharpeningVectors.Count;
            if (count > 0)
            {
                _sharpening.SetVectorArray(sharpeningXYToken, sharpeningVectors);
            }
            _sharpening.SetFloat(sharpeningColorSlant, _displayConfig.colorSlant);
            _sharpening.SetFloat(sharpeningPOverDU, _displayConfig.p_over_du);
            _sharpening.SetFloat(sharpeningXYLengthToken, sharpeningVectors.Count);
        }

        public override void GetFrameBufferSize(out int width, out int height)
        {
            var tileWidth = _displayConfig.ViewResolution.x;
            var tileHeight = _displayConfig.ViewResolution.y;
            width = (int)(_viewsWide * tileWidth);
            height = (int)(_viewsHigh * tileHeight);
        }

        public override void GetTileSize(out int tileWidth, out int tileHeight)
        {
            tileWidth = _displayConfig.UserViewResolution.x;
            tileHeight = _displayConfig.UserViewResolution.y;
        }

        public override void UpdateViews(LeiaCamera leiaCamera)
        {
            base.UpdateViews(leiaCamera);

            var near = Mathf.Max(1.0e-5f, leiaCamera.NearClipPlane);
            var far = Mathf.Max(near, leiaCamera.FarClipPlane);

            Matrix4x4 m = Matrix4x4.zero;

            float interocular = 63; //In MM

            float manualZCompensation = 1.0f;

            if (!leiaCamera.cameraZaxisMovement)
            {
                manualZCompensation = LeiaDisplay.Instance.displayConfig.ConvergenceDistance / LeiaDisplay.Instance.viewerPositionNonPredicted.z;
            }

            float baseline = (leiaCamera.FinalBaselineScaling * interocular * leiaCamera.virtualDisplay.width / ((_displayConfig.PixelPitchInMM.x) * _displayConfig.PanelResolution.x)) * manualZCompensation;

            float viewsPerIO = LeiaDisplay.Instance.displayConfig.ViewsPerInterocularAtConvergence();

            //If in LF mode, divide baseline by views per interocular (the number of views between the viewer's eyes)
            if (LeiaDisplay.Instance.DesiredRenderTechnique == LeiaDisplay.RenderTechnique.Multiview)
            {
                baseline /= viewsPerIO;
            }

            float viewsPerIOAtFaceZ = LeiaDisplay.Instance.displayConfig.ViewsPerInterocularAtFaceZ(LeiaDisplay.Instance.viewerPositionNonPredicted.z);
            int NumViews = LeiaDisplay.Instance.displayConfig.NumViews.x;
            float SafetyDiameter = (2f * Mathf.Floor(NumViews / 2f)); //Adjusting for odd number of views

            if (LeiaDisplay.Instance.CloseRangeSafety)
            {
                float closeRangeSafety = 1f - Mathf.SmoothStep(0, 1, Mathf.Clamp((viewsPerIOAtFaceZ - SafetyDiameter + 2f) / 2f, 0, 1));
                baseline *= closeRangeSafety;
            }

            System.Func<int, int, float> GetPosX = (nx, ny) =>
            {
                if (leiaCamera.Camera.orthographic) { return GetEmissionX(nx, ny) * baseline + leiaCamera.CameraShift.x; }
                else { return baseline * (GetEmissionX(nx, ny)) + leiaCamera.CameraShift.x; }
            };
            System.Func<int, int, float> GetPosY = (nx, ny) =>
            {
                if (leiaCamera.Camera.orthographic) { return GetEmissionY(nx, ny) * baseline + leiaCamera.CameraShift.y; }
                else { return baseline * (GetEmissionY(nx, ny)) + leiaCamera.CameraShift.y; }
            };

            for (int ny = 0; ny < _viewsHigh; ny++)
            {
                for (int nx = 0; nx < _viewsWide; nx++)
                {
                    var viewId = ny * _viewsWide + nx;
                    var view = leiaCamera.GetView(viewId);

                    if (view.IsCameraNull)
                    {
                        continue;
                    }

                    float posx = GetPosX(nx, ny);
                    float posy = GetPosY(nx, ny);

                    // must set position before calculating projection-for-position
                    view.Position = new Vector3(posx, posy, leiaCamera.CameraShift.z);

                    m = CameraCalculatedParams.GetConvergedProjectionMatrixForPosition(view.Camera, leiaCamera.transform.position + leiaCamera.transform.forward * leiaCamera.ConvergenceDistance);

                    view.Matrix = m;
                    view.NearClipPlane = near;
                    view.FarClipPlane = far;
                }
            }
        }


        public override void UpdateState(LeiaStateDecorators decorators, ILeiaDevice device)
        {
            if (_material == null)
            {
                _material = CreateMaterial(decorators.AlphaBlending);
            }

            // inside of CheckRenderTechnique, write into UserNumViews based upon stereo and device orientation and call SetViewCount
            SetUserNumViewsFromDecoratorsAndDevice(decorators, device);

            RespectOrientation(decorators);
            UpdateEmissionPattern(decorators);
            var shaderParams = new ShaderFloatParams();

            shaderParams._width = _displayConfig.UserPanelResolution.x;
            shaderParams._height = _displayConfig.UserPanelResolution.y;
            shaderParams._viewResX = _displayConfig.UserViewResolution.x;
            shaderParams._viewResY = _displayConfig.UserViewResolution.y;

            //TODO: Set d over n from display config

            shaderParams.faceX = decorators.FacePosition.x;
            shaderParams.faceY = decorators.FacePosition.y;
            shaderParams.faceZ = decorators.FacePosition.z;

            if (decorators.DeltaXArray != null)
            {
                shaderParams._deltaXArray = decorators.DeltaXArray;
                shaderParams._deltaXArraySize = decorators.DeltaXArray.Length;
            }

            var offset = _displayConfig.AlignmentOffset;
            shaderParams._offsetX = offset.x + (decorators.ParallaxOrientation.IsInv() ? XOffsetWhenInverted() : 0);
            shaderParams._offsetY = offset.y + (decorators.ParallaxOrientation.IsInv() ? YOffsetWhenInverted() : 0);

            // _displayHorizontalViewCount represents the display's actual view count along the horizontal / major axis
            // can be NumViews.x or NumViews.y depending upon display orientation
            shaderParams._viewsX = _displayHorizontalViewCount;
            // due to SetUserNumViewsFromDecoratorsAndDevice calling SetViewCount (h, 1), _viewsHigh will always be 1
            shaderParams._viewsY = _viewsHigh;

            shaderParams._orientation = decorators.ParallaxOrientation.IsLandscape() ? 1 : 0;
            shaderParams._adaptFOVx = decorators.AdaptFOV.x;
            shaderParams._adaptFOVy = decorators.AdaptFOV.y;
            shaderParams._enableSwizzledRendering = 1;
            shaderParams._enableHoloRendering = 1;
            shaderParams._enableSuperSampling = 0;
            shaderParams._separateTiles = 1;


            var is2d = shaderParams._viewsY == 1 && shaderParams._viewsX == 1;

            if (decorators.ShowTiles || is2d)
            {
                shaderParams._enableSwizzledRendering = 0;
                shaderParams._enableHoloRendering = 0;
            }

            // enable interlacing view count in interlacing shader
            _material.EnableKeyword(string.Format("LEIA_INTERLACING_READ_{0}V", _displayHorizontalViewCount - 1));

            shaderParams._isFlippedAlignment = 0.0f;

            ScreenOrientation orientation = device.GetScreenOrientationRGB();

            float[] selectedInterlacingMatrix = _displayConfig.getInterlacingMatrixForOrientation(orientation);
            shaderParams._interlace_matrix = selectedInterlacingMatrix.ToMatrix4x4();
            float[] selelctedInterlacingVector = _displayConfig.getInterlacingVectorForOrientation(orientation);
            shaderParams._interlace_vector = selelctedInterlacingVector.ToVector4();

            if (decorators.ShowTiles)
            {
                _material.EnableKeyword("ShowTiles");
            }
            else
            {
                _material.DisableKeyword("ShowTiles");
            }

            // note - since this is ILeiaState.UpdateState, be sure that you called UpdateState
            _material.SetFloat("dynamic_interlace_scale", 1.0f);
            _material.SetFloat("dynamic_interlace_cos", 1.0f);
            _material.SetFloat("dynamic_interlace_sin", 0.0f);

            if(_displayConfig.colorSlant != 0)
                SetShaderSubpixelKeywordsFromMatrix(shaderParams._interlace_matrix);

            foreach (string keyword in _displayConfig.InterlacingMaterialEnableKeywords)
            {
                _material.EnableKeyword(keyword);
            }

            SetInterlacedBackgroundPropertiesFromDecorators(decorators);

            if (LeiaDisplay.Instance.PerPixelCorrectionEnabled)
            {
                if (LeiaDisplay.Instance.DesiredRenderTechnique == LeiaDisplay.RenderTechnique.Stereo)
                {
                    shaderParams.perPixelCorrection = 2;
                }
                else
                {
                    shaderParams.perPixelCorrection = 1;
                }
            }
            else
            {
                shaderParams.perPixelCorrection = 0;
            }

            shaderParams._showCalibrationSquares = decorators.ShowCalibration ? 1 : 0;
            shaderParams.ApplyTo(_material);

            UpdateSharpeningParameters();
        }

        public override void Release()
        {
            // release sharpening _material
            if (_sharpening != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(_sharpening);
                }
                else
                {
                    GameObject.DestroyImmediate(_sharpening);
                }
            }
            base.Release();
        }
    }
}
