using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

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
                isValid = IsValidMove(gameState, move);
            }



            SolitaireMoveList alternatives = SolitaireMoveSuggestor.SuggestMovesForCard(gameState, move.Subject);
            return new ValidatorResult(isValid, alternatives);
        }

        public static bool IsValidMove(SolitaireGameState gameState, SolitaireMove move)
        {
            GameStateFlags bitflags = SolitaireGameState.GetBitFlagsForCurrentGameState(gameState, move);
            SolitaireMoveType moveType = move.GetSolitaireMoveType();
            SolitaireMoveTypeFromGroup moveFromGroup = move.GetSolitaireMoveTypeFromGroup();
            SolitaireMoveTypeToGroup moveToGroup = move.GetSolitaireMoveTypeToGroup();
            SolitaireMoveStatusFlags moveStatusFlags = SolitaireMoveSet.GetStatusFlagsForMove(gameState, move);

            bool isValid = false; // false by default;

            Debug.Log($"validating moveType: {moveType} with current state bitflags: {bitflags} and moveStatusFlags {moveStatusFlags}");

            // todo block moves from player if autoplaying

            //if(moveToGroup == SolitaireMoveTypeToGroup.HAND)
            if (move.ToSpot.area == PlayfieldArea.HAND)
            {
                if( (bitflags & GameStateFlags.HandIsEmpty) == 0 )
                {
                    Debug.Log($"blocking TO_HAND move, hand is not empty");
                    Debug.Log($"TODO: allow exception for when we're picking up a SUBSTACK");
                    return false;
                }
                else
                {
                    // otherwise, i don't think there's any other validation to do
                    // maybe we could add specifics like... it can't be coming straight from Stock
                    // and... maybe some additional checks to prevent picking up cards that are face down?
                    if (move.FromSpot.area == PlayfieldArea.STOCK)
                    {
                        Debug.Log($"blocking TO_HAND move, cannot pick up directly from stock");
                        return false;
                    }
                    if (!move.Subject.IsFaceUp)
                    {
                        Debug.Log($"blocking TO_HAND move, card was not face up");
                        return false;
                    }
                    
                    return true;
                }
            }

            switch (moveType)
            {
                // === 1. From Hand Moves
                case SolitaireMoveType.HAND_TO_HAND:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.HandCanReceiveCard) == (SolitaireMoveStatusFlags.HandCanReceiveCard);
                case SolitaireMoveType.HAND_TO_FOUNDATION:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.FoundationCanReceiveCard) == (SolitaireMoveStatusFlags.FoundationCanReceiveCard);
                case SolitaireMoveType.HAND_TO_DECK:
                    // valid when collecting
                    return (bitflags & GameStateFlags.IsCollectingCardsToDeck) == (GameStateFlags.IsCollectingCardsToDeck);
                case SolitaireMoveType.HAND_TO_TABLEAU:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.TableauCanReceiveCard) == (SolitaireMoveStatusFlags.TableauCanReceiveCard) 
                        || (moveStatusFlags & SolitaireMoveStatusFlags.CardIsReturningFromHand) == (SolitaireMoveStatusFlags.CardIsReturningFromHand); ;
                case SolitaireMoveType.HAND_TO_STOCK:
                    return false;
                case SolitaireMoveType.HAND_TO_WASTE:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.CardIsReturningFromHand) == (SolitaireMoveStatusFlags.CardIsReturningFromHand);

                // === 2. From Deck Moves
                case SolitaireMoveType.DECK_TO_HAND:
                case SolitaireMoveType.DECK_TO_WASTE:
                case SolitaireMoveType.DECK_TO_FOUNDATION:
                    return false; // these 3 are always invalid
                case SolitaireMoveType.DECK_TO_DECK:
                    // valid when shuffling
                    return (bitflags & GameStateFlags.IsShuffling) == (GameStateFlags.IsShuffling);

                case SolitaireMoveType.DECK_TO_STOCK:
                case SolitaireMoveType.DECK_TO_TABLEAU:
                    // valid when dealing
                    return (bitflags & GameStateFlags.IsDealing) == (GameStateFlags.IsDealing);

                // === 3. From Stock Moves
                case SolitaireMoveType.STOCK_TO_HAND:
                    return false;
                case SolitaireMoveType.STOCK_TO_DECK:
                    // WhenCollecting
                    return (bitflags & GameStateFlags.IsCollectingCardsToDeck) == (GameStateFlags.IsCollectingCardsToDeck);
                case SolitaireMoveType.STOCK_TO_STOCK:
                    return false;
                case SolitaireMoveType.STOCK_TO_WASTE:
                    // we require hand to be empty, otherwise a HAND_TO move should've been requested
                    return (bitflags & GameStateFlags.HandIsEmpty) == (GameStateFlags.HandIsEmpty);
                case SolitaireMoveType.STOCK_TO_FOUNDATION:
                    return false;
                case SolitaireMoveType.STOCK_TO_TABLEAU:
                    return false;

                // === 4. From Waste Moves
                case SolitaireMoveType.WASTE_TO_HAND:
                    return ( bitflags & GameStateFlags.HandIsEmpty ) == (GameStateFlags.HandIsEmpty);
                case SolitaireMoveType.WASTE_TO_DECK:
                    return (bitflags & GameStateFlags.IsCollectingCardsToDeck) == (GameStateFlags.IsCollectingCardsToDeck);
                case SolitaireMoveType.WASTE_TO_STOCK:
                    return (bitflags & GameStateFlags.IsRecyclingWasteToStock) == (GameStateFlags.IsRecyclingWasteToStock);
                case SolitaireMoveType.WASTE_TO_WASTE:
                    return false;
                case SolitaireMoveType.WASTE_TO_FOUNDATION:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.FoundationCanReceiveCard) == (SolitaireMoveStatusFlags.FoundationCanReceiveCard);
                case SolitaireMoveType.WASTE_TO_TABLEAU:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.TableauCanReceiveCard) == (SolitaireMoveStatusFlags.TableauCanReceiveCard);

                // === 5. From Foundation Moves
                case SolitaireMoveType.FOUNDATION_TO_HAND:
                    return (bitflags & GameStateFlags.HandIsEmpty) == (GameStateFlags.HandIsEmpty);
                case SolitaireMoveType.FOUNDATION_TO_DECK:
                    return (bitflags & GameStateFlags.IsCollectingCardsToDeck) == (GameStateFlags.IsCollectingCardsToDeck);
                case SolitaireMoveType.FOUNDATION_TO_STOCK:
                    return false; // NEVER
                case SolitaireMoveType.FOUNDATION_TO_WASTE:
                    return false; // TODO: only when undoRedo is being applied
                case SolitaireMoveType.FOUNDATION_TO_FOUNDATION:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.FoundationCanReceiveCard) == (SolitaireMoveStatusFlags.FoundationCanReceiveCard);
                case SolitaireMoveType.FOUNDATION_TO_TABLEAU:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.TableauCanReceiveCard) == (SolitaireMoveStatusFlags.TableauCanReceiveCard);

                // === 6. From Tableau Moves
                case SolitaireMoveType.TABLEAU_TO_HAND:
                    return (bitflags & GameStateFlags.HandIsEmpty) == (GameStateFlags.HandIsEmpty);
                case SolitaireMoveType.TABLEAU_TO_DECK:
                    return (bitflags & GameStateFlags.IsCollectingCardsToDeck) == (GameStateFlags.IsCollectingCardsToDeck);
                case SolitaireMoveType.TABLEAU_TO_STOCK:
                    return false;
                case SolitaireMoveType.TABLEAU_TO_WASTE:
                    return false; // TODO: when card is returning it should be valid
                case SolitaireMoveType.TABLEAU_TO_FOUNDATION:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.FoundationCanReceiveCard) == (SolitaireMoveStatusFlags.FoundationCanReceiveCard);
                case SolitaireMoveType.TABLEAU_TO_TABLEAU:
                    return (moveStatusFlags & SolitaireMoveStatusFlags.TableauCanReceiveCard) == (SolitaireMoveStatusFlags.TableauCanReceiveCard);

                default:
                    Debug.LogWarning("unhandled move type: " + moveType);
                    return isValid;
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
