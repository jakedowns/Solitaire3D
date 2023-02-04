using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.PlayingCards
{
    // suit enum
    public enum Suit
    {
        CLUBS
        , DIAMONDS
        , HEARTS
        , SPADES
        , NONE
    }

    // rank enum
    public enum Rank
    {
        ACE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING,
        NONE
    }

    // Special Tuple (maybe rename or alias to CardIDTuple or CardID)
    public struct SuitRank //: IEqualityComparer<SuitRank>
    {
        public Suit suit { get; set; }
        public Rank rank { get; set; }

        public SuitRank(Suit suit, Rank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public static SuitRank NONE
        {
            get
            {
                return new SuitRank(Suit.NONE, Rank.NONE);
            }
        }

        public static bool IsSuitRankNone(SuitRank suitRank)
        {
            return suitRank.suit == Suit.NONE && suitRank.rank == Rank.NONE;
        }

        public bool IsSuitRankNone() => IsSuitRankNone(this);

        // custom ToString method
        public override string ToString()
        {
            return Enum.GetName(typeof(Suit), suit) + " " + Enum.GetName(typeof(Rank), rank);
        }
    }
}
