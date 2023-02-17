using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public enum SolitaireGameObject // extends GameGameObject
    {
        None,
        
        Foundation1_Base,
        Foundation2_Base,
        Foundation3_Base,
        Foundation4_Base,

        // tableaus
        Tableau1_Base,
        Tableau2_Base,
        Tableau3_Base,
        Tableau4_Base,
        Tableau5_Base,
        Tableau6_Base,
        Tableau7_Base,

        // stock / waste
        Stock_Base,
        Waste_Base,
        
        Hand_Base,
        Deck_Base,
        Deck_Offset, // separate anchor point for the deck that exists outside of the deck's subtree

        // card types:
        Card_ace_of_clubs,
        Card_two_of_clubs,
        Card_three_of_clubs,
        Card_four_of_clubs,
        Card_five_of_clubs,
        Card_six_of_clubs,
        Card_seven_of_clubs,
        Card_eight_of_clubs,
        Card_nine_of_clubs,
        Card_ten_of_clubs,
        Card_jack_of_clubs,
        Card_queen_of_clubs,
        Card_king_of_clubs,

        Card_ace_of_diamonds,
        Card_two_of_diamonds,
        Card_three_of_diamonds,
        Card_four_of_diamonds,
        Card_five_of_diamonds,
        Card_six_of_diamonds,
        Card_seven_of_diamonds,
        Card_eight_of_diamonds,
        Card_nine_of_diamonds,
        Card_ten_of_diamonds,
        Card_jack_of_diamonds,
        Card_queen_of_diamonds,
        Card_king_of_diamonds,

        Card_ace_of_hearts,
        Card_two_of_hearts,
        Card_three_of_hearts,
        Card_four_of_hearts,
        Card_five_of_hearts,
        Card_six_of_hearts,
        Card_seven_of_hearts,
        Card_eight_of_hearts,
        Card_nine_of_hearts,
        Card_ten_of_hearts,
        Card_jack_of_hearts,
        Card_queen_of_hearts,
        Card_king_of_hearts,

        Card_ace_of_spades,
        Card_two_of_spades,
        Card_three_of_spades,
        Card_four_of_spades,
        Card_five_of_spades,
        Card_six_of_spades,
        Card_seven_of_spades,
        Card_eight_of_spades,
        Card_nine_of_spades,
        Card_ten_of_spades,
        Card_jack_of_spades,
        Card_queen_of_spades,
        Card_king_of_spades
    }

    public enum SolitairePileObject
    {
        Foundation1_Pile,
        Foundation2_Pile,
        Foundation3_Pile,
        Foundation4_Pile,
        
        Tableau1_Pile,
        Tableau2_Pile,
        Tableau3_Pile,
        Tableau4_Pile,
        Tableau5_Pile,
        Tableau6_Pile,
        Tableau7_Pile,

        Stock_Pile,
        Waste_Pile,
        Hand_Pile,
        Deck_Pile
    }

    public enum SolitairePileType
    {
        Foundation,
        Tableau,
        Stock,
        Waste,
        Hand,
        Deck
    }
}
