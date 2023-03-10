#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LeiaLoft
{
    public class InterlacingPreviewWindow : EditorWindow
    {
        private RenderTexture myRT;
        private RenderTextureExtractor extractor;
        private LeiaRenderCamera renderCam;

        int rtWidth = -1;
        int rtHeight = -1;

        const int idealWidth = 3840;
        const int idealHeight = 2160;

        /// <summary>
        /// Method for constructing an instance of the InterlacingPreviewWindow class / custom window
        /// </summary>
        [MenuItem("LeiaLoft/8-view preview %e")]
        static void Init()
        {
            InterlacingPreviewWindow previewWindow = GetWindow<InterlacingPreviewWindow>("8-view preview", true);
            previewWindow.autoRepaintOnSceneChange = true;
            previewWindow.Show();
        }

        /// <summary>
        /// Gets a LeiaRenderCamera in scene if possible.
        /// Might return null.
        /// </summary>
        /// <returns></returns>
        LeiaRenderCamera GetRenderCamera()
        {
            LeiaRenderCamera rc;
            if (LeiaCamera.Instance != null && LeiaCamera.Instance.GetComponent<LeiaRenderCamera>() != null)
            {
                rc = LeiaCamera.Instance.GetComponentInChildren<LeiaRenderCamera>();
            }
            else
            {
                rc = FindObjectOfType<LeiaRenderCamera>();
            }

            return (rc);
        }

        /// <summary>
        /// InterlacingPreviewWindow :: OnGUI
        /// </summary>
        void OnGUI()
        {
            if (Application.isPlaying)
            {
                if (renderCam == null)
                {
                    // acquire a renderCamera if missing
                    renderCam = GetRenderCamera();
                    if (renderCam != null && renderCam.GetComponent<Camera>() != null)
                    {
                        rtWidth = renderCam.GetComponent<Camera>().pixelWidth;
                        rtHeight = renderCam.GetComponent<Camera>().pixelHeight;

                        if (myRT != null)
                        {
                            myRT.Release();
                        }
                        myRT = new RenderTexture(rtWidth, rtHeight, 24) { filterMode = FilterMode.Point };
                    }
                }

                if (renderCam != null && extractor == null && myRT != null)
                {
                    // if we do not have a reference to an extractor yet, add one and set it to copy into myRT that we constructed
                    extractor = renderCam.gameObject.AddComponent<RenderTextureExtractor>();
                    extractor.SetRenderTexture(myRT);
                }

                if (myRT != null)
                {
                    GUI.DrawTextureWithTexCoords(new Rect(0.0f, 0.0f, position.width, position.height), myRT, new Rect(0.0f, 0.0f, position.width * 1.0f / rtWidth, position.height * 1.0f / rtHeight));
                }

                // warnings
                if (renderCam != null && LeiaCamera.Instance != null && LeiaCamera.Instance.GetViewCount() > 0 && LeiaCamera.Instance.GetViewCount() < 8)
                {
                    GUIContent gc = new GUIContent(string.Format("{0}-view preview not supported", LeiaCamera.Instance.GetViewCount()));
                    this.ShowNotification(gc);
                }
                else if (myRT == null) 
                {
                    // add additional draw command so users see something different, which will cue them a bit
                    GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), Texture2D.whiteTexture);
                    GUIContent gc = new GUIContent("no renderTexture");
                    this.ShowNotification(gc);
                }
                else if (position.width < idealWidth * 0.9f || position.height < idealHeight * 0.9f)
                {
                    GUIContent gc = new GUIContent(string.Format("Window dims {0:F0} x {1:F0}. Want {2} x {3}", position.width, position.height, rtWidth, rtHeight));
                    this.ShowNotification(gc);
                }

            }
            else
            {
                GUIContent gc = new GUIContent("No preview outside play mode");
                this.ShowNotification(gc);
            }
        }

        private void OnDestroy()
        {
            if (extractor != null)
            {
                // editor only
                Destroy(extractor);
            }
            if (myRT != null)
            {
                myRT.Release();
            }
        }

    }
}

#endif
