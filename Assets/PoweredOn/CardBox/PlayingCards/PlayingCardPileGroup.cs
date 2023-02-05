using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardPileGroup
    {
        private List<PlayingCardPile> piles;
        public int Count
        {
            get
            {
                return this.piles.Count;
            }
        }

        public IEnumerator<PlayingCardPile> GetEnumerator()
        {
            return this.piles.GetEnumerator();
        }

        public PlayingCardPile this[int index]
        {
            get => this.piles[index];
            set => this.piles[index] = value;
        }
    }
}
