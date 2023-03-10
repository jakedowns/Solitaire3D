using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour
{
    RenderTexture graphTexture;
    [SerializeField] private string GraphName;
    [SerializeField] private ComputeShader graphComputeShader;
    float[] dataFaceZ;
    float[] dataFOV;
    float[] dataConvergence;
    float[] dataCameraShiftZ;
    int addPosition = 0;

    BlinkTrackingUnityPlugin blink;

    int kernelID;

    float maxValueFaceZ;
    float maxValueFOV;
    float maxValueConvergence;
    float maxValueCameraShiftZ;

    float minValueFaceZ;
    float minValueFOV;
    float minValueConvergence;
    float minValueCameraShiftZ;

    [SerializeField] private Text label;

    public enum Data { FaceZ, FOV, Convergence, CameraShiftZ };
    Data dataSelected = Data.FaceZ;

    public void OnDropdownValueChanged(int newVal)
    {
        dataSelected = (Data)newVal;
        Debug.Log("Data selected: " + dataSelected.ToString());
    }

    void Start()
    {
        graphComputeShader = Resources.Load<ComputeShader>("Graph");

        kernelID = graphComputeShader.FindKernel(
                "CSMain"
            );

        dataFaceZ = new float[2000];
        dataFOV = new float[2000];
        dataConvergence = new float[2000];
        dataCameraShiftZ = new float[2000];
        blink = FindObjectOfType<BlinkTrackingUnityPlugin>();
        graphTexture = new RenderTexture(Screen.width, 200, 16, RenderTextureFormat.ARGB32);
        graphTexture.name = "DepthRenderTexture";
        graphTexture.antiAliasing = 1;
        graphTexture.autoGenerateMips = false;
        graphTexture.filterMode = FilterMode.Point;
        graphTexture.anisoLevel = 0;
        graphTexture.enableRandomWrite = true;
        graphTexture.Create();

        RawImage rawImage = GetComponent<RawImage>();
        rawImage.texture = graphTexture;
    }

    void Update()
    {
        float maxValue = 0;
        float minValue = 0;
        float currentValue = 0;

        switch (dataSelected)
        {
            case Data.FaceZ:
                graphComputeShader.SetFloats("data", dataFaceZ);
                maxValue = maxValueFaceZ;
                minValue = minValueFaceZ;
                currentValue = blink.faceZ;
                break;
            case Data.CameraShiftZ:
                graphComputeShader.SetFloats("data", dataCameraShiftZ);
                maxValue = maxValueCameraShiftZ;
                minValue = minValueCameraShiftZ;
                currentValue = LeiaCamera.Instance.CameraShift.z;
                break;
            case Data.Convergence:
                graphComputeShader.SetFloats("data", dataConvergence);
                maxValue = maxValueConvergence;
                minValue = minValueConvergence;
                currentValue = LeiaCamera.Instance.ConvergenceDistance;
                break;
            case Data.FOV:
                graphComputeShader.SetFloats("data", dataFOV);
                maxValue = maxValueFOV;
                minValue = minValueFOV;
                currentValue = LeiaCamera.Instance.FieldOfView;
                break;
        }

        graphComputeShader.SetFloat("maxValue", maxValue);
        graphComputeShader.SetFloat("minValue", minValue);

        label.text = dataSelected.ToString() + " maxValue = " + maxValue + " currentValue = " + currentValue;

        graphComputeShader.SetTexture(kernelID, "Result", graphTexture);
        //graphTexture.SetPixel(Time.frameCount % 200, (int)(Input.mousePosition.y / Screen.height * 100), Color.cyan);

        if (blink.faceZ > maxValueFaceZ)
        {
            maxValueFaceZ = blink.faceZ;
        }
        if (blink.faceZ < minValueFaceZ)
        {
            minValueFaceZ = blink.faceZ;
        }

        if (LeiaCamera.Instance.CameraShift.z > maxValueCameraShiftZ)
        {
            maxValueCameraShiftZ = LeiaCamera.Instance.CameraShift.z;
        }
        if (LeiaCamera.Instance.CameraShift.z < minValueCameraShiftZ)
        {
            minValueCameraShiftZ = LeiaCamera.Instance.CameraShift.z;
        }

        if (LeiaCamera.Instance.ConvergenceDistance > maxValueConvergence)
        {
            maxValueConvergence = LeiaCamera.Instance.ConvergenceDistance;
        }
        if (LeiaCamera.Instance.ConvergenceDistance < minValueConvergence)
        {
            minValueConvergence = LeiaCamera.Instance.ConvergenceDistance;
        }

        if (LeiaCamera.Instance.FieldOfView > maxValueFOV)
        {
            maxValueFOV = LeiaCamera.Instance.FieldOfView;
        }
        if (LeiaCamera.Instance.FieldOfView < minValueFOV)
        {
            minValueFOV = LeiaCamera.Instance.FieldOfView;
        }

        dataFaceZ[addPosition] = blink.faceZ;
        dataFOV[addPosition] = LeiaCamera.Instance.FieldOfView;
        dataConvergence[addPosition] = LeiaCamera.Instance.ConvergenceDistance;
        dataCameraShiftZ[addPosition] = LeiaCamera.Instance.CameraShift.z;

        addPosition++;
        if (addPosition == 2000)
        {
            addPosition = 0;
        }

        graphComputeShader.Dispatch(kernelID, 2000, 200, 1);
    }
}
