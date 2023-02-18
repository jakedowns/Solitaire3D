using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointTestObj : MonoBehaviour
{
    bool clicked = false;

    GameObject currentAnchor = null;
    GameObject anchorA;
    GameObject anchorB;

    SpringJoint joint;

    // Start is called before the first frame update
    void Start()
    {
        anchorA = GameObject.Find("AnchorA");
        anchorB = GameObject.Find("AnchorB");
        currentAnchor = anchorA;

        joint = this.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = currentAnchor.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown(){
        if(!this.clicked){
            this.clicked = true;
            this.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, .01f));

            currentAnchor = currentAnchor == anchorA ? anchorB : anchorA;

            joint.connectedBody = currentAnchor.GetComponent<Rigidbody>();
        }
    }

    void OnMouseUp(){
        this.clicked = false;
    }
}
