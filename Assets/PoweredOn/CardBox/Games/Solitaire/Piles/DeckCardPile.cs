using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;

namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class DeckCardPile : PlayingCardPile
    {
        public DeckCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new DeckCardPile Clone()
        {
            return new DeckCardPile(cardList.Clone());
        }
    }
}
