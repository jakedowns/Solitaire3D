using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NRKernal;

public class FollowNRPointerAsHand : MonoBehaviour
{
    /// <summary> The raycaster. </summary>
    [SerializeField]
    private NRPointerRaycaster m_Raycaster;

    public float defaultDistance = 0.5f;
    private float FollowSpeed = 5.0f;
    public float currentDistance = 0.5f;
    private ControllerHandEnum m_CurrentDebugHand;
    private Vector2 previousTouch;
    public float currentRotationAngle = 0.0f;
    public float HAND_ROTATE_ANGLE_INCREMENT = 0.5f;
    public float DISTANCE_CONTROL_INCREMENT = 0.1f;
    private GameObject m_PlayPlaneOffset;
    private Text m_PlayfieldButtonLabel;
    private GameObject m_PlayfieldButton;
    // toggles distance control between hand and playfield
    private bool m_PlayfieldSelected = false;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayPlaneOffset = GameObject.Find("PlayPlaneOffset");
        if(m_PlayPlaneOffset == null )
        {
            Debug.LogError("PlayPlaneOffset not found");
        }
        
        m_PlayfieldButton = GameObject.Find("PlayfieldButton");
        if (m_PlayfieldButton == null)
        {
            Debug.LogError("PlayfieldButton not found");
        }

        m_PlayfieldButtonLabel = m_PlayfieldButton.transform.Find("Text").GetComponent<Text>();
        
        defaultDistance = Mathf.Clamp(defaultDistance, m_Raycaster.NearDistance, m_Raycaster.FarDistance);
        currentDistance = defaultDistance;
    }

    void Update()
    {
        //RaycastResult info = m_Raycaster.FirstRaycastResult();

        float t = Time.deltaTime * FollowSpeed;
        transform.position = Vector3.Lerp(
            transform.position,
            transform.InverseTransformPoint(
                m_Raycaster.transform.position + m_Raycaster.transform.forward * currentDistance
            ), 
        t);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            NRInput.GetRotation(),
            t * FollowSpeed
        );

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

        if (NRInput.GetButtonDown(m_CurrentDebugHand, ControllerButton.TRIGGER))
            OnTrackpadDown();

        if (NRInput.GetButtonUp(m_CurrentDebugHand, ControllerButton.TRIGGER))
            OnTrackpadUp();

        Vector2 touchPos = NRInput.GetTouch(m_CurrentDebugHand);
        if (touchPos != Vector2.zero)
        {
            OnTrackpad(touchPos);
        }

    }

    public void ToggleSelectPlayfield()
    {
        m_PlayfieldSelected = !m_PlayfieldSelected;
        m_PlayfieldButtonLabel.text = m_PlayfieldSelected ? "Playfield Selected" : "Hand Selected";
    }

    void OnTrackpad(Vector2 touchPos){
        float deltaY = touchPos.y - previousTouch.y;
        float deltaX = touchPos.x - previousTouch.x;
        if (Mathf.Abs(deltaY) > 0.01f)
        {
            if (m_PlayfieldSelected)
            {
                Vector3 positionNext = new Vector3(
                    m_PlayPlaneOffset.transform.position.x,
                    m_PlayPlaneOffset.transform.position.y,
                    m_PlayPlaneOffset.transform.position.z + (deltaY * DISTANCE_CONTROL_INCREMENT)
                );
                m_PlayPlaneOffset.transform.position = positionNext;
            }
            else
            {
                // hand distance
                currentDistance += deltaY * DISTANCE_CONTROL_INCREMENT;
            }
        }

        if(Mathf.Abs(deltaX) > 0.01f)
        {
            if (!m_PlayfieldSelected)
                transform.Rotate(new Vector3(0.0f, deltaX * HAND_ROTATE_ANGLE_INCREMENT, 0.0f));
            else
                m_PlayPlaneOffset.transform.Rotate(new Vector3(deltaX * HAND_ROTATE_ANGLE_INCREMENT, 0.0f, 0.0f));
        }
        

        previousTouch = touchPos;
    }

    void OnTrackpadDown()
    {
        
    }

    void OnTrackpadUp()
    {
        
    }

}
