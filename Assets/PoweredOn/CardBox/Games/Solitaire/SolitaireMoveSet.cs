using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{

    [Flags]
    public enum SolitaireMoveStatusFlags
    {
        None,
        TableauCanReceiveCard,
        FoundationCanReceiveCard,
        WasteCanReceiveCard,
        StockCanReceiveCard,
        HandCanReceiveCard,
        DeckCanReceiveCard,
        CardIsReturningFromHand
    }

    public enum SolitaireMoveTypeFromGroup
    {
        DECK,
        STOCK,
        WASTE,
        FOUNDATION,
        TABLEAU,
        HAND,
        NONE
    }

    public enum SolitaireMoveTypeToGroup
    {
        DECK,
        STOCK,
        WASTE,
        FOUNDATION,
        TABLEAU,
        HAND,
        NONE
    }
    
    public enum SolitaireMoveType
    {
        // all permutations of each of these to eachother, or themselves
        // Deck, Stock, Waste, Foundation, Tableau, Hand

        DECK_TO_DECK,
        DECK_TO_STOCK,
        DECK_TO_WASTE,
        DECK_TO_FOUNDATION,
        DECK_TO_TABLEAU,
        DECK_TO_HAND,

        STOCK_TO_DECK,
        STOCK_TO_STOCK,
        STOCK_TO_WASTE,
        STOCK_TO_FOUNDATION,
        STOCK_TO_TABLEAU,
        STOCK_TO_HAND,

        WASTE_TO_DECK,
        WASTE_TO_STOCK,
        WASTE_TO_WASTE,
        WASTE_TO_FOUNDATION,
        WASTE_TO_TABLEAU,
        WASTE_TO_HAND,

        FOUNDATION_TO_DECK,
        FOUNDATION_TO_STOCK,
        FOUNDATION_TO_WASTE,
        FOUNDATION_TO_FOUNDATION,
        FOUNDATION_TO_TABLEAU,
        FOUNDATION_TO_HAND,

        TABLEAU_TO_DECK,
        TABLEAU_TO_STOCK,
        TABLEAU_TO_WASTE,
        TABLEAU_TO_FOUNDATION,
        TABLEAU_TO_TABLEAU,
        TABLEAU_TO_HAND,

        HAND_TO_DECK,
        HAND_TO_STOCK,
        HAND_TO_WASTE,
        HAND_TO_FOUNDATION,
        HAND_TO_TABLEAU,
        HAND_TO_HAND,

        NONE
    }

    public static class SolitaireMoveSet
    {
        public static SolitaireMoveType GetSolitaireMoveTypeForMove(SolitaireMove move)
        {
            string from = Enum.GetName(typeof(PlayfieldArea), move.FromSpot.area);
            string to = Enum.GetName(typeof(PlayfieldArea), move.ToSpot.area);
            SolitaireMoveType type;
            var result = Enum.TryParse<SolitaireMoveType>($"{from}_TO_{to}", true, out type);
            if (result)
                return type;
            else
                return SolitaireMoveType.NONE;
        }

        public static Dictionary<SolitaireMoveTypeFromGroup, PlayfieldArea> FromTypeToAreaMap = new Dictionary<SolitaireMoveTypeFromGroup, PlayfieldArea>{
            { SolitaireMoveTypeFromGroup.STOCK, PlayfieldArea.STOCK },
            { SolitaireMoveTypeFromGroup.WASTE, PlayfieldArea.WASTE },
            { SolitaireMoveTypeFromGroup.FOUNDATION, PlayfieldArea.FOUNDATION },
            { SolitaireMoveTypeFromGroup.TABLEAU, PlayfieldArea.TABLEAU },
            { SolitaireMoveTypeFromGroup.HAND, PlayfieldArea.HAND },
            { SolitaireMoveTypeFromGroup.DECK, PlayfieldArea.DECK },
            { SolitaireMoveTypeFromGroup.NONE, PlayfieldArea.INVALID }
        };

        public static Dictionary<SolitaireMoveTypeToGroup, PlayfieldArea> ToTypeToAreaMap = new Dictionary<SolitaireMoveTypeToGroup, PlayfieldArea>{
            { SolitaireMoveTypeToGroup.STOCK, PlayfieldArea.STOCK },
            { SolitaireMoveTypeToGroup.WASTE, PlayfieldArea.WASTE },
            { SolitaireMoveTypeToGroup.FOUNDATION, PlayfieldArea.FOUNDATION },
            { SolitaireMoveTypeToGroup.TABLEAU, PlayfieldArea.TABLEAU },
            { SolitaireMoveTypeToGroup.HAND, PlayfieldArea.HAND },
            { SolitaireMoveTypeToGroup.DECK, PlayfieldArea.DECK },
            { SolitaireMoveTypeToGroup.NONE, PlayfieldArea.INVALID }
        };

        public static PlayfieldArea GetPlayfieldAreaForMoveTypeFrom(SolitaireMoveTypeFromGroup type)
        {
            return FromTypeToAreaMap[type];   
        }

        public static PlayfieldArea GetPlayfieldAreaForMoveToTypeTo(SolitaireMoveTypeToGroup type)
        {
            return ToTypeToAreaMap[type];
        }

        public static SolitaireMoveTypeFromGroup GetSolitaireMoveTypeFromGroup(SolitaireMove move)
        {
            string from = Enum.GetName(typeof(PlayfieldArea), move.FromSpot.area);
            SolitaireMoveTypeFromGroup type;
            var result = Enum.TryParse<SolitaireMoveTypeFromGroup>($"{from}", true, out type);
            if (result)
                return type;
            else
                return SolitaireMoveTypeFromGroup.NONE;
        }

        public static SolitaireMoveTypeToGroup GetSolitaireMoveTypeToGroup(SolitaireMove move)
        {
            string to = Enum.GetName(typeof(PlayfieldArea), move.ToSpot.area);
            SolitaireMoveTypeToGroup type;
            var result = Enum.TryParse<SolitaireMoveTypeToGroup>($"{to}", true, out type);
            if (result)
                return type;
            else
                return SolitaireMoveTypeToGroup.NONE;
        }

        
        public static SolitaireMoveStatusFlags GetStatusFlagsForMove(SolitaireGameState gameState, SolitaireMove move)
        {
            var flags = SolitaireMoveStatusFlags.None;

            if (
                move.Subject.playfieldSpot.area == PlayfieldArea.HAND
                && move.Subject.previousPlayfieldSpot.area == move.ToSpot.area
            )
            {
                flags |= SolitaireMoveStatusFlags.CardIsReturningFromHand;
            }

            switch (move.ToSpot.area)
            {
                case PlayfieldArea.TABLEAU:
                    var tableau = gameState.TableauPileGroup[move.ToSpot.index];
                    if (tableau.CanReceiveCard(move.Subject))
                        flags |= SolitaireMoveStatusFlags.TableauCanReceiveCard;
                    break;
                case PlayfieldArea.FOUNDATION:
                    UnityEngine.Debug.LogWarning($"get status flags for move: foundation pile group : move.ToSpot.index {move.ToSpot.index}");
                    var foundation = gameState.FoundationPileGroup[move.ToSpot.index];
                    if (foundation.CanReceiveCard(move.Subject))
                        flags |= SolitaireMoveStatusFlags.FoundationCanReceiveCard;
                    break;
                case PlayfieldArea.STOCK:
                    if (gameState.StockPile.CanReceiveCard(move.Subject))
                        flags |= SolitaireMoveStatusFlags.StockCanReceiveCard;
                    break;
                case PlayfieldArea.WASTE:
                    if (gameState.WastePile.CanRecieveCard(move.Subject))
                        flags |= SolitaireMoveStatusFlags.WasteCanReceiveCard;
                    break;
                case PlayfieldArea.HAND:
                    if (gameState.HandPile.CanReceiveCard(move.Subject))
                        flags |= SolitaireMoveStatusFlags.HandCanReceiveCard;
                    break;
                case PlayfieldArea.DECK:
                    if (gameState.DeckPile.CanReceiveCard(move.Subject))
                        flags |= SolitaireMoveStatusFlags.DeckCanReceiveCard;
                    break;
            }

            return flags;
        }
    }
}
