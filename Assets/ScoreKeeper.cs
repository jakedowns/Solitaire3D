using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.Games.Solitaire;
using UnityEngine;

namespace Assets
{
    public class ScoreKeeper
    {
        public int score { get; private set; } = 0;
        public int moves { get; private set; } = 0;
        public int time { get; private set; } = 0;

        private ScoreDisplay scoreDisplay;

        public ScoreKeeper()
        {
            this.scoreDisplay = GameObject.Find("ScoreDisplay").GetComponent<ScoreDisplay>();
        }

        public void RecordMove(SolitaireMove move)
        {
            moves++;
            score += move.GetMoveValue();
            scoreDisplay.UpdateText(this);
        }

        public void RecordTableauCardFlipped()
        {
            moves++;
            score += (int)SolitaireMoveTypeScores.TURN_TABLEAU_CARD_FACE_UP;
            scoreDisplay.UpdateText(this);
        }

        public void RecordWasteToStockRecycle()
        {
            moves++;
            // todo: support DRAW_THREE
            score += (int)SolitaireMoveTypeScores.PENALTY_FOR_RECYCLING_WASTE_TO_STOCK_DRAW_ONE;
            scoreDisplay.UpdateText(this);
        }

        public string GetTimeFormatted()
        {
            // convert int time (seconds) in MM:SS format:
            TimeSpan t = TimeSpan.FromSeconds(time);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                t.Hours,
                t.Minutes,
                t.Seconds);
        }

        public void Tick()
        {
            // increase by 1 second
            time++;
            scoreDisplay.UpdateText(this);
        }

        public void Reset()
        {
            score = 0;
            moves = 0;
            time = 0;
        }
    }
}
