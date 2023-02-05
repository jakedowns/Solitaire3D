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
            Test_StockWasteHand_HandEmpty_HandFull();
        }

        public static void CanCreateDeck()
        {
            var game = SolitaireGame.TestGame;
            SolitaireDeck deck = game.BuildDeck();
            Assert.IsTrue(deck.Count == 52);
        }

        public static void DeckCanBeShuffled()
        {
            var game = SolitaireGame.TestGame;
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

            var game = SolitaireGame.TestGame;
            game.NewGame();
            game.Deal();

            // assert deck.deckCardPile.Count == 0 (all cards have been dealt)
            Assert.IsTrue(game.deck.DeckCardPile.Count == 0);

            // 1 + 2 + 3 + 4 + 5 + 6 + 7 = 28 INITIAL_TABLEAU_COUNT_AFTER_DEAL
            // 52 - 28 = 24 INITIAL_STOCK_COUNT_AFTER_DEAL

            // expect "dealtOrder" to be 28 cards
            PlayingCardIDList dealtOrder = game.GetDealtOrder();
            Debug.Log($"expect dealtOrder list to be 28 cards / got {dealtOrder.Count}");
            Assert.IsTrue(dealtOrder.Count == 28);

            // 1. expect there to be 24 cards in the Stock pile
            StockCardPile stockPile = game.GetStockCardPile();
            Debug.Log($"stock card count = {stockPile.Count} / expected 24");
            Assert.IsTrue(stockPile.Count == 24);
            //}

            // 2. expect there to be 1..7 cards in each tableau pile
            TableauCardPileGroup tabList = game.GetTableauCardPileGroup();
            for (int i = 0; i < tabList.Count; i++)
            {
                Debug.Log($"tableau {i} card count = {tabList[i].Count} / expected {i + 1}");
                Assert.IsTrue(tabList[i].Count == i + 1);

                // 2.1 expect the top card ONLY of each tableau to be face up
                Assert.IsTrue(tabList[i].GetTopCard().IsFaceUp);
            }

            // 3. expect there to be 0 cards in the waste pile
            Assert.IsTrue(game.GetWasteCardPile().Count == 0);

            // 4. expect there to be 0 cards in the foundation piles
            FoundationCardPileGroup foundationList = game.GetFoundationCardPileGroup();
            foreach (FoundationCardPile foundation in foundationList)
            {
                Assert.IsTrue(foundation.Count == 0);
            }
        }

        public static void CheckCanCycleThroughStockToWasteAndBack()
        {
            Debug.Log("[TEST] CheckCanCycleThroughStockToWasteAndBack");

            var game = SolitaireGame.TestGame;
            game.NewGame();
            game.Deal();

            // 1. expect there to be 24 cards in the Stock pile
            StockCardPile stockPile = game.GetStockCardPile();
            Debug.Log($"stock card count = {stockPile.Count} / expected 24");
            Assert.IsTrue(stockPile.Count == 24);

            // 2. expect there to be 0 cards in the waste pile
            Assert.IsTrue(game.GetWasteCardPile().Count == 0);

            for (var i = 0; i < 24; i++)
            {
                // 3. expect there to be 1 more card in the waste pile after clicking top stock card
                game.StockToWaste();
                Assert.IsTrue(game.GetWasteCardPile().Count == i + 1);

                // 4. expect there to be 1 less card in the stock pile after clicking top stock card
                Assert.IsTrue(game.GetStockCardPile().Count == 24 - (i + 1));
            }

            Assert.IsTrue(game.GetWasteCardPile().Count == 24);
            Assert.IsTrue(game.GetStockCardPile().Count == 0);
            Debug.Log($"[TEST] TestStockWaste confirmed 24 cards in waste pile and 0 cards in stock pile");

            // 5. expect there to be 0 cards in the waste pile after cycling through waste
            game.WasteToStock();
            Assert.IsTrue(game.GetWasteCardPile().Count == 0);

            // 6. expect there to be 24 cards in the stock pile after cycling through waste
            Assert.IsTrue(game.GetStockCardPile().Count == 24);

            Debug.Log($"[TEST] TestStockWaste confirmed 0 cards in waste pile and 24 in stock pile after cycling back to stock pile");
        }

        
    /** 
        * here is our move card-from-to truth table:
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

        // Test: StockWasteHand_HandEmpty_HandFull
        // 1. stock -> waste (valid)
        // 2. stock -> hand (invalid)
        // 3. waste -> hand (valid)
        // 4. stock -> waste (valid)
        // 5. waste -> hand (invalid cause hand already holds a card and we're not picking up a substack from a tableau)
        public static void Test_StockWasteHand_HandEmpty_HandFull()
        {
            // get a fresh, empty test game
            var game = SolitaireGame.TestGame;
            game.NewGame();
            game.Deal(); // deal the deck

            Assert.IsTrue(game.GetStockCardPile().Count == 24);
            Assert.IsTrue(game.deck.DeckCardPile.Count == 0, "assert deck is empty after dealing");

            //var firstCard = game.GetStockCardPile().GetTopCard();
            var firstCard = game.GetTopStockCard(); // :(
            var fromStock = new PlayfieldSpot(PlayfieldArea.STOCK, 0);
            var toWaste = new PlayfieldSpot(PlayfieldArea.WASTE, 0);
            var move = new SolitaireMove(firstCard, fromStock, toWaste);

            // check move is valid
            // move should be valid because hand is empty
            var moveIsValid = SolitaireMoveValidator.IsValidMove(game.GetGameState(), move);
            Assert.IsTrue(moveIsValid);

            int stockLengthBeforeMove = game.GetStockCardPile().Count;

            // add our card to the waste pile
            // could also call game.StockToWaste()
            game.MoveCardToNewSpot(firstCard, toWaste, true);

            //SolitaireCard freshFirstCard = game.deck.GetCardBySuitRank(firstCard.GetSuitRank());
            //Debug.LogWarning($" is this an immutable issue? firstCard: {firstCard.playfieldSpot} vs firstFirstCard: {freshFirstCard.playfieldSpot}");

            // verify it got removed from the stock
            Assert.IsTrue(game.GetStockCardPile().Count == stockLengthBeforeMove - 1);

            // verify it got added to waste
            Assert.IsTrue(game.GetWasteCardPile().Count == 1, $"verify waste pile contains 1 card {game.GetWasteCardPile().Count}");


            
            
            // verify we CAN'T just pick up a stock card the card straight from the Stock to the Hand (has to pass to Waste first)
            var nextStockCard = game.GetTopStockCard(); // game.GetStockCardPile().GetTopCard();
            var invalidStockToHandMove = new SolitaireMove(nextStockCard, nextStockCard.playfieldSpot, PlayfieldSpot.Hand);
            Assert.IsFalse(SolitaireMoveValidator.IsValidMove(game.GetGameState(), invalidStockToHandMove));

            
            
            
            // Now, pre-occupy the hand by picking up the first card we turned over into the stock
            var moveToHand = new SolitaireMove(firstCard, firstCard.playfieldSpot, PlayfieldSpot.Hand);
            Assert.IsTrue(SolitaireMoveValidator.IsValidMove(game.GetGameState(), moveToHand));
            int countBeforeWasteToHand = game.GetWasteCardPile().Count;
            game.MoveCardToNewSpot(firstCard, PlayfieldSpot.Hand, true); // TODO: put the faceup logic in the MoveCardToNewSpot method

                // assert it left the waste pile
                Assert.IsTrue(game.GetWasteCardPile().Count == countBeforeWasteToHand - 1, $"assert waste pile count is one less than before we picked up the card {game.GetWasteCardPile().Count} / {countBeforeWasteToHand - 1}"); // should be 0 i think

                // assert it's in our hand
                Assert.IsTrue(game.GetHandCardPile().Count == 1); // todo: could GetTopCard and verify SuitRank equality


            
            
            // k, now we have to move a new stock card to the waste to try and pick it up (could also try to pick up a tableau card)
            // we'll do that in the next test method
            game.MoveCardToNewSpot(nextStockCard, PlayfieldSpot.Waste, true);

                // assert it's in the waste
                Assert.IsTrue(game.GetWasteCardPile().Count == 1);

            // [FAILURE CASE] now try to pick it up from the stock with a card already in our hand
            var moveAttemptPickUpCardWhenHandIsNotEmpty = new SolitaireMove(nextStockCard, nextStockCard.playfieldSpot, PlayfieldSpot.Hand);
                // assert it fails because we have a card in our hand!
                Assert.IsFalse(SolitaireMoveValidator.IsValidMove(game.GetGameState(), moveAttemptPickUpCardWhenHandIsNotEmpty));

            Debug.Log("Test_StockWasteHand_HandEmpty_HandFull [passed]!");
        }

        public static void test_foundation_validations()
        {
            // 1. test that autoplacing an ace goes to the correct foundation // todo: make foundations suit agnostic
            
            // 2. test that tableau and waste cards autoplace correctly onto foundation (even if there's a second valid spot on tableau)
        }

        public static void test_to_hand_and_return_to()
        {
            // for: waste, tableau, foundation
                // pick up card from {pile}
                // verify valid to place card back to pile
        }
    }
}
