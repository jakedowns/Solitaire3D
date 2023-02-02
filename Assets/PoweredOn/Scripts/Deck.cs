using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.PlayingCards;
using UnityEngine;

namespace PoweredOn
{
    public class Deck
    {
        public List<Card> cards;
        Dictionary<SuitRank, int> cardIndexLookup;
        GameObject m_deckOfCards;
        public CardList deckOrderList;
        List<List<ShuffleMove>> shuffleLog;

        public Deck(GameObject deckOfCards, DebugOutput m_DebugOutput)
        {
            m_deckOfCards = deckOfCards;
            // Instantiate in-memory cards
            cards = new List<Card>();
            shuffleLog = new List<List<ShuffleMove>>();
            cardIndexLookup = new Dictionary<SuitRank, int>();
            deckOrderList = new CardList();

            // for 4 suits, and 13 ranks, create 52 cards
            int deckOrder = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Card newCard = new Card((Suit)i, (Rank)j, deckOrder);
                    cards.Add(newCard);

                    cardIndexLookup.Add(newCard.GetSuitRank(), deckOrder);

                    deckOrderList.Add(newCard.GetSuitRank());

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

                    // remove any existing CardInteractive components
                    CardInteractive[] prevCardInteractives = child.GetComponents<CardInteractive>();
                    foreach (CardInteractive prevCardInteractive in prevCardInteractives)
                    {
                        GameObject.DestroyImmediate(prevCardInteractive);
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
            shuffleLog.Clear();
            for (int i = 0; i < numShuffles; i++)
            {
                Shuffle();
            }
        }

        public struct ShuffleMove {

            public SuitRank suitRank;
            public int from;
            public int to;
            public ShuffleMove(SuitRank suitRank, int from, int to)
            {
                this.suitRank = suitRank;
                this.from = from;
                this.to = to;
            }
        }

        public void Shuffle()
        {
            
            CardList prevDeckOrderList = deckOrderList.Count > 0 ? deckOrderList : new CardList(PLAYING_CARD_DEFAULTS.DEFAULT_DECK_ORDER);

            deckOrderList = new CardList(PLAYING_CARD_DEFAULTS.DEFAULT_DECK_ORDER);
            List<ShuffleMove> iteration_log = new List<ShuffleMove>();
            for (int j = 0; j < cards.Count; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, cards.Count);
                SuitRank temp = prevDeckOrderList[j];
                SuitRank randomTemp = prevDeckOrderList[randomIndex];
                deckOrderList[j] = randomTemp;
                deckOrderList[randomIndex] = temp;

                // record the moves so we can animate them
                iteration_log.Add(new ShuffleMove(temp, j, randomIndex));
                iteration_log.Add(new ShuffleMove(randomTemp, randomIndex, j));
                
                UpdateCardDeckOrder(cards[j], j);
                UpdateCardDeckOrder(cards[randomIndex], randomIndex);               
            }
            shuffleLog.Add(iteration_log);
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
