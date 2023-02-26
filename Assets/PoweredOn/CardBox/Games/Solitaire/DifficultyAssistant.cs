using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.Random;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class DifficultyAssistant
    {
        private int difficulty = 3;
        private List<SuitRank> unseenPool = new List<SuitRank>();

        public void FlagSeen(SuitRank id)
        {
            if (unseenPool.Contains(id))
            {
                unseenPool.Remove(id);
            }
        }

        public DifficultyAssistant()
        {
            // initialize pool
            ResetPool();
        }

        public List<SuitRank> GetStackedDeck()
        {
            List<SuitRank> allCards = new(SolitaireDeck.DEFAULT_DECK_ORDER);

            /*
                * When "dealing" cards in the game, we start with the first card and deal across the tableaus in 7 passes.
                * When "stacking" the deck, we use the game difficulty setting (0-10) to determine how closely to follow a specific ordering of cards that makes the game more winnable. Specifically, we:
                    * Keep Aces towards the top of the tableaus or in the stockpile if there are no more top spots available.
                    * Keep low cards (<7) towards the top of the tableaus or in the stockpile.
                    * Keep high cards (7+) towards the bottom of the tableaus and in descending order.
                    * If there are still spots available in the stockpile, we add Kings, Queens, and Jacks there.
                * The higher the game difficulty, the less closely we follow the specific ordering and the more randomly we shuffle the deck. At maximum difficulty (10), the deck is completely randomly shuffled.
            */

            // IDEAL ORDER:
            List<SuitRank> idealOrder = new List<SuitRank>(allCards);
            int[] tableauPositions = new int[7];
            int[] tableauLengths = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
            for (int i = 0; i < allCards.Count; i++)
            {
                SuitRank card = allCards[i];
                /*int columnIndex = i % 7;
                int rowIndex = i / 7;
                int position = tableauPositions[columnIndex];
                if (card.rank == Rank.ACE && position < tableauLengths[columnIndex])
                {
                    idealOrder.Insert(rowIndex * 7 + columnIndex, card);
                    position++;
                    tableauPositions[columnIndex] = position;
                }
                else if (card.rank < Rank.SEVEN && position < tableauLengths[columnIndex])
                {
                    idealOrder.Insert(rowIndex * 7 + columnIndex, card);
                    position++;
                    tableauPositions[columnIndex] = position;
                }
                else if (card.rank >= Rank.SEVEN && position < tableauLengths[columnIndex])
                {
                    int index = (rowIndex + tableauLengths[columnIndex] - 1) * 7 + columnIndex;
                    idealOrder.Insert(index, card);
                }
                else
                {
                    idealOrder.Add(card);
                }*/
            }

            Debug.LogWarning("ideal order: " + idealOrder);

            List<SuitRank> stackedDeck = new List<SuitRank>(idealOrder);

            // Apply perturbation based on difficulty setting
            float perturbationRate = (10f - difficulty) / 10f;
            for (int i = 0; i < stackedDeck.Count; i++)
            {
                if (UnityEngine.Random.value < perturbationRate)
                {
                    int j = UnityEngine.Random.Range(i + 1, stackedDeck.Count);
                    SuitRank temp = stackedDeck[i];
                    stackedDeck[i] = stackedDeck[j];
                    stackedDeck[j] = temp;
                }
            }

            Debug.LogWarning("stacked order: " + idealOrder);

            return stackedDeck;
        }

        public void ResetPool()
        {
            unseenPool = new List<SuitRank>(SolitaireDeck.DEFAULT_DECK_ORDER);
        }

        public void SetDifficulty(int newDifficulty)
        {
            difficulty = Mathf.Clamp(newDifficulty, 0, 10);
        }

        public SuitRank GetNextMostHelpfulCard(SolitaireGameState gameState)
        {
            UnityEngine.Random.InitState((int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            List<SuitRank> candidates = new List<SuitRank>();
            for (int i = 0; i < 4; i++)
            {
                candidates.AddRange(gameState.FoundationPileGroup[i].GetNextValidCards());
            }
            for (int i = 0; i < 7; i++)
            {
                candidates.AddRange(gameState.TableauPileGroup[i].GetNextValidCards());
            }
            candidates.RemoveAll(c => !unseenPool.Contains(c));
            if (candidates.Count == 0)
            {
                return SuitRank.NONE;
            }
            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            SuitRank card = candidates[randomIndex];
            FlagSeen(card);
            return card;
        }
    }
}
