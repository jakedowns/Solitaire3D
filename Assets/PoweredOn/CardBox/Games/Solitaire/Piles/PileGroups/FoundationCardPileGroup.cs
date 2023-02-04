using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Games.Solitaire.Piles;

namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class FoundationCardPileGroup
    {
        List<FoundationCardPile> piles;

        public FoundationCardPileGroup()
        {
            piles = new List<FoundationCardPile>();
            for (int i = 0; i < 4; i++)
            {
                piles.Add(new FoundationCardPile(i));
            }
        }

        public FoundationCardPileGroup(FoundationCardPile[] piles)
        {
            this.piles = new List<FoundationCardPile>(piles);
        }

        public FoundationCardPileGroup(List<FoundationCardPile> piles)
        {
            this.piles = new List<FoundationCardPile>(piles);
        }

        public FoundationCardPileGroup Clone()
        {
            List<FoundationCardPile> newPiles = new List<FoundationCardPile>(4);
            for (int i = 0; i < 4; i++)
            {
                newPiles[i] = piles[i].Clone();
            }

            return new FoundationCardPileGroup(newPiles);
        }

        /*public static PlayingCardIDList GetFoundationPlayingCardIDListForSuit(Suit suit)
        {
            return Managers.GameManager.Instance.game.GetFoundationCardsImmutable()[(int)suit];
        }*/

        public FoundationCardPile GetFoundationCardPileForSuit(Suit suit)
        {
            return piles[(int)suit];
        }

        public static PlayfieldSpot GetFoundationThatCanAcceptCard(SolitaireCard card)
        {
            FoundationCardPileGroup foundationCardPileGroup = Managers.GameManager.Instance.game.GetFoundationCardPileGroup();
            /*for (int i = 0; i < foundationCardPileGroup.Count; i++)
            {
                PlayingCardIDList foundationList = foundationCardPileGroup[i];

                if (foundationList.Count == 0)
                {
                    if (card.GetRank() == Rank.ACE)
                    {
                        return new PlayfieldSpot(PlayfieldArea.FOUNDATION, i, foundationList.Count);
                    }
                }
                else if (foundationList.Last() == card.GetSuit())
                {
                    if (foundationList.Last().rank == (int)card.GetRank() - 1)
                    {
                        return (PlayfieldSpot)(i + (int)PlayfieldSpot.Foundation1);
                    }
                }
            }*/
            return PlayfieldSpot.INVALID;
        }

        public IEnumerator<FoundationCardPile> GetEnumerator()
        {
            return this.piles.GetEnumerator();
        }
    }
}
