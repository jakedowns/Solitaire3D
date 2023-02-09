using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling;
using UnityEngine;

public class TouchInputMode : MonoBehaviour
{
    bool isTouching = false;
    Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        /* using mouse input; raycast from screenspace to world space and see if we've hit a card */
        if (Input.GetMouseButtonDown(0))
        {
            if (!isTouching)
            {
                isTouching = true;
                TestDidHit(Input.mousePosition);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            if (isTouching)
            {
                isTouching = false;
            }
        }

        /* using touch input; raycast from screenspace to world space and see if we've hit a card */
        if (Input.touchCount > 0)
        {
            if (!isTouching)
            {
                isTouching = true;

                Touch touch = Input.GetTouch(0);

                TestDidHit(touch.position);
            }
        }
        else
        {
            if (isTouching)
            {
                isTouching = false;
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

    void TestDidHit(Vector2 touchPosition)
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
                    gameManager.OnSingleClickCard(card);
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
