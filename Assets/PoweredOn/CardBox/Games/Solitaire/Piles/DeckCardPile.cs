using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class DeckCardPile : SolitaireCardPile
    {
        public new const SolitaireGameObject gameObjectType = SolitaireGameObject.Deck_Base;
        public DeckCardPile() { }
        public DeckCardPile(DeckCardPile pile) {
            cardList = pile.cardList.Clone();
        }
        public DeckCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new static DeckCardPile EMPTY
        {
            get { return new DeckCardPile(); }
        }

        public new DeckCardPile Clone()
        {
            return new DeckCardPile(cardList.Clone());
        }

        // why do i need to extend this to get the gameObjectType reference to point to the extended value and not the base value???
        public new GameObject gameObject
        {
            get
            {
                if (gameObjectType == SolitaireGameObject.None)
                {
                    throw new Exception($"{this.GetType().Name} class does not have a proper gameObjectType defined");
                }
                return gmi.game.GetGameObjectByType(gameObjectType);
            }
        }

        public bool CanReceiveCard(SolitaireCard card)
        {
            // TODO: if undoing or redoing move; return true

            if ((GameManager.Instance ?? GameObject.FindObjectOfType<GameManager>()).game.deck.IsCollectingCardsToDeck)
            {
                return true;
            }

            return false;
        }
    }
}
