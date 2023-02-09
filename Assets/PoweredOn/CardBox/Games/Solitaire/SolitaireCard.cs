using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.CardBox.PlayingCards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireCard : PlayingCard
    {

        public MonoSolitaireCard monoCard;
        public SolitaireCard(Suit suit, Rank rank, int deckOrder) : base(suit, rank, deckOrder)
        {
            string gameObjectTypeName = "Card_" + suit.ToString().ToLower() + "_of_" + rank.ToString().ToLower();
            this.gameObjectType = Enum.TryParse(gameObjectTypeName, out SolitaireGameObject gameObjectType) ? gameObjectType : SolitaireGameObject.None;
        }

        // listen for message "OnMouseDown"
        public void OnMouseDown(){
            Debug.Log("OnMouseDown");
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("OnMouseDown: IsPointerOverGameObject");
                return;
            }
            Debug.Log("OnMouseDown: Not IsPointerOverGameObject");
            Managers.GameManager.Instance.OnSingleClickCard(this);
        }

        public SolitaireGameObject gameObjectType;

        /*public new GameObject gameObject {
            get {
                return base.gameObject;
                return Managers.GameManager.Instance.game.GetGameObjectByType(this.gameObjectType);
            }
        }*/

        public static SolitaireCard AceOfSpades
        {
            get {
                return new SolitaireCard(Suit.SPADES, Rank.ACE, 0);
            }
        }

        public static SolitaireCard AceOfHearts
        {
            get {
                return new SolitaireCard(Suit.HEARTS, Rank.ACE, 0);
            }
        }

        public FoundationCardPile GetFoundationCardPile()
        {
            return Managers.GameManager.Instance.game.GetFoundationCardPileForSuit(this.GetSuit());
        }

        public SolitaireCard(PlayingCard card): base(card.GetSuit(), card.GetRank(), card.GetDeckOrder()){
        }

        public void SetMonoCard(MonoSolitaireCard monoCard)
        {
            this.monoCard = monoCard;
            this.selfGameObject = monoCard.gameObject;
        }
        public void SetPosition(Vector3 position)
        {
            this.monoCard.gameObject.transform.localPosition = position;
        }

        public void UpdateGoalIDScale(Vector3 newScale)
        {
            this.goalIdentity.scale = newScale;   
        }
        public void SetRotation(Quaternion rotation)
        {
            this.monoCard.gameObject.transform.rotation = rotation;
        }

        public static SolitaireCard NONE
        {
            get
            {
                return new SolitaireCard(Suit.NONE, Rank.NONE, -1);
            }
        }
    }
}
