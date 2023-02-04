using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public static class SolitaireMoveSuggestor
    {
        public static SolitaireMoveList SuggestMoves(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();

            // 1. any moves that move a card from the waste|tableau to a foundation
            moves = GetMovesToFoundation(game);
            if (moves.moves.Count > 0)
                return moves;

            // 2. any moves that move a card from the waste|tableau to a tableau
            moves = GetMovesToTableau(game);
            if (moves.moves.Count > 0)
                return moves;

            // 3. any moves that move a card from the stock to the waste (if the stock is empty, recycle the waste)
            moves = GetMovesFromStockToWaste(game);
            if (moves.moves.Count > 0)
                return moves;

            // 4. any moves that recycle the waste to the stock
            moves = GetMovesFromWasteToStock(game);
            if (moves.moves.Count > 0)
                return moves;

            // default: no moves left (rare)
            return moves; // empty
        }

        private static SolitaireMoveList GetMovesFromStockToWaste(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        private static SolitaireMoveList GetMovesFromWasteToStock(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        private static SolitaireMoveList GetMovesToTableau(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        private static SolitaireMoveList GetMovesToFoundation(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        // ### Card Specific Variants

        public static SolitaireMoveList SuggestMovesForCard(SolitaireGameState gameState, SolitaireCard card)
        {
            SolitaireMoveList moves = new SolitaireMoveList();

            moves = GetMovesToFoundationForCard(gameState, card);
            if (moves.Count > 0)
                return moves;

            return moves;
        }

        private static SolitaireMoveList GetMovesToFoundationForCard(SolitaireGameState gameState, SolitaireCard card)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            FoundationCardPile foundationCardPile = card.GetFoundationForCard();
            return moves;
        }
    }
}
