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

            bool isValid = false; // false by default;

            Debug.Log($"validating moveType: {moveType} with bitflags: {bitflags}");

            //if(moveToGroup == SolitaireMoveTypeToGroup.HAND)
            if(move.ToSpot.area == PlayfieldArea.HAND)
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
                case SolitaireMoveType.STOCK_TO_WASTE:
                    // we require hand to be empty, otherwise a HAND_TO move should've been requested
                    return ( bitflags & GameStateFlags.HandIsEmpty ) == ( GameStateFlags.HandIsEmpty );

                case SolitaireMoveType.WASTE_TO_HAND:
                    return ( bitflags & GameStateFlags.HandIsEmpty ) == (GameStateFlags.HandIsEmpty);


                /* case SolitaireMoveType.StockToAny:
                     return false;
                 case SolitaireMoveType.WasteToStock:
                     return (bitflags & GameStateFlags.StockIsEmpty) == GameStateFlags.StockIsEmpty;
                 case SolitaireMoveType.WasteToWaste:
                     return (bitflags & GameStateFlags.WasteCanAcceptCard) == GameStateFlags.WasteCanAcceptCard;
                 case SolitaireMoveType.WasteToFoundation:
                     return (bitflags & GameStateFlags.FoundationCanAcceptCard) == GameStateFlags.FoundationCanAcceptCard;
                 case SolitaireMoveType.WasteToTabealu:
                     return (bitflags & GameStateFlags.TableauCanAcceptCard) == GameStateFlags.TableauCanAcceptCard;
                 case SolitaireMoveType.WasteToHand:
                     return (bitflags & GameStateFlags.HandIsEmpty) == GameStateFlags.HandIsEmpty;
                 case SolitaireMoveType.WasteToDeck:
                     return (bitflags & GameStateFlags.IsCollectingDeck) == GameStateFlags.IsCollectingDeck;
                 case SolitaireMoveType.TableauToStock:
                     return (bitflags & GameStateFlags.StockIsEmpty) == GameStateFlags.StockIsEmpty;
                 case SolitaireMoveType.TableauToWaste:
                     return (bitflags & GameStateFlags.WasteIsEmpty) == GameStateFlags.WasteIsEmpty;
                 case SolitaireMoveType.TableauToFoundation:
                     return (bitflags & GameStateFlags.FoundationCanAcceptCard) == GameStateFlags.FoundationCanAcceptCard;
                 case SolitaireMoveType.TableauToTableau:
                     return (bitflags & GameStateFlags.TableauCanAcceptCard) == GameStateFlags.TableauCanAcceptCard;*/
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
