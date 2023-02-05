using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PoweredOn.Managers;

namespace PoweredOn
{
    public class DebugOutput : MonoBehaviour
    {
        public static DebugOutput Instance { get; private set; }
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
                if (GameManager.Instance != null && GameManager.Instance.game != null)
                {
                    m_TextComponent.text = GameManager.Instance.game.GetDebugText();
                }
                else
                {
                    Debug.LogWarning("game manager instance game missing");
                }
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
            //Debug.LogWarning(message);

            AddToGuiLogOutput(message);
        }

        public void LogError(string message)
        {
            Debug.LogError(message);

            AddToGuiLogOutput(message);
        }
    }

}