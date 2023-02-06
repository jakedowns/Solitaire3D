using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.CardBox.PlayingCards;
using UnityEngine;
using UnityEngine.Assertions;

namespace PoweredOn.CardBox.Games.Solitaire
{

    [Flags]
    public enum GameStateFlags
    {
        None,
        Always,
        Never,

        HandIsEmpty,
        StockIsEmpty,
        WasteIsEmpty,

        WasteCanAcceptCard,

        IsCollectingCardsToDeck,
        IsDealing,
        IsShuffling
    }
    public class SolitaireGameState
    {
        DeckCardPile _deckPile = DeckCardPile.EMPTY;
        StockCardPile _stockPile = StockCardPile.EMPTY;
        WasteCardPile _wastePile = WasteCardPile.EMPTY;
        HandCardPile _handPile = HandCardPile.EMPTY;
        FoundationCardPileGroup _foundationPileGroup = FoundationCardPileGroup.EMPTY;
        TableauCardPileGroup _tableauPileGroup = TableauCardPileGroup.EMPTY;

        public bool IsDealing { get; protected set; }
        public bool IsCollectingCardsToDeck { get; protected set; }
        public bool IsShuffling { get; protected set; }
        public bool IsRecyclingWasteToStock { get; protected set; }
        
        

        public void SetMockIsDealing(bool value)
        {
            this.IsDealing = value;
        }

        public void SetMockIsCollectingCardsToDeck(bool value)
        {
            this.IsCollectingCardsToDeck = value;
        }
        public void SetMockIsShuffling(bool value)
        {
            this.IsShuffling = value;
        }
        public void SetMockIsRecyclingWasteToStock(bool value)
        {
            this.IsRecyclingWasteToStock = value;
        }


        /*public SolitaireGameState()
        {
            this._handPile = new HandCardPile();
            this._deckPile = new DeckCardPile();
            this._stockPile = new StockCardPile();
            this._foundationPileGroup = new FoundationCardPileGroup();
            this._tableauPileGroup = new TableauCardPileGroup();
        }*/

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

            // other state flags used for validation
            this.IsDealing = game.IsDealing;
            this.IsCollectingCardsToDeck = game.deck.IsCollectingCardsToDeck;
            this.IsShuffling = game.deck.IsShuffling;
            this.IsRecyclingWasteToStock = game.IsRecyclingWasteToStock;

        }

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

        public static GameStateFlags GetBitFlagsForCurrentGameState(SolitaireGameState gameState, SolitaireMove move)
        {
            Debug.Log("gameState: "+gameState);
            var bitflags = GameStateFlags.None;
            
            //SolitaireMoveType moveType = move.GetSolitaireMoveType();

            // flag if hand is empty
            if (gameState.HandPile.Count == 0)
                bitflags |= GameStateFlags.HandIsEmpty;

            if (gameState.IsDealing)
                bitflags |= GameStateFlags.IsDealing;

            if (gameState.IsCollectingCardsToDeck)
                bitflags |= GameStateFlags.IsCollectingCardsToDeck;

            if (gameState.IsShuffling)
                bitflags |= GameStateFlags.IsShuffling;

            return bitflags;
        }

        /*public static SolitaireGameState GetMockGameState()
        {
            return new SolitaireGameState();
        }*/

        public override string ToString()
        {
            string outstring = "";
            outstring += $"\n hand pile: {HandPile.Count}";
            outstring += $"\n deck pile: {DeckPile.Count}";
            outstring += $"\n stock pile: {StockPile.Count}";
            outstring += $"\n waste pile: {WastePile.Count}";
            outstring += $"\n foundations: ";
            int i = 0;
            foreach(FoundationCardPile pile in FoundationPileGroup)
            {
                outstring += $"f{i}: {pile.Count} ";
                    i++;
            }
            outstring += $"\n";
            i = 0;
            foreach (TableauCardPile pile in TableauPileGroup)
            {
                outstring += $"t{i}: {pile.Count} ";
                i++;
            }

            return outstring;
        }
    }
}
