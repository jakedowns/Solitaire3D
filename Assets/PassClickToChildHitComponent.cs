using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using PoweredOn.CardBox.Games.Solitaire;

public class PassClickToChildHitComponent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    MonoSolitaireCardPileBase monoSolitaireCardPileBase;

    // Start is called before the first frame update
    void Start()
    {
        monoSolitaireCardPileBase = GetComponent<MonoSolitaireCardPileBase>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.LogWarning("[PassClickToChildHitComponent]@OnPointerClick");
        monoSolitaireCardPileBase.OnPointerClick(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.LogWarning("[PassClickToChildHitComponent]@OnPointerEnter");
        monoSolitaireCardPileBase.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.LogWarning("[PassClickToChildHitComponent]@OnPointerExit");
        monoSolitaireCardPileBase.OnPointerExit(eventData);
    }


}
