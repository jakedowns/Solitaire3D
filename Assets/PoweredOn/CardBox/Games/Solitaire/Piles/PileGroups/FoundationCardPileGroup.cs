using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PoweredOn.CardBox.PlayingCards;

namespace PoweredOn.CardBox.Games.Solitaire    
{
    public class FoundationCardPileGroup: SolitaireCardPileGroup
    {
        List<FoundationCardPile> piles = new List<FoundationCardPile>();

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
            List<FoundationCardPile> newPiles = new List<FoundationCardPile>();
            UnityEngine.Debug.Log($"trying to clone {newPiles.Count} {this.piles.Count}");
            for (int i = 0; i < this.piles.Count; i++)
            {
                newPiles.Add(piles[i].Clone());
            }

            return new FoundationCardPileGroup(newPiles);
        }

        public static FoundationCardPileGroup EMPTY
        {
            get { return new FoundationCardPileGroup(); }
        }

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

        public new IEnumerator<FoundationCardPile> GetEnumerator()
        {
            return this.piles.GetEnumerator();
        }

        public new FoundationCardPile this[int index]
        {
            get => this.piles[index];
            set => this.piles[index] = value;
        }
    }
}
