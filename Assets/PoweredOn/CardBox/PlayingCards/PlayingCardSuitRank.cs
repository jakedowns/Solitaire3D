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

        internal static SuitRank FromInt(int suitRank)
        {
            if(suitRank >= 1000)
            {
                suitRank -= 1000; // remove extra offset for "face-up-ness" of tableau cards
            }
            Suit suit = (Suit)(suitRank / 100);
            Rank rank = (Rank)(suitRank % 100);
            return new SuitRank(suit, rank);
        }

        public static explicit operator int(SuitRank v)
        {
            return (int)v.suit * 100 + (int)v.rank;
        }
    }
}
