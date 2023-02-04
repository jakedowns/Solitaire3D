using PoweredOn.CardBox.Cards;
using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireDeck
    {
        public List<SolitaireCard> cards;
        Dictionary<SuitRank, int> cardIndexLookup;
        GameObject m_deckOfCards;
        public PlayingCardIDList deckOrderList;
        List<List<ShuffleMove>> shuffleLog;

        public static PlayingCardIDList DEFAULT_DECK_ORDER
        {
            get
            {
                PlayingCardIDList defaultDeckOrder = new PlayingCardIDList();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        defaultDeckOrder.Add(new SuitRank((Suit)i, (Rank)j));
                    }
                }
                return defaultDeckOrder;
            }
        }

        public SolitaireDeck()
        {
            // Instantiate in-memory cards
            cards = new List<SolitaireCard>();
            shuffleLog = new List<List<ShuffleMove>>();
            cardIndexLookup = new Dictionary<SuitRank, int>();
            deckOrderList = new PlayingCardIDList();

            // for 4 suits, and 13 ranks, create 52 cards
            int deckOrder = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    SolitaireCard newCard = new SolitaireCard((Suit)i, (Rank)j, deckOrder);
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
                        DebugOutput.Instance.LogError("child not found " + gameObjectName);
                        continue;
                    }

                    // remove any existing MonoSolitaireCard components
                    MonoSolitaireCard[] prevCards = child.GetComponents<MonoSolitaireCard>();
                    foreach (MonoSolitaireCard prevCard in prevCards)
                    {
                        GameObject.DestroyImmediate(prevCard);
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

        public SuitRank GetCardIDByIndex(int index)
        {
            return this.deckOrderList.ElementAtOrDefault(index);
        }

        public SolitaireCard GetCardBySuitRank(SuitRank suitrank)
        {
            int cardIndex = cardIndexLookup[suitrank];
            return cards[cardIndex];
        }

        public int GetCardIndex(SuitRank suitRank)
        {
            return cardIndexLookup[suitRank];
        }

        public int Count
        {
            get
            {
                return this.deckOrderList.Count;
            }
        }

        public void Shuffle(int numShuffles)
        {
            shuffleLog.Clear();
            for (int i = 0; i < numShuffles; i++)
            {
                Shuffle();
            }
        }

        public struct ShuffleMove
        {

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
            PlayingCardIDList prevDeckOrderList = deckOrderList.Count > 0 ? deckOrderList : new PlayingCardIDList(DEFAULT_DECK_ORDER);

            deckOrderList = new PlayingCardIDList(prevDeckOrderList);
            List<ShuffleMove> iteration_log = new List<ShuffleMove>();
            for (int j = 0; j < 52; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 52);
                SuitRank temp = deckOrderList[j];
                deckOrderList[j] = deckOrderList[randomIndex];
                deckOrderList[randomIndex] = temp;

                // record the moves so we can animate them
                iteration_log.Add(new ShuffleMove(temp, j, randomIndex));
                iteration_log.Add(new ShuffleMove(deckOrderList[randomIndex], randomIndex, j));

                UpdateCardDeckOrder(cards[j], j);
                UpdateCardDeckOrder(cards[randomIndex], randomIndex);
            }
            if (deckOrderList.Count != 52)
            {
                throw new Exception("invalid deckOrderList count after shuffle");
            }
            shuffleLog.Add(iteration_log);
        }

        void UpdateCardDeckOrder(SolitaireCard card, int deckOrder)
        {
            // record the updated order within the cards themselves
            card.SetDeckOrder(deckOrder);
            // update the indexLookup table
            cardIndexLookup[card.GetSuitRank()] = deckOrder;
        }

        public void SetCardsToDefaultSortOrder()
        {
            PlayingCardIDList deckOrderListNext = new PlayingCardIDList();
            // loop through the suits and ranks, and set the deck order of each card to match the relative order within the loop
            int deckOrder = 0;

            PlayingCardIDList DEFAULT_DECK_ORDER = SolitaireDeck.DEFAULT_DECK_ORDER;

            /*foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {*/
            foreach (SuitRank cardID in DEFAULT_DECK_ORDER)
            {
                int currentCardIndex = GetCardIndex(cardID);
                SolitaireCard card = cards[currentCardIndex];
                deckOrderListNext.Add(cardID);

                UpdateCardDeckOrder(card, deckOrder);

                deckOrder++;
            }
            /*}
        }*/
            deckOrderList = deckOrderListNext;
        }

        public static int[][] SuitsByColor
        {
            get
            {
                return new int[2][] {
                    new int[2] { (int)Suit.CLUBS, (int)Suit.SPADES },
                    new int[2] { (int)Suit.DIAMONDS, (int)Suit.HEARTS }
                };
            }
        }

        public static bool SuitColorsAreOpposite(Suit suitA, Suit suitB)
        {
            int[][] suitsByColor = SuitsByColor;
            int[] blackSuits = suitsByColor[0];
            int[] redSuits = suitsByColor[1];
            if (
                blackSuits.Contains((int)suitA) && redSuits.Contains((int)suitB)
                || redSuits.Contains((int)suitA) && blackSuits.Contains((int)suitB)
            )
            {
                return true;
            }
            return false;
        }

        public SuitRank ElementAtOrDefault(int index)
        {
            return this.deckOrderList
                .DefaultIfEmpty(SuitRank.NONE)
                .ElementAtOrDefault(index);
        }

        public bool IsInDefaultSortOrder()
        {
            bool orderBroken = false;
            int i = 0;
            while (i < DEFAULT_DECK_ORDER.Count && !orderBroken)
            {
                SuitRank a = DEFAULT_DECK_ORDER[i];
                SuitRank b = GetCardIDByIndex(i);
                if (
                    a.suit != b.suit
                    ||
                    a.rank != b.rank
                )
                {
                    orderBroken = true;
                    break;
                }
                i++;
            }
            return !orderBroken;
        }
    }
}
