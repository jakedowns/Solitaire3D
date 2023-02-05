using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class HandCardPile : SolitaireCardPile
    {

        private new SolitaireGameObject gameObjectType = SolitaireGameObject.Hand_Base;

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
    }
}
