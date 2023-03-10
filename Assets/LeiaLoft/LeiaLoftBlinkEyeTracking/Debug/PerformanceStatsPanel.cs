using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PerformanceStatsPanel : MonoBehaviour
{
    public Text fpsLabel;
    public Text latencyLabel;
    public Text blinkProcessTimeLabel;

    private List<float> currentFPSList = new List<float>();
    private List<float> currentFPSMsList = new List<float>();
    private List<float> blinkProcessingList = new List<float>();
    private List<float> frameDelayList = new List<float>();

    private void Update()
    {
        UpdateTimer();
    }
    void UpdateTimer()
    {
        float fps = Mathf.Round(1.0f / Time.deltaTime);
        float latency = LeiaDisplay.Instance.tracker.GetFrameDelay();
        long blinkProcessTimeNS = -1;

#if UNITY_ANDROID
        blinkProcessTimeNS = (long)LeiaDisplay.Instance.tracker.GetBlinkProcessingTime();
        int averageBPTime = (int)getAverageFloat(blinkProcessTimeNS, blinkProcessingList);
#endif

        int averageLatency = (int)getAverageFloat(latency, frameDelayList);
        /// <remove_from_public>
#if UNITY_STANDALONE_WIN
        LeiaHeadTracking.Engine.Frame currentFrame;

        currentFrame = LeiaHeadTracking.Engine.GetCurrentFrame();
        if (currentFrame != null)
        {
            currentFrame.GetDetectionTimeNs(out blinkProcessTimeNS);
        }
        long blinkProcessTimeMS = blinkProcessTimeNS / 1000000000;
        float averageBPTime = getAverageFloat(blinkProcessTimeMS, blinkProcessingList);
#endif
        /// </remove_from_public>

        int averageFPSMs= (int)getAverageFloat(Time.deltaTime * 1000, currentFPSMsList);
        int averageFPS = (int)getAverageFloat(fps, currentFPSList);

        if (latency > 0)
        {
            latencyLabel.text = "Tracking Latency: " + averageLatency + " ms";
        }

        blinkProcessTimeLabel.text = "Blink Process Time: " + averageBPTime + " ms";
        fpsLabel.text = "Framerate: " + averageFPS + " fps (" + averageFPSMs + " ms)";
    }

    private float getAverageFloat(float newFloat, List<float> list)
    {
        list.Add(newFloat);
        if (list.Count > 60)
        {
            list.RemoveAt(0);
        }

        float total = 0f;
        foreach (float f in list)
        {
            total += f;
        }
        float average = total / (float)list.Count; 

        return average;
    }
}
