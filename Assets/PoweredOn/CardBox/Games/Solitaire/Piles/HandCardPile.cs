using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;

namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class HandCardPile : PlayingCardPile
    {
        public HandCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new HandCardPile Clone()
        {
            return new HandCardPile(cardList.Clone());
        }
    }
}
