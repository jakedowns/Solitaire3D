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
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
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
            if (isTouching && !didLongTouch && Time.time - touchedAt > longTouchDuration)
            {
                didLongTouch = true;
                Vector2 touchPos = Input.GetMouseButton(0) ? (Vector2)Input.mousePosition : Input.GetTouch(0).position;
                TestDidHit(touchPos, true);
            }
            else
            {
                Vector2 touchPos = Input.GetMouseButton(0) ? (Vector2)Input.mousePosition : Input.GetTouch(0).position;
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
        }
    }

    Ray ray;
    RaycastHit hitData;
    MonoSolitaireCard monoCard;
    MonoSolitaireCardPileBase pileBase;
    SolitaireCard card;
    GameManager gameManager
    {
        get
        {
            return PoweredOn.Managers.GameManager.Instance ?? FindObjectOfType<GameManager>();
        }
    }

    void TestDidHit(Vector2 touchPosition, bool longTouch = false)
    {
        //Debug.Log($"touchPosition {touchPosition}");
        ray = mainCamera.ScreenPointToRay(touchPosition);
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
                        gameManager.game.OnLongPressCard(card);
                    }
                    else
                    {
                        gameManager.OnSingleClickCard(card);

                    }
                }
                else if(touchedObject.tag == "PileBase")
                {
                    pileBase = touchedObject.GetComponent<MonoSolitaireCardPileBase>();
                    gameManager.game.OnSingleClickCardPileBase(pileBase);
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
    }
}