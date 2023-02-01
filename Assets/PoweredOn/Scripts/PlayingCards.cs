﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn
{
    public class PlayingCards
    {
        internal static List<SuitRank> GetDeckDefaultCardOrderList()
        {
            List <SuitRank> defaultDeckOrder = new List<SuitRank>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    defaultDeckOrder.Add(new SuitRank((Suit)i, (Rank)j));
                }
            }
            return defaultDeckOrder;
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
        public struct SuitRank
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
        }
    }
}
