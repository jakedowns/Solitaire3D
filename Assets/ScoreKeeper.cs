using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class ScoreKeeper
    {
        private int _10_sec_penalty_counter = 0;
        public int score { get; private set; } = 0;
        public int moves { get; private set; } = 0;
        public int time { get; private set; } = 0;

        public int best_score { get; private set; } = 0;
        public int best_time { get; private set; } = int.MaxValue;
        public int best_moves { get; private set; } = int.MaxValue;

        private ScoreDisplay scoreDisplay;
        private Text bestScoreDisplayText;

        public ScoreKeeper()
        {
            this.scoreDisplay = GameObject.Find("ScoreDisplay").GetComponent<ScoreDisplay>();
            bestScoreDisplayText = GameObject.Find("BestScoreDisplay").GetComponent<Text>();
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

        public void RecordScore(int _score)
        {
            this.score += _score;

            // prevent score from going below 0
            // TODO: keep a second internal score for the AI to see?
            this.score = Math.Max(0, this.score);

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

        public string GetBestTimeFormatted()
        {
            // convert int time (seconds) in MM:SS format:
            TimeSpan t = TimeSpan.FromSeconds(best_time);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
        }

        public void Tick()
        {
            // increase by 1 second
            time++;
            _10_sec_penalty_counter++;
            if(_10_sec_penalty_counter >= 10)
            {
                // apply 10 second penalty
                RecordScore((int)SolitaireMoveTypeScores.PENALTY_PER_10_SECONDS_ELAPSED);
                _10_sec_penalty_counter = 0;
            }
            
            scoreDisplay.UpdateText(this);
            if (PoweredOn.Managers.GameManager.Instance == null)
            {
                Debug.LogError("cannot save score, no game manager instance");
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

        internal void UpdateBestScoreDisplay()
        {
            string text = $"Best Score: {best_score} | Best Time: {GetBestTimeFormatted()} | Best Moves: {best_moves}";
            bestScoreDisplayText.text = text;
        }

        internal void CalculateFinalScore()
        {
            if (PoweredOn.Managers.GameManager.Instance == null)
            {
                Debug.LogError("cannot CalculateFinalScore, no game manager instance");
                return;
            }
            
            if (score > best_score)
            {
                best_score = score;
            }   
            
            if(time < best_time)
            {
                best_time = time;
            }

            if(moves < best_moves)
            {
                best_moves = moves;
            }

            UpdateBestScoreDisplay();
            PoweredOn.Managers.GameManager.Instance.dataStore.UpdateScoreData(PoweredOn.Managers.GameManager.Instance.game);
            PoweredOn.Managers.GameManager.Instance.dataStore.StoreData();
        }
    }
}
