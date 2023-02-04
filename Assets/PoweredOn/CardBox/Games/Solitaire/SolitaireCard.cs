using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.CardBox.Games.Solitaire.Piles;
using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireCard : PlayingCard
    {
        public SolitaireCard(Suit suit, Rank rank, int deckOrder) : base(suit, rank, deckOrder)
        {
        }

        public FoundationCardPile GetFoundationCardPile()
        {
            return Managers.GameManager.Instance.game.GetFoundationCardPileForSuit(this.GetSuit());
        }
    }
}
