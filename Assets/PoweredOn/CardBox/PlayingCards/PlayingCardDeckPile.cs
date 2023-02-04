using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.Games.Solitaire.Piles;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardDeckPile: DeckCardPile
    {
        public PlayingCardDeckPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new PlayingCardDeckPile Clone()
        {
            return new PlayingCardDeckPile(cardList.Clone());
        }
    }
}
