using PoweredOn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PoweredOn.PlayingCards;
using UnityEngine;

namespace PoweredOn
{
    public class Deck
    {
        public List<Card> cards;
        Dictionary<SuitRank, int> cardIndexLookup;
        GameObject m_deckOfCards;
        List<List<int>> shuffleLog;

        public Deck(GameObject deckOfCards, DebugOutput m_DebugOutput)
        {
            m_deckOfCards = deckOfCards;
            // Instantiate in-memory cards
            cards = new List<Card>();
            shuffleLog = new List<List<int>>();
            cardIndexLookup = new Dictionary<SuitRank, int>();

            // for 4 suits, and 13 ranks, create 52 cards
            int deckOrder = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Card newCard = new Card((Suit)i, (Rank)j, deckOrder);
                    cards.Add(newCard);

                    cardIndexLookup.Add(newCard.GetSuitRank(), deckOrder);

                    deckOrder++;

                    // generate a string of the game object name based on the current suit & rank
                    // ex: "ace_of_clubs" or "king_of_spades" or "2_of_diamonds"
                    string gameObjectName = newCard.GetGameObjectName();

                    Transform child = deckOfCards.transform.Find(gameObjectName);
                    if (!child)
                    {
                        m_DebugOutput.LogError("child not found " + gameObjectName);
                        continue;
                    }
                    child.gameObject.AddComponent(typeof(CardInteractive));

                    CardInteractive cardInteractive = child.GetComponent<CardInteractive>();

                    // call SetCard on the matching child's cardInteractive component
                    cardInteractive.SetCard(newCard);

                    // link the cardInteractive component to the card
                    // needs to be done AFTER attaching the cardInteractive component to the child
                    newCard.SetCardInteractive(cardInteractive);
                }
            }
        }

        public Card GetCardBySuitRank(SuitRank suitrank)
        {
            int cardIndex = cardIndexLookup[suitrank];
            return cards[cardIndex];
        }

        public int GetCardIndex(SuitRank suitRank)
        {
            return cardIndexLookup[suitRank];
        }

        public void Shuffle(int numShuffles)
        {
            for (int i = 0; i < numShuffles; i++)
            {
                Shuffle();
            }
        }

        // TODO: make this shuffle deckOrder instead of the List<Card> cards list
        public void Shuffle()
        {
            shuffleLog.Clear();
            //List<SuitRank> newDeckOrder = new List<SuitRank>(52);
            List<int> iteration_log = new List<int>();
            for (int j = 0; j < cards.Count; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, cards.Count);
                Card temp = cards[j];
                cards[j] = cards[randomIndex];
                cards[randomIndex] = temp;
                UpdateCardDeckOrder(cards[j], j);
                UpdateCardDeckOrder(cards[randomIndex], randomIndex);
                iteration_log.Add(randomIndex);
            }
            shuffleLog.Add(iteration_log);
            //deckOrder = newDeckOrder;
        }
        
        void UpdateCardDeckOrder(Card card, int deckOrder)
        {
            // record the updated order within the cards themselves
            card.SetDeckOrder(deckOrder);
            // update the indexLookup table
            cardIndexLookup[card.GetSuitRank()] = deckOrder;
        }

        public void SetCardsToDefaultSortOrder()
        {
            List<Card> cardListNext = new List<Card>();
            // loop through the suits and ranks, and set the deck order of each card to match the relative order within the loop
            int deckOrder = 0;
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    int currentCardIndex = GetCardIndex(new SuitRank(suit, rank));
                    Card card = cards[currentCardIndex];
                    cardListNext.Add(card);

                    UpdateCardDeckOrder(card, deckOrder);

                    deckOrder++;
                }
            }
            cards = cardListNext;
        }

        public static bool SuitsAreOpposite(Suit suitA, Suit suitB)
        {
            if (
                (int)suitA < 2 && (int)suitB > 1
                || (int)suitA > 1 && (int)suitB < 2)
            {
                return true;
            }
            return false;
        }
    }
}
