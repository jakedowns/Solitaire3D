using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class FoundationCardPile : PlayingCardPile
    {
        private int _index;

        public FoundationCardPile(int index)
        {
            this._index = index;
        }
    }
}
