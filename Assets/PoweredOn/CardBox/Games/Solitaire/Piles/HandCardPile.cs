using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class HandCardPile : SolitaireCardPile
    {

        public new const SolitaireGameObject gameObjectType = SolitaireGameObject.Hand_Base;

        public HandCardPile()
        {
            
        }
        
        public HandCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new static HandCardPile EMPTY
        {
            get { return new HandCardPile(); }
        }

        public new HandCardPile Clone()
        {
            return new HandCardPile(cardList.Clone());
        }

        public bool CanReceiveCard(SolitaireCard card)
        {
            // TODO: if undoing or redoing move; return true
            GameManager gmi = GameManager.Instance ?? GameObject.FindObjectOfType<GameManager>();


            if (Count > 0 && !gmi.game.IsPickingUpSubstack)
            {
                // block if we already have a card in our hand and we're not picking up a substack
                return false;
            }
            
            // cannot pick up cards from deck or hand
            if (
                card.playfieldSpot.area == PlayfieldArea.DECK
                || card.playfieldSpot.area == PlayfieldArea.HAND
            )
            {
                return false;
            }

            if (!card.IsFaceUp)
            {
                // block if the card is not face up
                return false;
            }
            
            return true;
        }
    }
}
