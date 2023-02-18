using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointTestObjConfigurable : MonoBehaviour
{
    bool clicked = false;

    GameObject currentAnchor = null;
    GameObject anchorA;
    GameObject anchorB;

    ConfigurableJoint joint;

    // Start is called before the first frame update
    void Start()
    {
        anchorA = GameObject.Find("AnchorA");
        anchorB = GameObject.Find("AnchorB");
        currentAnchor = anchorA;

        joint = this.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = currentAnchor.GetComponent<Rigidbody>();

        // Set the anchor position
        joint.anchor = new Vector3(0, 0.9f, 1.92f);

        // Set the primary axis
        joint.axis = new Vector3(1, 0, 0);

        // Set the secondary axis
        joint.secondaryAxis = new Vector3(0, 1, 0);

        // Set the motion
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        // Set the connected anchor position
        joint.connectedAnchor = new Vector3(0, 0, 0);

        // Set the linear limit
        SoftJointLimit linearLimit = new SoftJointLimit();
        linearLimit.limit = 0.01f;
        linearLimit.bounciness = 0.01f;
        linearLimit.contactDistance = 0f;
        joint.linearLimit = linearLimit;

        // Set the linear limit spring
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 10000f;
        linearLimitSpring.damper = 1000f;
        joint.linearLimitSpring = linearLimitSpring;

        // Set the low and high angular X limit
        SoftJointLimit lowAngularXLimit = new SoftJointLimit();
        lowAngularXLimit.limit = 0f;
        lowAngularXLimit.bounciness = 0f;
        lowAngularXLimit.contactDistance = 0f;

        SoftJointLimit highAngularXLimit = new SoftJointLimit();
        highAngularXLimit.limit = 0f;
        highAngularXLimit.bounciness = 0f;
        highAngularXLimit.contactDistance = 0f;

        joint.lowAngularXLimit = lowAngularXLimit;
        joint.highAngularXLimit = highAngularXLimit;

        // Set the angular X limit spring
        // JointSpring angularXLimitSpring = new JointSpring();
        // angularXLimitSpring.spring = 100f;
        // angularXLimitSpring.damper = 100f;
        // joint.angularXLimitSpring = angularXLimitSpring;

        // Set the angular Y and Z limit
        SoftJointLimit angularYLimit = new SoftJointLimit();
        angularYLimit.limit = 177f;
        angularYLimit.bounciness = 0f;
        angularYLimit.contactDistance = 0f;

        SoftJointLimit angularZLimit = new SoftJointLimit();
        angularZLimit.limit = 3f;
        angularZLimit.bounciness = 0f;
        angularZLimit.contactDistance = 0f;

        joint.angularYLimit = angularYLimit;
        joint.angularZLimit = angularZLimit;

        // Set the angular YZ limit spring
        // JointSpring angularYZLimitSpring = new JointSpring();
        // angularYZLimitSpring.spring = 100f;
        // angularYZLimitSpring.damper = 100f;
        // joint.angularYZLimitSpring = angularYZLimitSpring;

        // Set the target position
        joint.targetPosition = new Vector3(0, 0, 0);

        // Set the target velocity
        joint.targetVelocity = new Vector3(0, 0, 0);

        // Set the X-drive parameters
        SoftJointLimitSpring xDriveSpring = new SoftJointLimitSpring();
        xDriveSpring.spring = 1000000f;
        xDriveSpring.damper = 1000f;
        JointDrive xDrive = new JointDrive();
        xDrive.positionSpring = xDriveSpring.spring;
        xDrive.positionDamper = xDriveSpring.damper;
        xDrive.maximumForce = 25f;
        xDrive.mode = JointDriveMode.Position;
        joint.xDrive = xDrive;

        // Set the Y-drive parameters
        SoftJointLimitSpring yDriveSpring = new SoftJointLimitSpring();
        yDriveSpring.spring = 1000f;
        yDriveSpring.damper = 100f;
        JointDrive yDrive = new JointDrive();
        yDrive.positionSpring = yDriveSpring.spring;
        yDrive.positionDamper = yDriveSpring.damper;
        yDrive.maximumForce = 2f;
        yDrive.mode = JointDriveMode.Position;
        joint.yDrive = yDrive;

        // Set the Z-drive parameters
        SoftJointLimitSpring zDriveSpring = new SoftJointLimitSpring();
        zDriveSpring.spring = 1000f;
        zDriveSpring.damper = 0f;
        JointDrive zDrive = new JointDrive();
        zDrive.positionSpring = zDriveSpring.spring;
        zDrive.positionDamper = zDriveSpring.damper;
        zDrive.maximumForce = 2f;
        zDrive.mode = JointDriveMode.Position;
        joint.zDrive = zDrive;

        // Set the target rotation and angular velocity
        joint.targetRotation = Quaternion.identity;
        joint.targetAngularVelocity = new Vector3(0, 0, 0);

        // Set the rotation drive mode and parameters
        joint.rotationDriveMode = RotationDriveMode.Slerp;
        SoftJointLimitSpring angularXDriveSpring = new SoftJointLimitSpring();
        angularXDriveSpring.spring = 0f;
        angularXDriveSpring.damper = 0f;
        JointDrive angularXDrive = new JointDrive();
        angularXDrive.positionSpring = angularXDriveSpring.spring;
        angularXDrive.positionDamper = angularXDriveSpring.damper;
        angularXDrive.maximumForce = float.MaxValue;
        angularXDrive.mode = JointDriveMode.Position;
        joint.angularXDrive = angularXDrive;
        SoftJointLimitSpring angularYZDriveSpring = new SoftJointLimitSpring();
        angularYZDriveSpring.spring = 0f;
        angularYZDriveSpring.damper = 0f;
        JointDrive angularYZDrive = new JointDrive();
        angularYZDrive.positionSpring = angularYZDriveSpring.spring;
        angularYZDrive.positionDamper = angularYZDriveSpring.damper;
        angularYZDrive.maximumForce = float.MaxValue;
        angularYZDrive.mode = JointDriveMode.Position;
        joint.angularYZDrive = angularYZDrive;

        // Enable the joint
        joint.enableCollision = true;
        joint.enablePreprocessing = true;
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown(){
        if(!this.clicked){
            this.clicked = true;
            //this.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, .01f));

            currentAnchor = currentAnchor == anchorA ? anchorB : anchorA;

            joint.connectedBody = currentAnchor.GetComponent<Rigidbody>();
        }
    }

    void OnMouseUp(){
        this.clicked = false;
    }
}
