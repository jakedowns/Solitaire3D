using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.Managers;
using PoweredOn.PlayingCards;
using UnityEngine;
using UnityEngine.Assertions;

namespace PoweredOn
{
    internal class Solitaire3DTests
    {
        private SolitaireGame game;
        private DebugOutput m_DebugOutput;
        public void TestDeckManager()
        {
            m_DebugOutput = GameObject.Find("DebugOutput").GetComponent<DebugOutput>();
            game = new SolitaireGame();
        }

        public void Run()
        {
            //try { 
                CanCreateDeck();
                DeckCanBeShuffled();
                CanDealCards();
                CheckCanCycleThroughStockToWasteAndBack();
                CheckValidPickUpMoves();
            //}catch(Exception e)
            //{
            //    Debug.LogError(e);
            //    m_DebugOutput.LogError("error with test run: " + e.ToString());
            //}
        }
        
        public void CanCreateDeck()
        {
            game = new SolitaireGame();
            game.NewGame();
            game.m_DebugOutput.LogWarning("deck count " + game.deck.cards.Count);
        }

        private bool VerifyDeckIsNotInDefaultOrder()
        {
            CardList DEFAULT_DECK_ORDER = PLAYING_CARD_DEFAULTS.DEFAULT_DECK_ORDER;
            bool orderBroken = false;
            int i = 0;
            while(i<DEFAULT_DECK_ORDER.Count && !orderBroken)
            {
                SuitRank a = DEFAULT_DECK_ORDER[i];
                SuitRank b = game.deck.deckOrderList[i];
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
            return orderBroken;
        }

        private bool VerifyDeckIsInDefaultOrder()
        {
            CardList DEFAULT_DECK_ORDER = PLAYING_CARD_DEFAULTS.DEFAULT_DECK_ORDER;
            int orderIndex = 0;
            if(game == null || game.deck == null)
            {
                throw new Exception($"verify deck failed to find game.deck");
            }
            foreach (SuitRank suitRank in DEFAULT_DECK_ORDER)
            {
                Card card = game.deck.GetCardBySuitRank(suitRank);
                if (card.GetDeckOrder() != orderIndex)
                {
                    throw new Exception($"bad card deck index expected {orderIndex} got {card.GetDeckOrder()}");
                }

                int i = game.deck.deckOrderList.IndexOf(suitRank);
                if (i != orderIndex)
                {
                    throw new Exception($"bad deckOrderList index for card. expected {orderIndex} got {i}");
                }

                orderIndex++;
            }
            return true;
        }

        public void DeckCanBeShuffled()
        {
            game = new SolitaireGame();
            game.NewGame();

            // 1. expect the deck to be in initial deck order
            bool verified_start = VerifyDeckIsInDefaultOrder();
            Debug.Log("verified_start "+verified_start);
            
            if (verified_start)
            {
                Debug.Log("successfully verified new deck is in DEFAULT_DECK_ORDER!");
            }

            // 2. shuffle the deck
            game.deck.Shuffle(1);

            // 3. expect the deck to be in a random order (NOT initial deck order)
            bool verified_end = VerifyDeckIsNotInDefaultOrder();

            if (!verified_end)
            {
                throw new Exception($"bad deckOrderList. deck is still in DEFAULT_DECK_ORDER after calling Shuffle(1)!");
            }
            else
            {
                Debug.Log("successfully verified new deck is shuffled out of DEFAULT_DECK_ORDER!");
            }

        }

        public void CanDealCards()
        {
            Debug.Log("[TEST] CanDealCards");

            game = new SolitaireGame();
            game.NewGame();
            game.Deal();

            // 1 + 2 + 3 + 4 + 5 + 6 + 7 = 28 INITIAL_TABLEAU_COUNT_AFTER_DEAL
            // 52 - 28 = 24 INITIAL_STOCK_COUNT_AFTER_DEAL

            // expect "dealtOrder" to be 28 cards
            CardList dealtOrder = game.GetDealtOrderImmutable();
            Debug.Log($"expect dealtOrder list to be 28 cards / got {dealtOrder.Count}");
            Assert.IsTrue(dealtOrder.Count == 28);

            // 1. expect there to be 24 cards in the Stock pile
            //using (CardList stockPile = game.GetStockCardsImmutable())
            //{
                CardList stockPile = game.GetStockCardsImmutable();
                Debug.Log($"stock card count = {stockPile.Count} / expected 24");
                Assert.IsTrue(stockPile.Count == 24);
            //}

            // 2. expect there to be 1-7 cards in each tableau
            CardListList tabList = game.GetTableauListImmutable();
            for (int i = 0; i < tabList.Count; i++){
                Debug.Log($"tableau {i} card count = {tabList[i].Count} / expected {i + 1}");
                Assert.IsTrue(tabList[i].Count == i + 1);
            }

            // 3. expect there to be 0 cards in the waste pile
            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 0);

            // 4. expect there to be 0 cards in the foundation piles
            CardListList foundationList = game.GetFoundationCardsImmutable();
            foreach(CardList foundation in foundationList)
            {
                Assert.IsTrue(foundation.Count == 0);
            }
        }

        public void CheckCanCycleThroughStockToWasteAndBack()
        {
            Debug.Log("[TEST] CheckCanCycleThroughStockToWasteAndBack");

            game = new SolitaireGame();
            game.NewGame();
            game.Deal();

            // 1. expect there to be 24 cards in the Stock pile
            CardList stockPile = game.GetStockCardsImmutable();
            Debug.Log($"stock card count = {stockPile.Count} / expected 24");
            Assert.IsTrue(stockPile.Count == 24);

            // 2. expect there to be 0 cards in the waste pile
            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 0);

            for(var i = 0; i < 24; i++)
            {
                // 3. expect there to be 1 more card in the waste pile after clicking top stock card
                game.StockToWaste();
                Assert.IsTrue(game.GetWasteCardsImmutable().Count == i + 1);

                // 4. expect there to be 1 less card in the stock pile after clicking top stock card
                Assert.IsTrue(game.GetStockCardsImmutable().Count == 24 - (i + 1));
            }

            // 5. expect there to be 0 cards in the waste pile after cycling through waste
            game.WasteToStock();
            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 0);

            // 6. expect there to be 24 cards in the stock pile after cycling through waste
            Assert.IsTrue(game.GetStockCardsImmutable().Count == 24);
        }

        public void CheckValidPickUpMoves()
        {
            
        }

        public void CheckInvalidPickUpMoves()
        {
            
        }

        public void CheckValidPlaceMoves()
        {
            
        }

        public void CheckInvalidPlaceMoves()
        {
            
        }

        public void TestResetWasteToStock()
        {
            
        }
    }
}
