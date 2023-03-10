/****************************************************************
*
* Copyright 2019 Â© Leia Inc.  All rights reserved.
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace LeiaLoft
{
    /// <summary>
    /// Class for spawning an object with a LeiaMaterial in front of the LeiaCamera.
    /// The LeiaMaterial gives different views to different LeiaCameras.
    /// Media types which can be rendered using a LeiaMaterial include images (textures) and video.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]

    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(LeiaMediaVideoPlayer))]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/modules/leia-media")]
    public class LeiaMediaViewer : UnityEngine.MonoBehaviour, ILeiaMediaMaterialHandler
    {

    public delegate void OnMediaChanged();
        public event OnMediaChanged VideoChangedResponses;

        // variables which should always be present and used
        private MeshRenderer mr;
        private MeshFilter mf;
        private MaterialPropertyBlock mpb;

        // variables which may be hidden in some cases
        private VideoPlayer vp;
        private AudioSource aud_source;
        private LeiaMediaVideoPlayer lmvp;

        public bool automaticAspectRatio = true;

        [SerializeField] private string leiaMediaVideoURL;
        [SerializeField] private VideoClip leiaMediaVideoClip;
        [SerializeField] private Texture leiaMediaTexture;

        private static Material LeiaMaterial;
        private static readonly string LeiaMaterial_id = "Materials/LeiaMediaMaterial";

        public enum LeiaMediaType
        {
            Texture,
            Video,
            VideoURL
        }

        public enum MediaScaleMode
        {
            WorldXYZScale,
            OnscreenPercent
        }

        /// <summary>
        /// Getter/setter for activeMediaType as integer. Useful for controlling media from Unity drop-down
        /// </summary>
        public int activeMediaTypeInt
        {
            get
            {
                return (int) _activeMediaType;
            }
            set
            {
                activeMediaType = (LeiaMediaType)value;
            }
        }
          
        public Rect OnscreenPercent
        {
            get
            {
                return onscreenPercent;
            }

            set
            {
                onscreenPercent = value;
                Rebuild();
            }
        }

        [SerializeField] private LeiaMediaType _activeMediaType;
        public LeiaMediaType activeMediaType
        {
            get
            {
                return _activeMediaType;
            }
            set
            {
                _activeMediaType = value;
                Rebuild();
            }
        }

        [SerializeField] private MediaScaleMode _mediaScaleMode;
        public MediaScaleMode mediaScaleMode
        {
            get
            {
                return _mediaScaleMode;
            }
            set
            {
                _mediaScaleMode = value;
                Rebuild();
            }
        }

        private static readonly string id_main_tex = "_MainTex";
        private static readonly string id_col_count = "_ColCount";
        private static readonly string id_row_count = "_RowCount"; 
        private static readonly string id_user_view_count = "_UserViewCount";
        private static readonly string id_device_view_count = "_DeviceViewCount";
        private static readonly string id_OnscreenPercent = "_OnscreenPercent";
        private static readonly string id_EnableOnscreenPercent = "_EnableOnscreenPercent";

        public Vector3 maxScaleBeforeAspectRatio = Vector3.one;

        [Tooltip("X,Y: offset from left bottom screen corner, W : width H: height")]
        [SerializeField] private Rect onscreenPercent = new Rect(0, 0, 1, 1);

        void OnEnable()
        {

            if (!LeiaDisplay.InstanceIsNull)
            {
                LeiaDisplay.Instance.StateChanged += ExportShaderParams;
            }
        }

        void OnDisable()
        {
            if (!LeiaDisplay.InstanceIsNull)
            {
                LeiaDisplay.Instance.StateChanged -= Rebuild;
            }
        }

        void Awake()
        {
            Rebuild();
        }

        void ExportShaderParams()
        {
            int property_col_count = mediaTileLayoutsCols[(int)activeMediaType];
            int property_row_count = mediaTileLayoutsRows[(int)activeMediaType];
            mpb.SetFloat(id_col_count, property_col_count);
            mpb.SetFloat(id_row_count, property_row_count);
            if (!LeiaDisplay.InstanceIsNull)
            {
                DisplayConfig dc = LeiaDisplay.Instance.GetDisplayConfig();
                if (LeiaDisplay.Instance.DesiredLightfieldMode == LeiaDisplay.LightfieldMode.On)
                {
                    mpb.SetFloat(id_user_view_count, dc.UserNumViews.x);
                }
                else
                {
                    mpb.SetFloat(id_user_view_count, 1);
                }
                mpb.SetVector(id_OnscreenPercent, new Vector4(onscreenPercent.x, onscreenPercent.y, onscreenPercent.width, onscreenPercent.height));
                mpb.SetFloat(id_EnableOnscreenPercent, mediaScaleMode == MediaScaleMode.OnscreenPercent ? 1 : 0);
                mpb.SetFloat(id_device_view_count, dc.NumViews.x);
            }
            switch (activeMediaType)
            {
                case LeiaMediaType.Texture:
                    Texture nonNullTexture = Texture2D.blackTexture;
                    if (leiaMediaTexture != null)
                    {
                        nonNullTexture = leiaMediaTexture;
                    }
                    mpb.SetTexture(id_main_tex, nonNullTexture);
                    break;
                case  LeiaMediaType.VideoURL:
                    if (vp.texture)
                    {
                        mpb.SetTexture(id_main_tex, vp.texture);
                    }
                    break;
                case LeiaMediaType.Video:
                    if (vp.texture)
                    {
                        mpb.SetTexture(id_main_tex, vp.texture);
                    }
                    break;
            }
            if (mr != null)
            {
                // avoid an issue where callback for ExportRenderingParams can occur after object has been destroyed
                mr.SetPropertyBlock(mpb);
            }
        }

        /// <summary>
        /// Gathers identities of components, prepares content for export using MaterialPropertyBlock
        /// </summary>
        void Rebuild()
        {
            // script requires these components - always exist
            if (mr == null) { mr = transform.GetComponent<MeshRenderer>(); }
            if (mf == null) { mf = transform.GetComponent<MeshFilter>(); }
            if (mpb == null) { mpb = new MaterialPropertyBlock(); }
            if (aud_source == null) { aud_source = transform.GetComponent<AudioSource>(); }
            if (vp == null) { vp = transform.GetComponent<VideoPlayer>(); }
            if (lmvp == null) { lmvp = transform.GetComponent<LeiaMediaVideoPlayer>(); }

            switch(activeMediaType)
            {
                case LeiaMediaType.VideoURL:
                    if (!string.IsNullOrEmpty(leiaMediaVideoURL) && vp != null)
                    {
                        vp.enabled = true;
                        vp.url = leiaMediaVideoURL;
                        vp.source = VideoSource.Url;
                    }
                    RevealVideoComponents();
                    break;
                case LeiaMediaType.Video:
                    if (vp != null)
                    {
                        vp.enabled = true;
                        vp.clip = leiaMediaVideoClip;
                        vp.source = VideoSource.VideoClip;
                    }
                    RevealVideoComponents();
                    break;
                case LeiaMediaType.Texture:
                    if (vp != null) { vp.enabled = false; }
                    HideVideoComponents();
                    break;
                default:
                    if (vp != null) { vp.enabled = false; }
                    HideVideoComponents();
                    break;
            }

            if (LeiaMaterial == null)
            {
                LoadMat();
            }

            // mpb is attached to renderer here
            ExportRenderingParams();
            ExportShaderParams();
        }

        /// <summary>
        /// Forces component HideFlags to be hidden in inspector
        /// </summary>
        void HideVideoComponents()
        {
            if (lmvp != null)
            {
                lmvp.hideFlags = HideFlags.HideInInspector;
            }

            if (vp != null)
            {
                vp.hideFlags = HideFlags.HideInInspector;
                // address issue where video player was being hidden, but not detaching clip

                int old_id = (vp.clip == null ? 0 : vp.clip.GetInstanceID());
                int new_id = (leiaMediaVideoClip == null ? 0 : leiaMediaVideoClip.GetInstanceID());

                vp.clip = leiaMediaVideoClip;

                if (VideoChangedResponses != null && old_id != new_id)
                {
                    VideoChangedResponses();
                }
            }

            if (aud_source != null)
            {
                aud_source.hideFlags = HideFlags.HideInInspector;
            }
        }

        /// <summary>
        /// Forces component HideFlags to be None
        /// </summary>
        void RevealVideoComponents()
        {
            if (lmvp != null)
            {
                lmvp.hideFlags = HideFlags.None;
            }
            if (vp != null)
            {
                vp.hideFlags = HideFlags.None;
            }
            if (aud_source != null)
            {
                aud_source.hideFlags = HideFlags.None;
            }
        }

        // can't serialize an int[,] so have to maintain this info in two separate int[]s
        [SerializeField] private int[] mediaTileLayoutsCols = new int[System.Enum.GetNames(typeof(LeiaMediaType)).Length];
        [SerializeField] private int[] mediaTileLayoutsRows = new int[System.Enum.GetNames(typeof(LeiaMediaType)).Length];

        public int GetLeiaMediaColsFor(LeiaMediaType type)
        {
            return mediaTileLayoutsCols[(int)type];
        }
        public void SetLeiaMediaColsFor(LeiaMediaType type, int value)
        {
            mediaTileLayoutsCols[(int)type] = Mathf.Max(0, value);
            Rebuild(); 
        }

        public int GetLeiaMediaRowsFor(LeiaMediaType type)
        {
            return mediaTileLayoutsRows[(int)type];
        }
        public void SetLeiaMediaRowsFor(LeiaMediaType type, int value)
        {
            mediaTileLayoutsRows[(int)type] = Mathf.Max(0, value);
            Rebuild();
        }

        /// <summary>
        /// Static function - attempts to load the LeiaPrerendered8Material if possible
        /// </summary>
        static void LoadMat()
        {
            LeiaMaterial = Resources.Load(LeiaMaterial_id) as Material;
        }

        /// <summary>
        /// Sets properties of MPB. Sends MPB from memory to material queue
        /// </summary>
        void ExportRenderingParams()
        {
            int property_col_count = mediaTileLayoutsCols[(int)activeMediaType];
            int property_row_count = mediaTileLayoutsRows[(int)activeMediaType];

 
#if UNITY_EDITOR
            if (mr.sharedMaterial == null && LeiaMaterial != null)
            {
                mr.sharedMaterial = LeiaMaterial;
            }
#else
            if (mr.material == null && LeiaMaterial != null)
            {
                mr.material = LeiaMaterial;
            }
#endif

            if (property_row_count > 0 && property_col_count > 0)
            {
                switch (activeMediaType)
                {
                    case LeiaMediaType.VideoURL:
                        vp.enabled = false;

                        // avoid an issue where setting URL in edit mode would trigger a warning
                        if (Application.isPlaying && !string.IsNullOrEmpty(vp.url))
                        {
                            // no aspect ratio in URL-loaded files
                            vp.url = leiaMediaVideoURL.Trim();
                            vp.source = VideoSource.Url;
                        }

                        vp.enabled = true;
                        break;
                    case LeiaMediaType.Video:
                        vp.enabled = false;

                        if (vp.clip != null)
                        {
                            int old_id = (vp.clip == null ? 0 : vp.clip.GetInstanceID());
                            int new_id = (leiaMediaVideoClip == null ? 0 : leiaMediaVideoClip.GetInstanceID());
                            vp.clip = leiaMediaVideoClip;

                            // after we update the VideoPlayer.video_clip, provoke responses from listeners
                            if (VideoChangedResponses != null && old_id != new_id)
                            {
                                VideoChangedResponses();
                            }
                        }

                        vp.enabled = true;
                        break;
                    case LeiaMediaType.Texture:
                        break;
                    default:
                        LogUtil.Log(LogLevel.Error, "Missing a definition for how to set active LeiaMedia type {0}!", activeMediaType);
                        break;
                }
            }
        }
 
        /// <summary>
        /// Gets the video URL of this LeiaMediaViewer
        /// </summary>
        /// <returns></returns>
        public string GetLeiaMediaVideoURL()
        {
            return leiaMediaVideoURL;
        }

        /// <summary>
        /// Sets the video URL of this LeiaMediaViewer
        /// </summary>
        /// <param name="absolute_path">Absolute path to a video clip outside the Unity build</param>
        public void SetVideoURL(string absolute_path)
        {
            leiaMediaVideoURL = absolute_path;
            Rebuild();
        }
        /// <summary>
        /// Sets the video URL of this LeiaMediaViewer
        /// </summary>
        /// <param name="absolute_path">Absolute path to a video clip outside the Unity build</param>
        /// <param name="rows">Leia Media rows</param>
        /// <param name="cols">Leia Media cols</param>
        public void SetVideoURL(string absolute_path, int cols, int rows)
        {
            mediaTileLayoutsCols[(int)LeiaMediaType.VideoURL] = Mathf.Max(0, cols);
            mediaTileLayoutsRows[(int)LeiaMediaType.VideoURL] = Mathf.Max(0, rows);
            activeMediaType = LeiaMediaType.VideoURL;
            SetVideoURL(absolute_path);
        }

        /// <summary>
        /// Gets state of Renderer component on this object
        /// </summary>
        /// <returns>true if Renderer attached and enabled, false otherwise</returns>
        public bool GetRendererActive()
        {
            return (mr.enabled);
        }

        /// <summary>
        /// Sets renderer enabled state
        /// </summary>
        /// <param name="status">true if enabled, false otherwise</param>
        public void SetRendererActive(bool status)
        {
            if (mr == null)
            {
                mr = GetComponent<MeshRenderer>();
            }
            mr.enabled = status;
        }

        /// <summary>
        /// Toggle MeshRenderer on/off. Most useful for toggling one Leia Media off while toggling another on,
        /// so that movie can seamlessly move from one renderer to another.
        /// </summary>
        public void ToggleRenderer()
        {
            mr.enabled = !mr.enabled;
        }

        /// <summary>
        /// Sets video clip on Leia Media
        /// </summary>
        /// <param name="video_clip">A video clip which is routed through MaterialPropertyBlock</param>
        public void SetVideoClip(VideoClip video_clip)
        {
            leiaMediaVideoClip = video_clip;
            activeMediaType = LeiaMediaType.Video;
            Rebuild();
        }

        /// <summary>
        ///  Sets video clip on Leia Media 
        /// <param name="video_clip">A video clip which is routed through MaterialPropertyBlock</param>
        /// <param name="rows">Leia Media rows</param>
        /// <param name="cols">Leia Media cols</param>
        public void SetVideoClip(VideoClip video_clip, int cols, int rows)
        {
            mediaTileLayoutsCols[(int)LeiaMediaType.Video] = Mathf.Max(0, cols);
            mediaTileLayoutsRows[(int)LeiaMediaType.Video] = Mathf.Max(0, rows);
            activeMediaType = LeiaMediaType.Video;
            SetVideoClip(video_clip);
        }


        /// <summary>
        /// Sets texture on Leia Media
        /// </summary>
        /// <param name="texture"></param>
        public void SetTexture(Texture texture)
        {
            if (leiaMediaTexture != texture)
            {
                leiaMediaTexture = texture;
                activeMediaType = LeiaMediaType.Texture;
                Rebuild();
            }
        }
        /// <summary>
        /// Sets texture on Leia Media
        /// </summary>
        /// <param name="texture">texture to apply to Leia Media</param>
        /// <param name="rows">Leia Media rows</param>
        /// <param name="cols">Leia Media cols</param>
        public void SetTexture(Texture texture, int cols, int rows)
        {
            mediaTileLayoutsCols[(int)LeiaMediaType.Texture] = Mathf.Max(0, cols);
            mediaTileLayoutsRows[(int)LeiaMediaType.Texture] = Mathf.Max(0, rows);
            SetTexture(texture);
        }

        /// <summary>
        /// Switches aspect ratio regulation. By default, aspect ratio is regulated by dimensions of Leia Media
        /// </summary>
        public void ToggleAspectRatioRegulation()
        {
            automaticAspectRatio = !automaticAspectRatio;
        }

        /// <summary>
        /// Sets state of aspect ratio regulation.
        /// </summary>
        /// <param name="status">true: Leia Media's local xy scale are changed to fit the media playing on it. false: Leia Media's local xy scale are not changed</param>
        public void SetAspectRatioRegulation(bool status)
        {
            automaticAspectRatio = status;
        }

        /// <summary>
        /// Retrieves aspect ratio regulation state
        /// </summary>
        /// <returns>true if aspect ratio is corrected by Leia Media, false if aspect ratio is not corrected by Leia Media</returns>
        public bool GetAspectRatioRegulation()
        {
            return (automaticAspectRatio);
        }

        /// <summary>
        /// Forces dimensions/localScale of Leia Media to be forcedx, forcedy, same z
        /// </summary>
        /// <param name="forced_aspect_ratio">(width, height)</param>
        public void ForceAspectRatio(Vector2 forced_aspect_ratio)
        {}

        public void ProjectOntoZDP()
        {
            Transform t = null;
            LeiaCamera ideal_lc = null;

            if (Camera.main != null && Camera.main.GetComponent<LeiaCamera>() != null)
            {
                t = Camera.main.transform;
                ideal_lc = Camera.main.GetComponent<LeiaCamera>();
            }
            else if (FindObjectOfType<LeiaCamera>() != null)
            {
                t = FindObjectOfType<LeiaCamera>().transform;
                ideal_lc = t.GetComponent<LeiaCamera>();
            }
            else if (FindObjectOfType<Camera>() != null)
            {
                t = FindObjectOfType<Camera>().transform;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("No LeiaCamera or Camera in scene");
#else
                LogUtil.Debug("No LeiaCamera or Camera in scene.");
#endif

            }

            if (ideal_lc != null)
            {
                Vector3 error = ideal_lc.transform.position + ideal_lc.ConvergenceDistance * ideal_lc.transform.forward - transform.position;
                Vector3 BF = Vector3.Project(error, ideal_lc.transform.forward);
                transform.position = transform.position + BF;
                transform.rotation = ideal_lc.transform.rotation;
            }
            else if (t != null)
            {
                Vector3 error = t.position + 10.0f * t.forward - transform.position;
                Vector3 BF = Vector3.Project(error, t.forward);
                transform.position = t.position + BF;
                transform.rotation = t.rotation;
            }

        }

        /// <summary>
        /// Property that will always retrieve the string-ish equivalent of our active LeiaMedia
        /// </summary>
        public string ActiveLeiaMediaName
        {
            get
            {
                switch(activeMediaType)
                {
                    case LeiaMediaType.Texture:
                        return leiaMediaTexture != null ? leiaMediaTexture.name : "";
                    case LeiaMediaType.Video:
                        return leiaMediaVideoClip != null ? leiaMediaVideoClip.name : "";
                    case LeiaMediaType.VideoURL:
                        return !string.IsNullOrEmpty(leiaMediaVideoURL) ? leiaMediaVideoURL : "";
                    default: return "";
                }
            }
        }

        [System.Obsolete("No longer supported. Deprecated in 0.6.20")]
        /// <summary>
        /// Sets the local xy scale of the Leia Media
        /// </summary>
        /// <param name="r">Vector2(width, height)</param>
        private void FixRatio(Vector2 r)
        {
            // some variables may not be loaded at editor start time
            if ((int)r.x == 0 || (int)r.y == 0)
                return;
            if (!automaticAspectRatio)
                return;

            Vector2 ratios = new Vector2(maxScaleBeforeAspectRatio.x / r.x, maxScaleBeforeAspectRatio.y / r.y);
            float ratio = Mathf.Min(ratios.x, ratios.y);
            transform.localScale = new Vector3(r.x * ratio, r.y * ratio, transform.localScale.z);
            return;
        }
    }

}
