using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Cards;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public static class SolitaireGameTests
    {

        public static void Run()
        {
            CanCreateDeck();
            DeckCanBeShuffled();
            CanDealCards();
            CheckCanCycleThroughStockToWasteAndBack();
            TestStateMachineTruthTableForCardMovement();
        }

        public static void CanCreateDeck()
        {
            var game = new SolitaireGame();
            PlayingCardDeck deck = game.BuildDeck();
            Assert.IsTrue(deck.Count == 52);
        }

        public static void DeckCanBeShuffled()
        {
            var game = new SolitaireGame();
            game.NewGame();

            // 1. expect the deck to be in initial deck order
            Assert.IsTrue(game.deck.IsInDefaultSortOrder());

            // 2. shuffle the deck
            game.deck.Shuffle(1);

            // 3. expect the deck to be in a random order (NOT initial deck order)
            Assert.IsFalse(game.deck.IsInDefaultSortOrder());

        }

        public static void CanDealCards()
        {
            Debug.Log("[TEST] CanDealCards");

            var game = new SolitaireGame();
            game.NewGame();
            game.Deal();

            // 1 + 2 + 3 + 4 + 5 + 6 + 7 = 28 INITIAL_TABLEAU_COUNT_AFTER_DEAL
            // 52 - 28 = 24 INITIAL_STOCK_COUNT_AFTER_DEAL

            // expect "dealtOrder" to be 28 cards
            PlayingCardIDList dealtOrder = game.GetDealtOrderImmutable();
            Debug.Log($"expect dealtOrder list to be 28 cards / got {dealtOrder.Count}");
            Assert.IsTrue(dealtOrder.Count == 28);

            // 1. expect there to be 24 cards in the Stock pile
            //using (CardIDList stockPile = game.GetStockCardsImmutable())
            //{
            PlayingCardIDList stockPile = game.GetStockCardsImmutable();
            Debug.Log($"stock card count = {stockPile.Count} / expected 24");
            Assert.IsTrue(stockPile.Count == 24);
            //}

            // 2. expect there to be 1-7 cards in each tableau
            PlayingCardIDListGroup tabList = game.GetTableauListImmutable();
            for (int i = 0; i < tabList.Count; i++)
            {
                Debug.Log($"tableau {i} card count = {tabList[i].Count} / expected {i + 1}");
                Assert.IsTrue(tabList[i].Count == i + 1);
            }

            // 3. expect there to be 0 cards in the waste pile
            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 0);

            // 4. expect there to be 0 cards in the foundation piles
            PlayingCardIDListGroup foundationList = game.GetFoundationCardsImmutable();
            foreach (PlayingCardIDList foundation in foundationList)
            {
                Assert.IsTrue(foundation.Count == 0);
            }
        }

        public static void CheckCanCycleThroughStockToWasteAndBack()
        {
            Debug.Log("[TEST] CheckCanCycleThroughStockToWasteAndBack");

            var game = new SolitaireGame();
            game.NewGame();
            game.Deal();

            // 1. expect there to be 24 cards in the Stock pile
            CardIDList stockPile = game.GetStockCardsImmutable();
            Debug.Log($"stock card count = {stockPile.Count} / expected 24");
            Assert.IsTrue(stockPile.Count == 24);

            // 2. expect there to be 0 cards in the waste pile
            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 0);

            for (var i = 0; i < 24; i++)
            {
                // 3. expect there to be 1 more card in the waste pile after clicking top stock card
                game.StockToWaste();
                Assert.IsTrue(game.GetWasteCardsImmutable().Count == i + 1);

                // 4. expect there to be 1 less card in the stock pile after clicking top stock card
                Assert.IsTrue(game.GetStockCardsImmutable().Count == 24 - (i + 1));
            }

            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 24);
            Assert.IsTrue(game.GetStockCardsImmutable().Count == 0);
            Debug.Log($"[TEST] TestStockWaste confirmed 24 cards in waste pile and 0 cards in stock pile");

            // 5. expect there to be 0 cards in the waste pile after cycling through waste
            game.WasteToStock();
            Assert.IsTrue(game.GetWasteCardsImmutable().Count == 0);

            // 6. expect there to be 24 cards in the stock pile after cycling through waste
            Assert.IsTrue(game.GetStockCardsImmutable().Count == 24);

            Debug.Log($"[TEST] TestStockWaste confirmed 0 cards in waste pile and 24 in stock pile after cycling back to stock pile");
        }

        public static void TestStateMachineTruthTableForCardMovement()
        {
            /** 
             * In this test we do the following:
             * 
             *  for each of the 6 possible <CardIDList>`s (Piles): (Stock, Waste, Tableau, Foundation, Hand, Deck) SWTFHD
             * 
             *      then, for each of the MoveTypes:
             *      
             *          ### FromStockMoves:
             *          
             *          MoveType.StockToWaste       ValidWhen.HandIsEmpty
             *          MoveType.StockToAny         InvalidMove
             *      
             *          ### FromWasteMoves: 
             *          
             *          MoveType.WasteToStock       ValidWhen.StockIsEmpty
             *          MoveType.WasteToWaste       ValidWhen.WasteCanAcceptCard(CardIsReturning)
             *          MoveType.WasteToFoundation  ValidWhen.FoundationCanAcceptCard
             *          MoveType.WasteToTabealu     ValidWhen.TableauCanAcceptCard
             *          MoveType.WasteToHand        ValidWhen.HandIsEmpty
             *          MoveType.WasteToDeck        ValidWhen.IsCollectingDeck
             *          
             *          
             *          ### FromTableauMoves:
             *          
             *          MoveType.TableauToStock     ValidWhen.StockIsEmpty
             *          MoveType.TableauToWaste     ValidWhen.WasteIsEmpty
             *          MoveType.TableauToFoundation ValidWhen.FoundationCanAcceptCard
             *          MoveType.TableauToTableau   ValidWhen.TableauCanAcceptCard
             *          MoveType.TableauToHand      ValidWhen.HandIsEmpty
             *          MoveType.TableauToDeck      ValidWhen.IsCollectingDeck
             *          
             *          ### FromHandMoves:
             *      
             *          MoveType.HandToStock        InvalidMove
             *          MoveType.HandToWaste        ValidWhen.Returning
             *          MoveType.HandToTableau      ValidWhen.TableauCanAcceptCard(ValidNextTopCard|CardIsReturning)
             *          MoveType.HandToFoundation   ValidWhen.FoundationCanAcceptCard(ValidNextTopCard|CardIsReturning)
             *          MoveType.HandToHand         InvalidMove
             *          MoveType.HandToDeck         ValidWhen.IsCollectingDeck
             *          
             *          ### FromDeckMoves:
             *          
             *          MoveType.AnyToDeck          ValidWhen.IsCollectingDeck
             *          MoveType.DeckToStock        ValidWhen.IsDealing
             *          MoveType.DeckToTableau      ValidWhen.IsDealing
             * 
             **/
        }
    }
}
