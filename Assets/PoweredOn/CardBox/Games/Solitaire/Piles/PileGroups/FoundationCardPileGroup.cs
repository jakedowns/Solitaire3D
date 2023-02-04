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
        FoundationCardPile[] foundationCardPiles;

        FoundationCardPileGroup()
        {
            foundationCardPiles = new FoundationCardPile[4];
            for (int i = 0; i < 4; i++)
            {
                foundationCardPiles[i] = new FoundationCardPile(i);
            }
        }

        public FoundationCardPileGroup(FoundationCardPile[] foundationCardPiles)
        {
            this.foundationCardPiles = foundationCardPiles;
        }

        public static PlayingCardIDList GetFoundationPlayingCardIDListForSuit(Suit suit)
        {
            return Managers.GameManager.Instance.game.GetFoundationCardsImmutable()[(int)suit];
        }

        public FoundationCardPile GetFoundationCardPileForSuit(Suit suit)
        {
            return foundationCardPiles[(int)suit];
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
    }
}
