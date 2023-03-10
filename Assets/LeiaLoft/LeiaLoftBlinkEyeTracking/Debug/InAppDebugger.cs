using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InAppDebugger : MonoBehaviour {

    List<string> logLines;
    [SerializeField] private Text text;
    
    void Awake () {
        logLines = new List<string>();
    }
	
    public void Log(string newLog) {
        logLines.Add(newLog);
        if (logLines.Count > 20) {
            logLines.RemoveAt(0);
        }
        text.text = "";
        for (int i = 0; i < logLines.Count; i++)
            text.text += "\n" + logLines[i];
        }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        Log(logString + "\n" + stackTrace);
    }
}