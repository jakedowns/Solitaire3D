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
        DeckCardPile _deckPile;
        StockCardPile _stockPile;
        WasteCardPile _wastePile;
        HandCardPile _handPile;
        FoundationCardPileGroup _foundationPileGroup;
        TableauCardPileGroup _tableauPileGroup;

        public DeckCardPile DeckPile {
            get {
                return this._deckPile.Clone();
            }
        }

        public StockCardPile StockPile => _stockPile.Clone();

        public WasteCardPile WastePile
        {
            get
            {
                return this._wastePile.Clone();
            }
        }

        public HandCardPile HandPile
        {
            get
            {
                return this._handPile.Clone();
            }
        }

        public FoundationCardPileGroup FoundationPileGroup
        {
            get
            {
                return this._foundationPileGroup.Clone();
            }
        }

        public TableauCardPileGroup TableauPileGroup
        {
            get
            {
                return this._tableauPileGroup.Clone();
            }
        }

        public SolitaireGameState(
            SolitaireGame game
        )
        {
            this._handPile = game.GetHandCardPile();
            
            this._deckPile = game.GetDeckCardPile();

            this._stockPile = game.GetStockCardPile();

            this._wastePile = game.GetWasteCardPile();

            this._foundationPileGroup = game.GetFoundationCardPileGroup();

            this._tableauPileGroup = game.GetTableauCardPileGroup();

        }
    }
}
