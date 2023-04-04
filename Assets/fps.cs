using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fps : MonoBehaviour
{
    public float updateInterval = 0.5f; // interval to update FPS in seconds
    public int rollingAverageCount = 10; // number of frames to use for rolling average

    private Text text;
    private float[] fpsBuffer;
    private int fpsBufferIndex;
    private float minFPS = Mathf.Infinity;
    private float maxFPS = 0.0f;

    void Start()
    {
        text = gameObject.GetComponent<Text>();
        fpsBuffer = new float[rollingAverageCount];
        fpsBufferIndex = 0;
    }

    void Update()
    {
        // update FPS buffer
        fpsBuffer[fpsBufferIndex] = 1.0f / Time.deltaTime;
        fpsBufferIndex = (fpsBufferIndex + 1) % rollingAverageCount;

        // calculate rolling average FPS
        float sum = 0.0f;
        for (int i = 0; i < rollingAverageCount; i++)
        {
            sum += fpsBuffer[i];
        }
        float fps = sum / rollingAverageCount;

        // update minimum and maximum FPS
        if (fps < minFPS)
        {
            minFPS = fps;
        }
        if (fps > maxFPS)
        {
            maxFPS = fps;
        }

        // update FPS text
        text.text = "FPS: " + fps.ToString("0.0") + " (min: " + minFPS.ToString("0.0") + ", max: " + maxFPS.ToString("0.0") + ")";
    }
}
