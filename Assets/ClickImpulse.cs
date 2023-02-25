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
    private Vector3 click_impulse_point;

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
        // record the point where the user clicked using a raycast from screenspace to worldspace
        // note: we don't use m_raycaster or NRInput for this, but the built-in Unity raycast and mouse position
        Ray ray = PoweredOn.Managers.GameManager.Instance.TargetWorldCam.ScreenPointToRay(Input.mousePosition + new Vector3(0,0,0.001f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            click_impulse_point = hit.point;
        }

        if(NRInput.GetAvailableControllersCount() > 0){
            // if we ARE using nreal mode, DO use it's laser pointer to get the point where the user clicked
            RaycastResult info = m_Raycaster.FirstRaycastResult();
            click_impulse_point = info.worldPosition;
        }
    }

    void FixedUpdate()
    {
        // apply force when the object is clicked
        if (isClicked)
        {
            if(rigidBody == null)
            {
                return;
            }
            //rigidBody.AddForce(Vector3.forward * click_impulse_force, ForceMode.Impulse);

            // the previous code was applying the force at the center of the card, let's try new code that applies the force based on where the user clicked:
            rigidBody.AddForceAtPosition(Vector3.forward * click_impulse_force, click_impulse_point, ForceMode.Impulse);

            isClicked = false;
        }
    }
}
