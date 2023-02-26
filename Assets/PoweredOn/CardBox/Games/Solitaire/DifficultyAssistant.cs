using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using static System.Random;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class DifficultyAssistant
    {
        private GameObject difficultySlider;
        public int difficulty { get; private set; } = 0;
        public bool useJITHelper { get; private set; } = false;
        private List<SuitRank> unseenPool = new List<SuitRank>();

        public void FlagSeen(SuitRank id)
        {
            if (unseenPool.Contains(id))
            {
                unseenPool.Remove(id);
            }
        }

        public void SetJITHelperEnabled(bool enabled)
        {
            useJITHelper = enabled;
        }

        public void SetDifficultySliderGameObject(GameObject go)
        {
            difficultySlider = go;
        }

        public DifficultyAssistant()
        {
            // initialize pool
            ResetPool();
        }

        // an alternative "ideal" order where we make sure that all "low" cards (<7) are in the tableau (first 28 cards in the order)
        public List<SuitRank> GetIdealOrder()
        {
            List<SuitRank> startingList = new(SolitaireDeck.DEFAULT_DECK_ORDER);
            List<SuitRank> outputList = new();
            foreach(var id in startingList)
            {
                if((int)id.rank < 7)
                {
                    // add to front of outputList
                    outputList.Insert(0,id);
                }
                else
                {
                    // add to end of outputList
                    outputList.Add(id);
                }
            }
            Assert.IsTrue(outputList.Count == 52, $"expected 52, got {outputList.Count}");
            return outputList;
        }

        // a different "ideal order" where we deal 2-ish suits in A->K order so they can be move directly to the foundations from the tableau

        public List<SuitRank> GetIdealOrderTwo()
        {
            //List<SuitRank> allCards = new(SolitaireDeck.DEFAULT_DECK_ORDER);
            List<SuitRank> idealOrder = new List<SuitRank>();
            // pick 3 random suits to be our tableau suits
            // we stack them in the deck so they are dealt in a perfect
            // "winnable" order (then we perturb the order based on difficulty)
            List<Suit> suits = new List<Suit> {
                Suit.CLUBS,
                Suit.DIAMONDS,
                Suit.HEARTS,
                Suit.SPADES
            };

            List<Suit> randomSuits = new(suits);
            // drop one at random and shuffle the list
            int removedSuit = UnityEngine.Random.Range(0, randomSuits.Count);
            randomSuits.RemoveAt(removedSuit);
            for (int i = 0; i < randomSuits.Count; i++)
            {
                // fischer-yates in-place shuffle
                int j = UnityEngine.Random.Range(i + 1, randomSuits.Count);
                j = Mathf.Clamp(0, randomSuits.Count - 1, j);
                Suit temp = randomSuits[i];
                randomSuits[i] = randomSuits[j];
            }

            
            List<SuitRank> cardsForTableau = new();
            int suit_index = 0;
            int rank_counter = 0;
            for (int i = 0; i < 28; i++)
            {
                Suit suit = randomSuits[suit_index];
                Rank rank = (Rank)rank_counter;
                cardsForTableau.Add(new SuitRank(suit, rank));
                rank_counter++;
                if (rank_counter >= 13)
                {
                    rank_counter = 0;
                    suit_index++;
                }
            }
            
            for (int row = 0; row < 7; row++)
            {
                // since we deal horizontally, we need to do the following to stack our deck order:
                //     | 1     | 2    | 3    | 4     | 5    | 6    | 7    |
                // a   | S1_A  | S1_3 | S1_6 | S1_10 | S2_2 | S2_8 | S3_2
                // b   | -     | S1_2 | S1_5 | S1_9  | S2_A | S2_7 | S3_A
                // c   | -     | -    | S1_4 | S1_8  | S1_K | S2_6 | S2_K
                // d   | -     | -    | -    | S1_7  | S1_Q | S2_5 | S2_Q
                // e   | -     | -    | -    | -     | S1_J | S2_4 | S2_J
                // f   | -     | -    | -    | -     | -    | S2_3 | S2_10
                // g   | -     | -    | -    | -     | -    | -    | S2_9


                // 0     +2    +3    +4     +5    +6    +7
                // 0,    2,    5,    9,     14,   20,   27
                // -,    1,    4,    8,     13,   19,   26
                // -,    -,    3,    7,     12,   18,   25
                // -,    -,    -,    6,     11,   17,   24
                // -,    -,    -,    -,     10,   16,   23
                // -,    -,    -,    -,     -,    15,   22
                // -,    -,    -,    -,     -,    -,    21

                for (int col = 0; col < 7; col++)
                {
                    if(col < row)
                    {
                        continue;
                    }
                    // 0,   1,  3,  6, 10, 15, 21
                    // +0, +1, +2, +3, +4, +5, +6
                    int rowStart = row;
                    int colOffset = col == 0 ? 0 : col + 2;
                    int lookupIndex = rowStart + colOffset;
                    idealOrder.Add(cardsForTableau[lookupIndex]);
                }
            }

            // round out the list by adding the other 24 cards from the deck from the removed suit, and the remaining 11 cards from suit 3 (randomSuits[2])
            for(var i = 0; i < 11; i++)
            {
                idealOrder.Add(new SuitRank(randomSuits[2], (Rank)i + 2));
            }
            for(var i = 0; i < 13; i++)
            {
                idealOrder.Add(new SuitRank(suits[removedSuit], (Rank)i));
            }

            Assert.IsTrue(idealOrder.Count == 52);
            return idealOrder;
        }

        public List<SuitRank> GetStackedDeck()
        {
            //List<SuitRank> allCards = new(SolitaireDeck.DEFAULT_DECK_ORDER);

            /*
                * When "dealing" cards in the game, we start with the first card and deal across the tableaus in 7 passes.
                * When "stacking" the deck, we use the game difficulty setting (0-10) to determine how closely to follow a specific ordering of cards that makes the game more winnable. Specifically, we:
                    * Keep Aces towards the top of the tableaus or in the stockpile if there are no more top spots available.
                    * Keep low cards (<7) towards the top of the tableaus or in the stockpile.
                    * Keep high cards (7+) towards the bottom of the tableaus and in descending order.
                    * If there are still spots available in the stockpile, we add Kings, Queens, and Jacks there.
                * The higher the game difficulty, the less closely we follow the specific ordering and the more randomly we shuffle the deck. At maximum difficulty (10), the deck is completely randomly shuffled.
            */

            Debug.Log($"[DifficultyAssistant] GetStackedDeck. Current Difficulty: {difficulty}");

            // IDEAL ORDER:
            List<SuitRank> idealOrder;
            // 50/50 chance of using GetIdealOrder or GetIdealOrderTwo
            float idealProb = UnityEngine.Random.value;
            Debug.Log($"idealProb:{idealProb}");
            if (idealProb > 0.5f)
            {
                Debug.Log("[DiffAssist] using ideal one");
                idealOrder = GetIdealOrder();
            }
            else
            {
                Debug.Log("[DiffAssist] using ideal two");
                idealOrder = GetIdealOrderTwo();
            }

            Debug.LogWarning("[DiffAssist] ideal order: " + idealOrder);

            List<SuitRank> stackedDeck = new List<SuitRank>(idealOrder);

            // Apply perturbation based on difficulty setting
            if (difficulty > 0)
            {
                float perturbationRate = (10f - difficulty) / 10f;
                Debug.Log("[DiffAssist] perturbation rate: " + perturbationRate + " (10 - " + difficulty + "/10)");

                for (int i = 0; i < stackedDeck.Count; i++)
                {
                    if (UnityEngine.Random.value < perturbationRate)
                    {
                        // if use probabilistic perturbation
                        int j = UnityEngine.Random.Range(i + 1, stackedDeck.Count);
                        // else, use a fixed count, like:
                        // int numToSwap = Math.Round(perturbRate / 52)
                        // get a sampling of numToSwap cards at random
                        // then, loop over the stackedDeck and swap the sampled cards

                        /*if (j < 0 || j >= stackedDeck.Count)
                        {
                            Debug.LogWarning("[DeckStacker] invalid j : " + j + ", " + stackedDeck.Count);
                        }*/
                        j = Mathf.Clamp(0, stackedDeck.Count - 1, j);
                        SuitRank temp = stackedDeck[i];
                        stackedDeck[i] = stackedDeck[j];
                        stackedDeck[j] = temp;
                    }
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

        public void UpdateDifficultyText()
        {
            if(difficultySlider != null)
            {
                var controller = difficultySlider.GetComponent<DifficultyController>();
                controller.UpdateDifficulty(difficulty);
            }
        }

        public SuitRank GetNextMostHelpfulCard(SolitaireGameState gameState)
        {
            Debug.Log($"[DifficultyAssistant] GetNextMostHelpfulCard. Current Difficulty: {difficulty}");
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
