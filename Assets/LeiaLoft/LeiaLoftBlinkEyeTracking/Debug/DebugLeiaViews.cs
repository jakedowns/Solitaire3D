using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeiaLoft;

public class DebugLeiaViews : MonoBehaviour
{
    public List<GameObject> debugCameras;
    LeiaVirtualDisplay leiaVirtualDisplay;
    Transform debugCameraPrefab;

    void Start()
    {
        debugCameraPrefab = Resources.Load<Transform>("DebugLeiaViewIcon");
        leiaVirtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();
        Invoke("InstantiateCameras", 1f);
    }

    void InstantiateCameras()
    {
        debugCameras = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name.Contains("LeiaView"))
            {
                Transform newDebugCamIcon = Instantiate(
                    debugCameraPrefab,
                    transform.GetChild(i).position,
                    Quaternion.identity,
                    transform.GetChild(i)
                    );

                debugCameras.Add(newDebugCamIcon.gameObject);

                newDebugCamIcon.GetComponentInChildren<TextMesh>().text = "" + i;
            }
        }
    }

    void Update()
    {
        if (debugCameras == null)
        {
            return;
        }

        if (LeiaDisplay.Instance.DesiredRenderTechnique == LeiaDisplay.RenderTechnique.Stereo)
        {
            for (int i = 0; i < debugCameras.Count; i++)
            {
                if (debugCameras[i] != null)
                {
                    //In stereo only enable the first two camera icons
                    debugCameras[i].SetActive(i < 4);
                }
            }
        }
        else
        {
            for (int i = 0; i < debugCameras.Count; i++)
            {
                if (debugCameras[i] != null)
                {
                    //In LF enable all camera icons
                    debugCameras[i].SetActive(true);
                }
            }
        }

        for (int i = 0; i < debugCameras.Count; i++)
        {
            if (debugCameras[i] != null)
            {
                debugCameras[i].transform.localScale = Vector3.one * leiaVirtualDisplay.Height / 15f;
            }
        }
    }
}
