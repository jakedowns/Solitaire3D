using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class WasteCardPile : SolitaireCardPile
    {

        public WasteCardPile() { }

        public override SolitaireGameObject gameObjectType { get; set; } = SolitaireGameObject.Waste_Base;

        public WasteCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new static WasteCardPile EMPTY
        {
            get { return new WasteCardPile(); }
        }

        public new WasteCardPile Clone()
        {
            return new WasteCardPile(cardList.Clone());
        }

        public bool CanRecieveCard(SolitaireCard card)
        {
            if(card.playfieldSpot.area == PlayfieldArea.STOCK)
            {
                return true; // always valid from stock
            }
            
            /*
            if (
              PoweredOn.Managers.GameManager.Instance.game.IsUndoingMove
              || PoweredOn.Managers.GameManager.Instance.game.IsRedoingMove
            )
            {
                return true;
            }*/
            
            // if they picked up the card, and they're returning it, it's valid
            if (
                card.playfieldSpot.area == PlayfieldArea.HAND
                && card.previousPlayfieldSpot.area == PlayfieldArea.WASTE 
                && card.previousPlayfieldSpot.index == Count-1
            ) {
                return true;
            }
            return false;
        }

        // i hate that i have to override this, and that the base implementation can't read my overridden gameObjectType here.
        // i'm obviously doing something wrong because the whole point of inheritence is to not repeat code like this.
        // i'm guessing i need like a virtual method or something? or an interface? i dunno
        public new GameObject gameObject
        {
            get
            {
                if (this.gameObjectType == SolitaireGameObject.None)
                {
                    throw new Exception($"{this.GetType().Name} class does not have a proper gameObjectType defined");
                }
                return gmi.game.GetGameObjectByType(gameObjectType);
            }
        }
    }
}
