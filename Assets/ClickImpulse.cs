using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class ClickImpulse : MonoBehaviour
{
    /// <summary> The raycaster. </summary>
    [SerializeField]
    private NRPointerRaycaster m_Raycaster;

    private bool isClicked = false;
    public float click_impulse_force = 3f;

    private ControllerHandEnum m_CurrentDebugHand;

    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (NRInput.GetAvailableControllersCount() < 2)
        {
            m_CurrentDebugHand = NRInput.DomainHand;
        }
        else
        {
            if (NRInput.GetButtonDown(ControllerHandEnum.Right, ControllerButton.TRIGGER))
            {
                m_CurrentDebugHand = ControllerHandEnum.Right;
            }
            else if (NRInput.GetButtonDown(ControllerHandEnum.Left, ControllerButton.TRIGGER))
            {
                m_CurrentDebugHand = ControllerHandEnum.Left;
            }
        }

        if (NRInput.GetButtonDown(m_CurrentDebugHand, ControllerButton.TRIGGER)){
            // check if the raycaster is hitting this object
            RaycastResult info = m_Raycaster.FirstRaycastResult();
            if (info.gameObject == gameObject){
                // if so, apply force
                OnMouseDown();
            }
        }
            

        // if (NRInput.GetButtonUp(m_CurrentDebugHand, ControllerButton.TRIGGER))
        //     OnMouseUp();
    }

    // void OnMouseUp()
    // {
    //     isClicked = false;
    // }

    void OnMouseDown()
    {
        isClicked = true;
    }

    void FixedUpdate()
    {
        // apply force when the object is clicked
        if (isClicked)
        {
            rigidBody.AddForce(Vector3.forward * click_impulse_force, ForceMode.Impulse);
            isClicked = false;
        }
    }
}
