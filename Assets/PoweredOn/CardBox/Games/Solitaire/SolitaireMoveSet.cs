using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
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
    }
}
