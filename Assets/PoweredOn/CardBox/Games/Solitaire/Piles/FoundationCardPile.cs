using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class FoundationCardPile : SolitaireCardPile
    {
        private int _index;

        public FoundationCardPile(int index)
        {
            this._index = index;
        }

        public FoundationCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new FoundationCardPile Clone()
        {
            return new FoundationCardPile(cardList.Clone());
        }
        public bool CanAcceptCard(SolitaireCard card)
        {
            if (Count == 0)
            {
                if (card.GetRank() == Rank.ACE)
                {
                    return true;
                }
            }
            else if (Last().suit == card.GetSuit())
            {
                if ((int)Last().rank == (int)card.GetRank() - 1)
                {
                    return true;
                }
            }

            return false;
        }
        
        public PlayfieldSpot GetPlayfieldSpot()
        {
           return new PlayfieldSpot(PlayfieldArea.FOUNDATION, _index);
        }
    }
}
