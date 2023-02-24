using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using UnityEngine;

namespace Assets
{
    public class ScoreKeeper
    {
        public int score { get; private set; } = 0;
        public int moves { get; private set; } = 0;
        public int time { get; private set; } = 0;

        public int best_score { get; private set; } = 0;
        public int best_time { get; private set; } = int.MaxValue;
        public int best_moves { get; private set; } = int.MaxValue;

        private ScoreDisplay scoreDisplay;

        public ScoreKeeper()
        {
            this.scoreDisplay = GameObject.Find("ScoreDisplay").GetComponent<ScoreDisplay>();
        }

        public void LoadState(DataStore.UserData userData)
        {
            this.score = userData.current_score;
            this.time = userData.current_time;
            this.moves = userData.current_moves;

            // bests
            this.best_score = userData.best_score;
            this.best_time = userData.best_time;
            this.best_moves = userData.best_moves;
        }

        public void RecordMove(SolitaireMove move)
        {
            moves++;
            RecordScore(move.GetMoveValue());
        }

        public void RecordTableauCardFlipped()
        {
            moves++;
            RecordScore((int)SolitaireMoveTypeScores.TURN_TABLEAU_CARD_FACE_UP);
        }

        public void RecordWasteToStockRecycle()
        {
            moves++;
            // todo: support DRAW_THREE
            RecordScore((int)SolitaireMoveTypeScores.PENALTY_FOR_RECYCLING_WASTE_TO_STOCK_DRAW_ONE);
        }

        public void RecordStockToWaste()
        {
            moves++;
            RecordScore(0);
        }

        public void RecordScore(int score)
        {
            score += score;

            // prevent score from going below 0
            // TODO: keep a second internal score for the AI to see?
            score = Math.Max(0, score);

            scoreDisplay.UpdateText(this);

            // persist
            PoweredOn.Managers.GameManager.Instance.dataStore.UpdateScoreData(PoweredOn.Managers.GameManager.Instance.game);
            PoweredOn.Managers.GameManager.Instance.dataStore.StoreData(); // write to disk
        }

        public void CheckFinalScore(SolitaireGame game)
        {
            if(score > best_score)
            {
                best_score = score;
                // FIRE EVENT NEW HIGH SCORE
            }

            if (moves < best_moves)
            {
                best_moves = moves;
                // FIRE EVENT NEW FEWEEST MOVES
            }

            if(time < best_time)
            {
                best_time = time;
                // FIRE EVENT NEW BEST TIME
            }

            GameManager.Instance.dataStore.Reset();
            GameManager.Instance.dataStore.UpdateScoreData(game);
            GameManager.Instance.dataStore.StoreData();
        }

        public string GetTimeFormatted()
        {
            // convert int time (seconds) in MM:SS format:
            TimeSpan t = TimeSpan.FromSeconds(time);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
        }

        public void Tick()
        {
            // increase by 1 second
            time++;
            scoreDisplay.UpdateText(this);
            if (PoweredOn.Managers.GameManager.Instance == null)
            {
                return;
            }
            PoweredOn.Managers.GameManager.Instance.dataStore.UpdateScoreData(PoweredOn.Managers.GameManager.Instance.game);
            PoweredOn.Managers.GameManager.Instance.dataStore.StoreData(); // write to disk
        }

        public void Reset()
        {
            score = 0;
            moves = 0;
            time = 0;
        }
    }
}
