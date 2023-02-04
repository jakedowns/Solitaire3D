using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireMoveList
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

        internal void Add(SolitaireMove move)
        {
            this.moves.Add(move);
        }

        internal void Clear()
        {
            this.moves.Clear();
        }

        public int Count { get { return this.moves.Count; } }
    }
}
