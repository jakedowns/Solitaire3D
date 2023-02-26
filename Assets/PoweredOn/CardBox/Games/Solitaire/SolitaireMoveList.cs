using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//using Unity.VisualScripting;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireMoveList : IEnumerable<SolitaireMove>
    {
        public List<SolitaireMove> moves = new List<SolitaireMove>();

        public SolitaireMoveList()
        {
            this.moves = new List<SolitaireMove>();
        }

        public SolitaireMoveList(List<SolitaireMove> moves)
        {
            this.moves = moves;
        }

        public SolitaireMove First()
        {
            return this.moves.DefaultIfEmpty<SolitaireMove>(SolitaireMove.INVALID).FirstOrDefault();
        }

        internal void Add(SolitaireMove move)
        {
            this.moves.Add(move);
        }

        // collection initializer
        public SolitaireMoveList(IEnumerable<SolitaireMove> _moves)
        {
            this.moves = new List<SolitaireMove>(_moves);
        }

        internal void Clear()
        {
            this.moves.Clear();
        }

        // shuffle
        public void Shuffle()
        {
            System.Random rng = new System.Random();
            int n = this.moves.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                SolitaireMove value = this.moves[k];
                this.moves[k] = this.moves[n];
                this.moves[n] = value;
            }
        }

        // reverse
        public void Reverse()
        {
            this.moves.Reverse();
        }

        public int Count { get { return this.moves.Count; } }

        
        public IEnumerator<SolitaireMove> GetEnumerator()
        {
            return this.moves.GetEnumerator();
        }

        
        public SolitaireMove this[int index]
        {
            get
            {
                return this.moves[index];
            }
        }

        // clone
        public SolitaireMoveList Clone()
        {
            return new SolitaireMoveList(this.moves);
        }

        // custom sort with custom predicate
        public SolitaireMoveList SortByMoveRank()
        {
            List<SolitaireMove> moveListSorted = new(this.moves);
            moveListSorted.Sort(new MoveComparer());
            return new SolitaireMoveList(moveListSorted);
        }

        public override string ToString()
        {
            string outString = $"Move List: count({Count})";

            foreach (SolitaireMove move in this.moves)
            {
                outString += "\n" + move;
            }
            
            return outString;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.moves.GetEnumerator();
        }
    }

    // Todo: improve ranking to rank moves that move MORE cards from a tableau (lower subindex) higher than moves that only move a single card (to help unblock face down cards in tableaus)
    // Todo: improve ranking so that if we just came from the previous spot, lower the rank of the move
    public class MoveComparer : IComparer<SolitaireMove>
    {
        public int Compare(SolitaireMove x, SolitaireMove y)
        {
            return x.CompareTo(y);
        }
    }
}
