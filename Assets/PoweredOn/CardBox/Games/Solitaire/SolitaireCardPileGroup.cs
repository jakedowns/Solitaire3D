using System;
using System.Collections.Generic;
using System.Linq;
using PoweredOn.CardBox.PlayingCards;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireCardPileGroup: PlayingCardPileGroup
    {
        private List<SolitaireCardPile> piles;
        public new int Count
        {
            get
            {
                if(this.piles == null)
                {
                    return 0;
                }
                return this.piles.Count;
            }
        }

        /*public new IEnumerator<SolitaireCardPile> GetEnumerator()
        {
            return this.piles.GetEnumerator();
        }*/

        /*public new SolitaireCardPileGroup this[int index]
        {
            get => this.piles[index];
            set => this.piles[index] = value;
        }*/
    }
}
