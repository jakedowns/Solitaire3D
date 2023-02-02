using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

using static PoweredOn.PlayingCards.CardList;

namespace PoweredOn.PlayingCards
{
    public static class PLAYING_CARD_DEFAULTS
    {
        public static CardList DEFAULT_DECK_ORDER
        {
            get
            {
                CardList defaultDeckOrder = new CardList();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                defaultDeckOrder.Add(new SuitRank((Suit)i, (Rank)j));
            }
        }
        return defaultDeckOrder;
            }
        }
    }

    // suit enum
    public enum Suit
    {
        clubs    // 0 black
        , spades    // 1 black
        , diamonds // 2 red
        , hearts   // 3 red
    }

    // rank enum
    public enum Rank
    {
        ace,
        two,
        three,
        four,
        five,
        six,
        seven,
        eight,
        nine,
        ten,
        jack,
        queen,
        king
    }

    // Special Tuple
    public struct SuitRank //: IEqualityComparer<SuitRank>
    {
        public Suit suit { get; set; }
        public Rank rank { get; set; }

        public SuitRank(Suit suit, Rank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        // custom ToString method
        public override string ToString()
        {
            return Enum.GetName(typeof(Suit), suit) + " " + Enum.GetName(typeof(Rank), rank);
        }
        /*public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        int IEqualityComparer<SuitRank>.GetHashCode()
        {
            return (int)base.suit * 100 + (int)base.rank;
        }

        bool IEqualityComparer<SuitRank>.Equals(SuitRank x, SuitRank y)
        {
            return x.suit == y.suit && x.rank == y.rank;
        }

        int IEqualityComparer<SuitRank>.GetHashCode(SuitRank obj)
        {
            return (int)obj.suit * 100 + (int)obj.rank;
        }

        public static bool operator ==(SuitRank c1, SuitRank c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(SuitRank c1, SuitRank c2)
        {
            return !c1.Equals(c2);
        }*/
    }
}
