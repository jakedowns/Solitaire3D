using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.CardBox.Games.Solitaire.Piles;
using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireGameState
    {
        DeckCardPile _deckOrderList;
        public DeckCardPile DeckOrderList {
            get {
                return this._deckOrderList.Clone();
            }
        }

        public SolitaireGameState(
            SolitaireGame game
        )
        {
            this._deckOrderList = game.GetDeckOrderListImmutable();    
        }
    }
}
