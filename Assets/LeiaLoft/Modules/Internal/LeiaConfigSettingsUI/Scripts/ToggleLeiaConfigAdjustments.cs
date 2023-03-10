using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeiaLoft
{
    public class ToggleLeiaConfigAdjustments : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject leiaConfigSettingsPanel;
        [SerializeField] private KeyCode key1Option1, key1Option2, key2;
        [SerializeField] private int mobileFingerCount = 3;
        [SerializeField] private int mobileTapCount = 3;
        [SerializeField] private Button xButton = null;
        private int tapCount = 0;
        private float targetTime = 0.0f;
        private float tapTime = 1.0f;
        public event System.Action SettingsPanelToggled = delegate { };
        bool CursorWasPreviouslyVisible;
        CursorLockMode PreviousCurorLockMode;

        void Awake()
        {
            if (xButton != null)
            {
                this.xButton.onClick.AddListener(OnXClick);
            }
        }
        private void OnEnable()
        {
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
            if (eventSystems.Length == 0)
            {
                GameObject obj = new GameObject("EventSystem", typeof(EventSystem));
                obj.transform.parent = null;
                obj.AddComponent<StandaloneInputModule>();
            }
        }
        void Update()
        {
            if ((Input.GetKey(key1Option1) || Input.GetKey(key1Option2)) && Input.GetKeyDown(key2)) //keyboard controls
            {
                ToggleLeiaConfig();
            }
#if UNITY_ANDROID //&& !UNITY_EDITOR
            
            if (Input.touchCount == mobileFingerCount)
            {
                Touch tap = Input.GetTouch(0);

                if (tap.phase == TouchPhase.Ended)
                {
                    tapCount += 1;
                }

                if (tapCount == 1)
                {
                    targetTime = Time.time + tapTime;
                }
                else if (tapCount >= mobileTapCount)
                {
                    ToggleLeiaConfig();
                    tapCount = 0;
                }
            }

            if (Time.time > targetTime)
            {
                tapCount = 0;
            }
#endif
        }

        private void ToggleLeiaConfig()
        {
            leiaConfigSettingsPanel.SetActive(!leiaConfigSettingsPanel.activeSelf);
            if (leiaConfigSettingsPanel.activeSelf)
            {
                CursorWasPreviouslyVisible = Cursor.visible;
                PreviousCurorLockMode = Cursor.lockState;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
#if UNITY_ANDROID && !UNITY_EDITOR
                LeiaDisplay.Instance.tracker.SetProfilingEnabled(true);
#endif
            }
            else
            {
                if (!CursorWasPreviouslyVisible)
                {
                    Cursor.visible = false;
                }
                Cursor.lockState = PreviousCurorLockMode;
#if UNITY_ANDROID && !UNITY_EDITOR
                LeiaDisplay.Instance.tracker.SetProfilingEnabled(false);
#endif
            }

            SettingsPanelToggled();
        }
        private void OnXClick()
        {
            leiaConfigSettingsPanel.SetActive(false);
            SettingsPanelToggled();
        }
    }
}
