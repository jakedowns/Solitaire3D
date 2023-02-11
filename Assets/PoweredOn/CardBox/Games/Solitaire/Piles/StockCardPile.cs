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
    public class StockCardPile : SolitaireCardPile
    {

        public new SolitaireGameObject gameObjectType = SolitaireGameObject.Stock_Base;
        public StockCardPile() { }
        public StockCardPile(PlayingCardIDList idList) :base(idList) { }

        public new static StockCardPile EMPTY
        {
            get { return new StockCardPile(); }
        }

        public new StockCardPile Clone()
        {
            return new StockCardPile(cardList.Clone());
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
                return GameManager.Instance.game.GetGameObjectByType(gameObjectType);
            }
        }

        public bool CanReceiveCard(SolitaireCard card)
        {
            if (card.previousPlayfieldSpot.area == PlayfieldArea.WASTE || card.previousPlayfieldSpot.area == PlayfieldArea.DECK)
            {
                return true; // always valid from waste or deck
            }
            
            /* TODO: add undo/redo support:
            if (
              PoweredOn.Managers.GameManager.Instance.game.IsUndoingMove
              || PoweredOn.Managers.GameManager.Instance.game.IsRedoingMove
            )
            {
                return true;
            }*/

            return false;
        }
    }
}
