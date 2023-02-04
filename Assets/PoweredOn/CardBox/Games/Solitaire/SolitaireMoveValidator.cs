using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace PoweredOn.CardBox.Games.Solitaire
{
    class SolitaireMoveValidator
    {

        public struct ValidatorResult{
            public bool IsValid;
            public SolitaireMoveList Alternatives;

            public ValidatorResult(bool isValid, SolitaireMoveList alternatives)
            {
                this.IsValid = isValid;
                this.Alternatives = alternatives;
            }

            /*public void AcceptAlternative()
            {
                
            }*/
        }
        
        public ValidatorResult ValidateMoveAndSuggestAlternatives(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;

            if (CardIsAttemptingToReturnToPreviousSpot(move))
            {
                isValid = true;
            }
            else
            {
                isValid = ValidateMove(gameState, move);
            }



            SolitaireMoveList alternatives = SolitaireMoveSuggestor.SuggestMovesForCard(gameState, move.Subject);
            return new ValidatorResult(isValid, alternatives);
        }

        public bool ValidateMove(SolitaireGameState gameState, SolitaireMove move)
        {
            SolitaireMoveTypeGroup moveTypeGroup = move.GetSolitaireMoveTypeGroup();
            SolitaireMoveType moveType = move.GetSolitaireMoveType();
            switch (moveTypeGroup)
            {
                case SolitaireMoveTypeGroup.DECK_TO:
                    return ValidateDeckToMove(gameState, move);
                case SolitaireMoveTypeGroup.STOCK_TO:
                    return ValidateStockToMove(gameState, move);
                case SolitaireMoveTypeGroup.WASTE_TO:
                    return ValidateWasteToMove(gameState, move);
                case SolitaireMoveTypeGroup.HAND_TO:
                    return ValidateHandToMove(gameState, move);
                case SolitaireMoveTypeGroup.FOUNDATION_TO:
                    return ValidateFoundationToMove(gameState, move);
                case SolitaireMoveTypeGroup.TABLEAU_TO:
                    return ValidateTableauToMove(gameState, move);
                case SolitaireMoveTypeGroup.NONE:
                default:
                    return false;
            }
        }

        public bool ValidateDeckToMove(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;
            return isValid;
        }

        public bool ValidateStockToMove(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;
            return isValid;
        }

        public bool ValidateWasteToMove(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;
            return isValid;
        }

        public bool ValidateHandToMove(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;
            return isValid;
        }

        public bool ValidateFoundationToMove(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;
            return isValid;
        }

        public bool ValidateTableauToMove(SolitaireGameState gameState, SolitaireMove move)
        {
            bool isValid = false;
            return isValid;
        }

        public bool CardIsAttemptingToReturnToPreviousSpot(SolitaireMove move)
        {
            if (move.Subject.previousPlayfieldSpot.area == move.ToSpot.area)
            {
                return true;
            }
            return false;
        }
    }
}
