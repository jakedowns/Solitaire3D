using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PoweredOn.Managers;
using PoweredOn.Animations.Effects;

namespace PoweredOn
{
    public class DebugOutput : MonoBehaviour
    {
        [SerializeField]
        public RippleOptions rippleOptions = new();

        public float ripple_speed = 0.1f;
        public int ripple_numRipples = 1; //3f;
        public float ripple_amplitude = 0.5f;
        public float ripple_frequency = 0.01f;
        public float ripple_decay = 0.01f;
        public float ripple_wavelength = 0.01f;
        public float ripple_delayBetween = 0.2f;
        public float ripple_debug_duration = 0.1f;
        public float ripple_debug_factor = 1.0f;
        public bool ripple_log = false;
        public bool ripple_clear_before = false;
        public bool ripple_on_click = true;
        public bool ripple_on_land = true;
        public float ripple_delay_before_placement_ripple = 0.7f;
        public bool ripple_draw_debug_gizmos = false;

        public float click_impulse_force = 0.5f;

        private static DebugOutput _instance;
        public static DebugOutput Instance { 
            get {
                return _instance ?? GameObject.FindObjectOfType<DebugOutput>();
            } 
            private set { _instance = value; }
        }

        GameManager gameManagerInstance;
        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            gameManagerInstance = GameManager.Instance;
            /*if(gameManagerInstance == null){
                gameManagerInstance = GameObject.FindObjectOfType<GameManager>();
            }*/
        }
        private float timer = 0f;

        Text m_TextComponent;
        List<string> logMessages = new List<string>();

        // Start is called before the first frame update
        void Start()
        {
            //logMessages = new List<string>();
            m_TextComponent = transform.GetComponent<Text>();
            if(m_TextComponent == null)
            {
                Debug.LogWarning("text component not found");
            }
        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                // code to be executed once per second
                timer = 0f;
                
                m_TextComponent.text = gameManagerInstance.game.GetDebugText();

                m_TextComponent.text += "\n";
                //foreach (string message in logMessages)
                for (int i = logMessages.Count - 1; i >= 0; i--)
                {
                    m_TextComponent.text += logMessages[i] + "\n";
                }
            }
        }

        void AddToGuiLogOutput(string message)
        {
            logMessages.Add(message);

            if (logMessages.Count > 10)
            {
                logMessages.RemoveAt(0);
            }
        }

        public void ToggleLogVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void ClearLog()
        {
            logMessages.Clear();
        }

        public void Log(string message)
        {
            Debug.Log(message);
            AddToGuiLogOutput(message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(message);

            AddToGuiLogOutput(message);
        }

        public void LogError(string message)
        {
            Debug.LogError(message);

            AddToGuiLogOutput(message);
        }
    }

}