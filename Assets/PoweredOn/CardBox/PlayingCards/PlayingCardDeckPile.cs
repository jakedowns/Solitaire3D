using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardDeckPile: PlayingCardPile
    {
        public PlayingCardDeckPile(PlayingCardIDList cardList) : base(cardList, -1)
        {
        }

        public new PlayingCardDeckPile Clone()
        {
            return new PlayingCardDeckPile(cardList.Clone());
        }

        public void Shuffle()
        {
            // shuffle the underlying list
        }

        public void Shuffle(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                Shuffle();
            }
        }
    }
}
