using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class PeelOffsetLabel : MonoBehaviour
{
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Peel Offset: "+(LeiaDisplay.Instance.getPeelOffsetForShader())
            + "\nDisplay Offset: "+LeiaDisplay.Instance.displayOffset
            + "\nNo: "+LeiaDisplay.Instance.No
            + "\nnumViews: "+LeiaDisplay.Instance.numViews
            + "\nFaceX: "+LeiaDisplay.Instance.faceX
            + "\nFaceY: "+LeiaDisplay.Instance.faceY
            + "\nFaceZ: "+LeiaDisplay.Instance.faceZ
            ;
    }
}
