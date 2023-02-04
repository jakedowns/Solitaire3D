using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using static PoweredOn.CardBox.Games.Solitaire.SolitairePlayfield;
namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireMove
    {
        private SolitaireCard _subject;
        private PlayfieldSpot _fromSpot;
        private PlayfieldSpot _toSpot;

        public PlayingCard Subject
        {
            get
            {
                return _subject;
            }
        }

        public PlayfieldSpot FromSpot
        {

            get
            {
                return _fromSpot;
            }
        }

        public PlayfieldSpot ToSpot
        {
            get
            {
                return _toSpot;
            }
        }

        public SolitaireMove(SolitaireCard subject, PlayfieldSpot fromSpot, PlayfieldSpot toSpot)
        {
            this._subject = subject;
            this._fromSpot = fromSpot;
            this._toSpot = toSpot;
        }

        public SolitaireMoveType GetSolitaireMoveType()
        {
            return SolitaireMoveSet.GetSolitaireMoveTypeForMove(this);
        }

        public SolitaireMoveTypeGroup GetSolitaireMoveTypeGroup()
        {
            return SolitaireMoveSet.GetSolitaireMoveTypeGroup(this);
        }

        public override string ToString()
        {
            return string.Format("Move {0} from {1} to {2}", Subject, FromSpot, ToSpot);
        }
    }
}
