using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using UnityEngine;

public class TouchInputMode : MonoBehaviour
{
    bool isTouching = false;
    Camera mainCamera;
    const float longTouchDuration = 0.35f; // seconds
    float touchedAt = 0;
    bool didLongTouch = false;
    Vector2 lastTouchPosition;
    GameObject cameraParent;
    // Start is called before the first frame update
    void Start()
    {
        cameraParent = GameObject.Find("CameraParent");
        Camera[] findsCameras = Resources.FindObjectsOfTypeAll<Camera>();
        foreach (var _camera in findsCameras)
        {
            if (_camera.gameObject.name == "MainCamera")
            {
                mainCamera = _camera;
            }
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("main camera not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        /* TOUCH START */
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            isTouching = true;
            touchedAt = Time.time;
            didLongTouch = false;
        }

        /* MID-TOUCH */
        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            Vector2 touchPos = Input.GetMouseButton(0) ? (Vector2)Input.mousePosition : Input.GetTouch(0).position;
            if (previousTouchPosition != null)
            {
                // update camera parent rotation based on touch movement along x and y axis
                Vector2 delta = touchPos - (Vector2)previousTouchPosition;

                /* 
                   Note: this next section of code takes the delta x and y of the touch movement,
                   and converts it into yaw and pitch rotation values that are applied to the camera parent to pan and tilt the camera.
                   the values are reduced by a factor make the camera movement more subtle.
                */
                float yaw = delta.x / 500f;
                float pitch = delta.y / 500f;
                //updatedRotation *= Quaternion.Euler(pitch, yaw, 0);

                /* 
                   Note: the previous code had an issue where it wasn't rotating around world UP for yaw, it was rotating around local UP.
                   this next section of code fixes that by converting the camera parent's local up vector into world space, and using that as the axis of rotation.
                */
                // Quaternion updatedRotation = cameraParent.transform.rotation;
                // Vector3 worldUp = cameraParent.transform.TransformDirection(Vector3.up);
                // updatedRotation *= Quaternion.AngleAxis(yaw, worldUp);
                // /* Now we do the same for pitch along world right */
                // Vector3 worldRight = cameraParent.transform.TransformDirection(Vector3.right);
                // updatedRotation *= Quaternion.AngleAxis(-pitch, worldRight);
                // /* Finally, we apply the updated rotation to the camera parent */
                // cameraParent.transform.rotation = updatedRotation;

                /* actually, the above code is stillcausing issues, so we're going to try this instead: */
                cameraParent.transform.RotateAround(cameraParent.transform.position, Vector3.up, yaw);
                cameraParent.transform.RotateAround(cameraParent.transform.position, cameraParent.transform.right, -pitch);


            }

            if (isTouching && !didLongTouch && Time.time - touchedAt > longTouchDuration)
            {
                didLongTouch = true;
                
                TestDidHit(touchPos, true);
            }
            else
            {
                lastTouchPosition = touchPos;
            }
        }

        /* TOUCH END */
        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            isTouching = false;

            if (!didLongTouch)
            {
                Vector2 touchPos = Input.GetMouseButtonUp(0) ? (Vector2)Input.mousePosition : lastTouchPosition;
                TestDidHit(touchPos);
            }

            previousTouchPosition = null;
        }
    }

    Ray ray;
    RaycastHit hitData;
    MonoSolitaireCard monoCard;
    MonoSolitaireCardPileBase pileBase;
    SolitaireCard card;
#nullable enable
    Vector2? previousTouchPosition = null;

    void TestDidHit(Vector2 touchPosition, bool longTouch = false)
    {
        //Debug.Log($"touchPosition {touchPosition}");
        ray = mainCamera.ScreenPointToRay(touchPosition);

        // visualize the raycast
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 60f);

        if(Physics.Raycast(ray, out hitData, 1000))
        {
            if (hitData.collider != null)
            {
                GameObject touchedObject = hitData.transform.gameObject;
                if (touchedObject.tag == "Card")
                {
                    monoCard = touchedObject.GetComponent<MonoSolitaireCard>();
                    //Debug.Log($"monoCard: {monoCard}");
                    card = monoCard.GetCard();
                    //Debug.Log($"soliCard: {card}");
                    if (longTouch)
                    {
                        GameManager.Instance.game.OnLongPressCard(card);
                    }
                    else
                    {
                        GameManager.Instance.OnSingleClickCard(card);

                    }
                }
                else if(touchedObject.tag == "PileBase")
                {
                    pileBase = touchedObject.GetComponent<MonoSolitaireCardPileBase>();
                    GameManager.Instance.game.OnSingleClickCardPileBase(pileBase);
                }
                //Debug.Log("Touched " + touchedObject.name);
                //Debug.Log("touchedObject.tag " + touchedObject.tag);
            }
            else
            {

                Debug.Log($"no collider for target {hitData}");
            }
        }
        else
        {
            Debug.Log($"hit nothing");
        }

        previousTouchPosition = touchPosition;
    }
}
