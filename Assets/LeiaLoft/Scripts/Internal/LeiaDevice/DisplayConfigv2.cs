using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DisplayConfigv2
{
    public ActCoefficients actCoefficients;
    public AntiAliasing antiAliasing;
    public BacklightRatio2D backlightRatio2D;
    public ColorCorrection colorCorrection;
    public ConfigInfo configInfo;
    public DisplayGeometry displayGeometry;
    public NumViews numViews;
    public ScreenHeight screenHeight;
    public ScreenWidth screenWidth;
    public SystemDisparity systemDisparity;
    public TileHeight tileHeight;
    public TileWidth tileWidth;
    public TrackingCamera trackingCamera;
    public Rendering rendering;

    public DisplayConfigv2()
    {
        ActCoefficients actCoefficients = new ActCoefficients();
        AntiAliasing antiAliasing = new AntiAliasing();
        BacklightRatio2D backlightRatio2D = new BacklightRatio2D();
        ColorCorrection colorCorrection = new ColorCorrection();
        ConfigInfo configInfo = new ConfigInfo();
        DisplayGeometry displayGeometry = new DisplayGeometry();
        NumViews numViews = new NumViews();
        ScreenHeight screenHeight = new ScreenHeight();
        ScreenWidth screenWidth = new ScreenWidth();
        SystemDisparity systemDisparity = new SystemDisparity();
        TileHeight tileHeight = new TileHeight();
        TileWidth tileWidth = new TileWidth();
        TrackingCamera trackingCamera = new TrackingCamera();
        Rendering rendering = new Rendering();
    }

    public string SaveToString()
    {
        string serializedData = JsonUtility.ToJson(actCoefficients, true);
        return (serializedData);
    }
}

[System.Serializable]
public class ActCoefficients
{
    public string description = "Anti-Cross-Talk (ACT) coefficients";
    public float gamma = 1.99f;
    public float[] value = { 0.1f, 0.02f, 0.01f, 0.01f };
    public int beta = 2;

    public ActCoefficients()
    {

    }

    public void Set(float x, float y, float z, float w)
    {
        value[0] = x;
        value[1] = y;
        value[2] = w;
        value[3] = z;
    }
}

[System.Serializable]
public class AntiAliasing
{
    public string description = "Anti-aliasing kernel for interlacing tiles at suggested tile size";
    Vector2[] coordinates = {
        new Vector2(0.45f, 0.15f),
        new Vector2(-0.45f, -0.15f)
    };
    Vector2 weights = new Vector2(0.5f, 0.5f);
}

[System.Serializable]
public class BacklightRatio2D
{
    public string description = "3D backlight component in 2D mode";
    public float value = 0.11f;
}

[System.Serializable]
public class ColorCorrection
{
    public string description = "color correction params for 2D and 3D modes TBD";
}

[System.Serializable]
public class ConfigInfo
{
    public string versionNum = "2.0";
    public string lastUpdated = "2022-01-30-23:46:12";
}

[System.Serializable]
public class DisplayInfo
{
    string displayClass = "LP2-EVT1";
    string displaySN = "";
    string displayMfgDate = "2021W52";
}

[System.Serializable]
public class DisplayGeometry
{
    public float centerView = 4.2f;
    bool colorInversion = false;
    public float colorSlant = 0f;
    public float d_over_n = 0.46f;
    public string description = "Display Properties";
    public float n = 1.6f;
    public float p_over_du = 3f;
    public float p_over_dv = 1f;
    public float pixelPitch = 0.1f;
    public float s = 7.1f;
    public float theta = -0.33f;
}

[System.Serializable]
public class NumViews
{
    public string description = "Number of horizontal views";
    public float value = 9;
}

[System.Serializable]
public class ScreenHeight
{
    public string description = "Height of display";
    public float value = 9;
}

[System.Serializable]
public class ScreenWidth
{
    public string description = "Width of display";
    public float value = 9;
}

[System.Serializable]
public class SystemDisparity
{
    public string description = "recommended disparity level, referencing H1/LP value of 8";
    public float value = 4;
}

[System.Serializable]
public class TileHeight
{
    public string description = "Suggested height of rendering tile";
    public float value = 600;
}

[System.Serializable]
public class TileWidth
{
    public string description = "Suggested width of rendering tile";
    public float value = 960;
}

[System.Serializable]
public class TrackingCamera
{
    public float[] cameraT = { -8.88f, 91.6f, 0f };
    public float[] cameraR = { -0.2f, 0.49f, 0 };
    public float[] predictParams = {
            0.02f,
            0.04f,
            0.19f,
            0.1f,
            8.0f
    };
    public float width = 640;
    public float height = 480;
    public float fps = 90;
    public float focalLength = 0.1961f;
    public string description = "Extrinsic params for tracking camera, Rodrigues notation";
}

[System.Serializable]
public class Rendering
{
    public float fps = 60;
}
