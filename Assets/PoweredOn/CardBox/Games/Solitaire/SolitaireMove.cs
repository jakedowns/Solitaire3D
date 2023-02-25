using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
using static PoweredOn.CardBox.Games.Solitaire.SolitairePlayfield;
namespace PoweredOn.CardBox.Games.Solitaire
{

    public enum SolitaireMoveTypeScores
    {
        // via https://australiancardgames.com.au/solitaire/
        STOCK_TO_WASTE = 5,
        WASTE_TO_STOCK = 5, // not actually what we award player, but we use this to rank the moves in the AutoSuggestor
        
        WASTE_TO_FOUNDATION = 10,
        TABLEAU_TO_FOUNDATION = 10,
        
        TURN_TABLEAU_CARD_FACE_UP = 5,
        WASTE_TO_TABLEAU = 5,

        TABLEAU_TO_TABLEAU = 3,
        FOUNDATION_TO_TABLEAU = -15,


        PENALTY_PER_10_SECONDS_ELAPSED = -2,

        PENALTY_FOR_RECYCLING_WASTE_TO_STOCK_DRAW_ONE = - 100,
        PENALTY_FOR_RECYCLING_WASTE_TO_STOCK_DRAW_THREE = -20,
    }
    
    public class SolitaireMove
    {
        private SolitaireCard _subject;
        private PlayfieldSpot _fromSpot;
        private PlayfieldSpot _toSpot;
        private int _substackIndex = 0;

        public static SolitaireMove INVALID
        {
            get {
                return new SolitaireMove(null, PlayfieldSpot.INVALID, PlayfieldSpot.INVALID);
            }
        }

        public SolitaireCard Subject
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

        public SolitaireMove(SolitaireCard subject, PlayfieldSpot fromSpot, PlayfieldSpot toSpot, int substackIndex = 0)
        {
            this._subject = subject;
            this._fromSpot = fromSpot;
            this._substackIndex = substackIndex;
            this._toSpot = toSpot;

        }

        public SolitaireMoveType GetSolitaireMoveType()
        {
            return SolitaireMoveSet.GetSolitaireMoveTypeForMove(this);
        }

        public SolitaireMoveTypeFromGroup GetSolitaireMoveTypeFromGroup()
        {
            return SolitaireMoveSet.GetSolitaireMoveTypeFromGroup(this);
        }

        public SolitaireMoveTypeToGroup GetSolitaireMoveTypeToGroup()
        {
            return SolitaireMoveSet.GetSolitaireMoveTypeToGroup(this);
        }

        public override string ToString()
        {
            return string.Format($"Move: {GetSolitaireMoveType()}:{GetMoveValue()} ");
        }

        internal bool IsValid()
        {
            return SolitaireMoveValidator.IsValidMove(Managers.GameManager.Instance.game.GetGameState(), this);
        }

        internal void Execute()
        {
            SolitaireGame game = Managers.GameManager.Instance.game;
            if (_fromSpot.area == PlayfieldArea.WASTE && ToSpot.area == PlayfieldArea.STOCK)
            {
                // recycle waste to stock
                game.WasteToStock();
            }
            else
            {
                // card-by-card movements
                
                bool faceUp = true;
                if (_toSpot.area == PlayfieldArea.STOCK)
                    faceUp = false;

                var cardList = game.CollectSubStack(_subject);
                int tabIndex = _subject.playfieldSpot.index;
                bool cameFromTab = _subject.playfieldSpot.area == PlayfieldArea.TABLEAU;
                var toSpotSubindex = _toSpot.subindex;
                foreach(SuitRank cardID in cardList)
                {
                    var card = game.deck.GetCardBySuitRank(cardID);
                    var clonedToSpot = _toSpot.Clone();
                    clonedToSpot.subindex = toSpotSubindex;
                    game.MoveCardToNewSpot(ref card, clonedToSpot, faceUp, 0, _substackIndex);
                    toSpotSubindex++;
                }

                game.CheckTableauForCardsToFlip();

            }
        }

        internal int CompareTo(SolitaireMove y)
        {
            int a = this.GetMoveValue();
            int b = y.GetMoveValue();

            return a < b ? -1 : a > b ? 1 : 0;
        }

        public int GetMoveValue()
        {
            // get string name of move type
            string moveType = Enum.GetName(typeof(SolitaireMoveType), GetSolitaireMoveType());
            //UnityEngine.Debug.Log("GetMoveValue " + moveType);

            // try getting the value using the string
            if(Enum.TryParse<SolitaireMoveTypeScores>(moveType, out SolitaireMoveTypeScores result))
            {
                UnityEngine.Debug.Log($"calculated value for move {moveType} : " + (int)result);
                return (int)result;
            }
            else
            {
                UnityEngine.Debug.Log($"failed to locate value for moveType: {moveType}");
            }

            return 0;
        }


        public static SolitaireMove WASTE_TO_STOCK {
            get {
                return new SolitaireMove(null, PlayfieldSpot.WASTE, new PlayfieldSpot(PlayfieldArea.STOCK, 0));
            }
        }
    }
}
