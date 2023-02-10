using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Cards;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections;
using UnityEngine.XR;
using Unity.VisualScripting;

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
            
            // Test Our Move Validation
            TestFromHandMoves();
            TestFromDeckMoves();
            TestFromStockMoves();
            TestFromWasteMoves();
            TestFromFoundationMoves();
            TestFromTableauMoves();

            // it feels like it would've been more concise to test
            // TestToHandMoves, etc...

            // TODO:
            // test GetNextValidPlayfieldSpotForSuitRank
            // test CheckFlipOverTopCardInTableauCardJustLeft

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

            Managers.GameManager gmi = Managers.GameManager.Instance ?? GameObject.FindObjectOfType<Managers.GameManager>();

            Debug.Log($"Test[CanDealCards] Using Game Manager id: {gmi.gmi_id}");
            Managers.GameManager.Instance.SetGame(game);

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
                // i think this is returning a card from a fresh game manager instance, not the one we've already established
                SolitaireCard topCard = tabList[i].GetTopCard(); 
                //SolitaireCard topCard = game.deck.GetCardBySuitRank(tabList[i].Last());

                Debug.LogWarning("Debugging why top card is not face up after dealing");
                Debug.LogWarning($"topCard: {topCard} for tab list {i} (count:{tabList[i].Count}) isfaceup:{topCard.IsFaceUp}");

                Debug.LogWarning("what cards are in the tab list ? " + tabList);
                Assert.IsTrue(topCard.IsFaceUp);

                for(int c = 0; c < tabList[i].Count; c++)
                {
                    SolitaireCard checkCard = game.deck.GetCardBySuitRank(tabList[i][c]);

                    Debug.Log($"top card for tab {i} {checkCard}");
                    
                    // verify the previous cards are face down
                    if(c < tabList[i].Count - 1)
                    {
                        Assert.IsFalse(checkCard.IsFaceUp);
                    }
                    else
                    {
                        Assert.IsTrue(checkCard.IsFaceUp);
                    }
                }
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
            game.MoveCardToNewSpot(ref firstCard, toWaste, true);

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
            game.MoveCardToNewSpot(ref firstCard, PlayfieldSpot.Hand, true); // TODO: put the faceup logic in the MoveCardToNewSpot method

                // assert it left the waste pile
                Assert.IsTrue(game.GetWasteCardPile().Count == countBeforeWasteToHand - 1, $"assert waste pile count is one less than before we picked up the card {game.GetWasteCardPile().Count} / {countBeforeWasteToHand - 1}"); // should be 0 i think

                // assert it's in our hand
                Assert.IsTrue(game.GetHandCardPile().Count == 1); // todo: could GetTopCard and verify SuitRank equality


            
            
            // k, now we have to move a new stock card to the waste to try and pick it up (could also try to pick up a tableau card)
            // we'll do that in the next test method
            game.MoveCardToNewSpot(ref nextStockCard, PlayfieldSpot.Waste, true);

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

        public static void TestFromDeckMoves()
        {
            var game = SolitaireGame.TestGame;
            game.NewGame();
            //game.Deal(); -- skip dealing, all cards are still in deck

            // assert all cards still in deck
            Assert.IsTrue(game.deck.Count == 52, "assert all 52 cards still in game.deck.deckCardPile");
            
            var cardID = game.deck.DeckCardPile.Last();
            var testCard = game.deck.GetCardBySuitRank(cardID);
            
            var fromSpot = PlayfieldSpot.DECK;
            foreach(SolitaireMoveTypeToGroup toType in Enum.GetValues(typeof(SolitaireMoveTypeToGroup)))
            {
                // test validity of each move type
                var area_for_type = SolitaireMoveSet.GetPlayfieldAreaForMoveToTypeTo(toType);
                var toSpot = new PlayfieldSpot(area_for_type, 0);
                var testMoveTo = new SolitaireMove(testCard, fromSpot, toSpot);
                var isValid = SolitaireMoveValidator.IsValidMove(game.GetGameState(), testMoveTo);

                if(area_for_type == PlayfieldArea.HAND)
                {
                    // set it to face up so we don't get caught by the validator for trying to put a face down card in our hand
                    // TODO: add a separate test case for this
                    testCard.SetIsFaceUp(true);
                }

                SolitaireGameState mock_game_state;

                // the only valid moves in this set require special state cases,
                // so ALL should be false by default
                Assert.IsFalse(isValid, "assert invalid cases are reported as invalid");
                switch (area_for_type)
                {
                    // DECK_TO_DECK
                    case PlayfieldArea.DECK:
                        // assert only valid if "shuffling"
                        mock_game_state = game.GetGameState();
                        mock_game_state.SetMockIsShuffling(true);
                        bool isValidWhenCollecting = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenCollecting, "valid when shuffling");
                        break;
                    
                    // DECK_TO_STOCK
                    case PlayfieldArea.STOCK:
                    // DECK_TO_TABLEAU
                    case PlayfieldArea.TABLEAU:
                        // assert only valid if "dealing"
                        mock_game_state = game.GetGameState();
                        mock_game_state.SetMockIsDealing(true);
                        bool isValidWhenDealing = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenDealing, "valid when dealing");
                        break;
                }
            }

            Debug.LogWarning("[SolitaireGameTests > TestDeckToMoves (From Deck Moves)] All Assertions Passed!");
        }

        public static void TestFromHandMoves()
        {
            var game = SolitaireGame.TestGame;
            game.NewGame();
            //game.Deal(); -- skip dealing, all cards are still in deck

            // assert all cards still in deck
            Assert.IsTrue(game.deck.Count == 52, "assert all 52 cards still in game.deck.deckCardPile");

            // INTENTIONALLY SETTING A SPECIFIC CARD HERE
            // we don't want to accidentally get a King or an Ace which would cause the first Assertion to fail sometimes 8/52 runs
            var cardID = new SuitRank(Suit.SPADES, Rank.TWO);
            var testCard = game.deck.GetCardBySuitRank(cardID);

            // put the card in the hand
            testCard.SetIsFaceUp(true);
            PlayfieldSpot handSpot = PlayfieldSpot.HAND;
            game.MoveCardToNewSpot(ref testCard, handSpot, true);

            var fromSpot = handSpot;
            foreach (SolitaireMoveTypeToGroup toType in Enum.GetValues(typeof(SolitaireMoveTypeToGroup)))
            {
                // test validity of each move type
                var area_for_type = SolitaireMoveSet.GetPlayfieldAreaForMoveToTypeTo(toType);
                var toSpot = new PlayfieldSpot(area_for_type, 0);
                var testMoveTo = new SolitaireMove(testCard, fromSpot, toSpot);
                var isValid = SolitaireMoveValidator.IsValidMove(game.GetGameState(), testMoveTo);
                           
                // HAND_TO_HAND         Invalid (maybe valid for other games)
                // HAND_TO_STOCK        Invalid
                // HAND_TO_DECK         IsCollecting
                // HAND_TO_WASTE        OnlyWhenReturning
                // HAND_TO_FOUNDATION   ValidWhen.FoundationCanReceiveCard
                // HAND_TO_TABLEAU      ValidWhen.TableauCanReceiveCard
               
                Assert.IsFalse(isValid, "assert all invalid by default");

                SolitaireGameState mock_game_state;
                
                // Test special state triggers that make the move valid:
                switch (area_for_type)
                {
                    // HAND_TO_DECK
                    case PlayfieldArea.DECK:
                        // assert only valid if "collecting"
                        mock_game_state = game.GetGameState();
                        mock_game_state.SetMockIsCollectingCardsToDeck(true);
                        bool isValidWhenCollecting = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenCollecting, "valid when collecting");
                        break;
                    
                    // HAND_TO_TABLEAU
                    case PlayfieldArea.TABLEAU:
                        // only when TableauCanReceiveCard
                        game.BuildTableaus();
                        mock_game_state = game.GetGameState();
                        Suit oppositeSuit = SolitaireDeck.GetOppositeColorSuit(testCard.GetSuit());
                        int oneGreaterRank = (int)testCard.GetRank() + 1;
                        mock_game_state.TableauPileGroup[0].Add(new SuitRank(oppositeSuit, (Rank)oneGreaterRank));
                        Assert.IsTrue(mock_game_state.TableauPileGroup.Count == 7);
                        Assert.IsTrue(mock_game_state.TableauPileGroup[0].Count == 1);
                        bool isValidWhenDealing = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);

                        SuitRank topCard = mock_game_state.TableauPileGroup[0].Last();
                        Debug.LogWarning($"validating hand_to_tableau: {testCard} {topCard} {testMoveTo}");
                        
                        Assert.IsTrue(isValidWhenDealing, "valid when tableau can receive card");
                        break;

                    // HAND_TO_FOUNDATION
                    case PlayfieldArea.FOUNDATION:
                        game.BuildFoundations();
                        mock_game_state = game.GetGameState();

                        // only when FoundationCanReceiveCard
                        SuitRank foundationCard = new SuitRank(testCard.GetSuit(), Rank.ACE);
                        // important: make sure we have the right foundation index set (not 0)
                        toSpot = new(PlayfieldArea.FOUNDATION, (int)testCard.GetSuit());
                        testMoveTo = new SolitaireMove(testCard, testCard.playfieldSpot, toSpot); 
                        mock_game_state.FoundationPileGroup[(int)testCard.GetSuit()].Add(foundationCard);
                        Assert.IsTrue(mock_game_state.FoundationPileGroup.Count == 4, $"expected 4 got {mock_game_state.FoundationPileGroup.Count}");
                        Assert.IsTrue(mock_game_state.FoundationPileGroup[(int)testCard.GetSuit()].Count == 1);
                        bool isValidWhenFoundationCanReceiveCard = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenFoundationCanReceiveCard, "valid when foundation can receive card");
                        break;

                    // HAND_TO_WASTE
                    case PlayfieldArea.WASTE:
                        // only when returning
                        mock_game_state = game.GetGameState();
                        testMoveTo.Subject.previousPlayfieldSpot.area = PlayfieldArea.WASTE;
                        bool isValidWhenReturning = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenReturning, "valid when returning");
                        break;
                }
            }

            Debug.LogWarning("[SolitaireGameTests > TestHandToMoves (From Hand Moves)] All Assertions Passed!");
        }
    
        public static void TestFromStockMoves()
        {
            // === TODO: extract into a setup method ===
            var game = SolitaireGame.TestGame;
            game.NewGame();
            //game.Deal(); -- skip dealing, all cards are still in deck

            // assert all cards still in deck
            Assert.IsTrue(game.deck.Count == 52, "assert all 52 cards still in game.deck.deckCardPile");

            var cardID = game.deck.DeckCardPile.Last();
            var testCard = game.deck.GetCardBySuitRank(cardID);

            // put the card in the stock pile
            // testCard.SetIsFaceUp(false);
            PlayfieldSpot fromSpot = PlayfieldSpot.STOCK;
            game.MoveCardToNewSpot(ref testCard, fromSpot, true);

            foreach (SolitaireMoveTypeToGroup toType in Enum.GetValues(typeof(SolitaireMoveTypeToGroup)))
            {
                // test validity of each move type
                var area_for_type = SolitaireMoveSet.GetPlayfieldAreaForMoveToTypeTo(toType);
                var toSpot = new PlayfieldSpot(area_for_type, 0);
                var testMoveTo = new SolitaireMove(testCard, fromSpot, toSpot);
                var isValid = SolitaireMoveValidator.IsValidMove(game.GetGameState(), testMoveTo);

                // STOCK_TO_HAND         Invalid
                // STOCK_TO_STOCK        Invalid
                // STOCK_TO_FOUNDATION   Invalid
                // STOCK_TO_TABLEAU      Invalid
                // STOCK_TO_DECK         IsCollecting
                // STOCK_TO_WASTE        Valid

                if(toSpot.area == PlayfieldArea.WASTE)
                {
                    // to waste should be true by default, no special state needed
                    Assert.IsTrue(isValid, "STOCK_TO_WASTE is valid");
                }
                else
                {
                    // assert the rest are false by default
                    Assert.IsFalse(isValid, "assert all invalid by default");
                }

                SolitaireGameState mock_game_state;

                // Test special state triggers that make the move valid:
                switch (area_for_type)
                {
                    case PlayfieldArea.DECK:
                        // assert only valid if "collecting"
                        mock_game_state = game.GetGameState();
                        mock_game_state.SetMockIsCollectingCardsToDeck(true);
                        bool isValidWhenCollecting = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenCollecting, "valid when collecting");
                        break;
                }
            }

            Debug.LogWarning("[SolitaireGameTests > TestFromStockMoves] All Assertions Passed!");
        }

        public static void TestFromWasteMoves()
        {
            // === TODO: extract into a setup method ===
            var game = SolitaireGame.TestGame;
            game.NewGame();
            //game.Deal(); -- skip dealing, all cards are still in deck

            // assert all cards still in deck
            Assert.IsTrue(game.deck.Count == 52, "assert all 52 cards still in game.deck.deckCardPile");

            // INTENTIONALLY SETTING A SPECIFIC CARD HERE
            // we don't want to accidentally get a King or an Ace which would cause the first Assertion to fail sometimes 8/52 runs
            var cardID = new SuitRank(Suit.SPADES, Rank.TWO);
            var testCard = game.deck.GetCardBySuitRank(cardID);

            // put the card in the WASTE pile
            // testCard.SetIsFaceUp(false);
            PlayfieldSpot fromSpot = PlayfieldSpot.WASTE;
            game.MoveCardToNewSpot(ref testCard, fromSpot, true);

            foreach (SolitaireMoveTypeToGroup toType in Enum.GetValues(typeof(SolitaireMoveTypeToGroup)))
            {
                // test validity of each move type
                var area_for_type = SolitaireMoveSet.GetPlayfieldAreaForMoveToTypeTo(toType);
                var toSpot = new PlayfieldSpot(area_for_type, 0);
                var testMoveTo = new SolitaireMove(testCard, fromSpot, toSpot);
                SolitaireGameState mock_game_state = game.GetGameState();
                var isValid = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);

                // * WASTE_TO_WASTE        Invalid
                // * WASTE_TO_HAND         WhenHandIsEmpty (HandCanReceiveCard)
                // * WASTE_TO_STOCK        WhenRecycling
                // * WASTE_TO_FOUNDATION   WhenFoundationCanReceiveCard
                // * WASTE_TO_TABLEAU      WhenTableauCanReceiveCard
                // * WASTE_TO_DECK         IsCollecting

                if(toSpot.area == PlayfieldArea.HAND)
                {
                    // assume valid since our hand is empty
                    Assert.IsTrue(isValid, "valid for empty hand");
                    // test would fail if hand not empty
                    mock_game_state.HandPile.Add(game.deck.GetCardBySuitRank(new SuitRank(Suit.SPADES, Rank.THREE)));
                    var isValidWithCardAlreadyInHand = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                    Assert.IsFalse(isValidWithCardAlreadyInHand, "[WASTE_TO_HAND] invalid for occupied hand");
                    continue;
                }
                
                Assert.IsFalse(isValid, "assert all invalid by default");

                // Test special state triggers that make the move valid:
                switch (area_for_type)
                {
                    case PlayfieldArea.DECK:
                        // assert only valid if "collecting"
                        mock_game_state.SetMockIsCollectingCardsToDeck(true);
                        bool isValidWhenCollecting = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenCollecting, "valid when collecting");
                        break;
                    case PlayfieldArea.STOCK:
                        mock_game_state.SetMockIsRecyclingWasteToStock(true);
                        bool isValidWhenRecyling = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenRecyling, "valid when recylcing stock to waste");
                        break;
                    case PlayfieldArea.FOUNDATION:
                        // important: make sure we have the right foundation index set (not 0)
                        toSpot = new(PlayfieldArea.FOUNDATION, (int)testCard.GetSuit());
                        testMoveTo = new SolitaireMove(testCard, testCard.playfieldSpot, toSpot);
                        SolitaireCard aceOfSpades = game.deck.GetCardBySuitRank(new SuitRank(Suit.SPADES, Rank.ACE));
                        mock_game_state.FoundationPileGroup[(int)testCard.GetSuit()].Add(aceOfSpades);
                        bool isValidWhenFoundationCanReceive = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenFoundationCanReceive);
                        break;
                    case PlayfieldArea.TABLEAU:
                        SolitaireCard threeOfHearts = game.deck.GetCardBySuitRank(new SuitRank(Suit.HEARTS, Rank.THREE));
                        mock_game_state.TableauPileGroup[0].Add(threeOfHearts);
                        bool isValidWhenTableauCanReceive = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenTableauCanReceive);
                        break;
                    
                }
            }

            Debug.LogWarning("[SolitaireGameTests > TestFromWasteMoves] All Assertions Passed!");
        }

        public static void TestFromFoundationMoves()
        {
            // === TODO: extract into a setup method ===
            var game = SolitaireGame.TestGame;
            game.NewGame();
            //game.Deal(); -- skip dealing, all cards are still in deck

            // assert all cards still in deck
            Assert.IsTrue(game.deck.Count == 52, "assert all 52 cards still in game.deck.deckCardPile");

            // INTENTIONALLY SETTING A SPECIFIC CARD HERE
            // we don't want to accidentally get a King or an Ace which would cause the first Assertion to fail sometimes 8/52 runs
            var cardID = new SuitRank(Suit.SPADES, Rank.TWO);
            var testCard = game.deck.GetCardBySuitRank(cardID);

            // put the card in the FOUNDATION pile
            // testCard.SetIsFaceUp(false);
            PlayfieldSpot fromSpot = new PlayfieldSpot(PlayfieldArea.FOUNDATION, 0);
            game.MoveCardToNewSpot(ref testCard, fromSpot, true);

            foreach (SolitaireMoveTypeToGroup toType in Enum.GetValues(typeof(SolitaireMoveTypeToGroup)))
            {
                // test validity of each move type
                var area_for_type = SolitaireMoveSet.GetPlayfieldAreaForMoveToTypeTo(toType);
                var toSpot = new PlayfieldSpot(area_for_type, 0);
                var testMoveTo = new SolitaireMove(testCard, fromSpot, toSpot);
                SolitaireGameState mock_game_state = game.GetGameState();
                var isValid = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);

                // * FOUNDATION_TO_WASTE        WhenUndoRedo
                // * FOUNDATION_TO_HAND         WhenHandIsEmpty (HandCanReceiveCard)
                // * FOUNDATION_TO_STOCK        Invalid
                // * FOUNDATION_TO_FOUNDATION   Invalid
                // * FOUNDATION_TO_TABLEAU      WhenTableauCanReceiveCard
                // * FOUNDATION_TO_DECK         IsCollecting

                if (toSpot.area == PlayfieldArea.HAND)
                {
                    // assume valid since our hand is empty
                    Assert.IsTrue(isValid, "valid for empty hand");
                    // test would fail if hand not empty
                    mock_game_state.HandPile.Add(game.deck.GetCardBySuitRank(new SuitRank(Suit.SPADES, Rank.THREE)));
                    var isValidWithCardAlreadyInHand = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                    Assert.IsFalse(isValidWithCardAlreadyInHand, "[WASTE_TO_HAND] invalid for occupied hand");
                    continue;
                }

                Assert.IsFalse(isValid, "assert all invalid by default");

                // Test special state triggers that make the move valid:
                switch (area_for_type)
                {
                    case PlayfieldArea.DECK:
                        // assert only valid if "collecting"
                        mock_game_state.SetMockIsCollectingCardsToDeck(true);
                        bool isValidWhenCollecting = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenCollecting, "valid when collecting");
                        break;
                    case PlayfieldArea.TABLEAU:
                        SolitaireCard threeOfHearts = game.deck.GetCardBySuitRank(new SuitRank(Suit.HEARTS, Rank.THREE));
                        mock_game_state.TableauPileGroup[0].Add(threeOfHearts);
                        bool isValidWhenTableauCanReceive = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenTableauCanReceive);
                        break;

                }
            }
            
            Debug.LogWarning("[SolitaireGameTests > TestFromFoundationMoves] All Assertions Passed!");
        }

        public static void TestFromTableauMoves()
        {
            // === TODO: extract into a setup method ===
            var game = SolitaireGame.TestGame;
            game.NewGame();
            //game.Deal(); -- skip dealing, all cards are still in deck

            // assert all cards still in deck
            Assert.IsTrue(game.deck.Count == 52, "assert all 52 cards still in game.deck.deckCardPile");

            // INTENTIONALLY SETTING A SPECIFIC CARD HERE
            // we don't want to accidentally get a King or an Ace which would cause the first Assertion to fail sometimes 8/52 runs
            var cardID = new SuitRank(Suit.SPADES, Rank.TWO);
            var testCard = game.deck.GetCardBySuitRank(cardID);

            // put the card in the TABLEAU pile
            // testCard.SetIsFaceUp(false);
            PlayfieldSpot fromSpot = new PlayfieldSpot(PlayfieldArea.TABLEAU, 0);
            game.MoveCardToNewSpot(ref testCard, fromSpot, true);

            foreach (SolitaireMoveTypeToGroup toType in Enum.GetValues(typeof(SolitaireMoveTypeToGroup)))
            {
                // test validity of each move type
                var area_for_type = SolitaireMoveSet.GetPlayfieldAreaForMoveToTypeTo(toType);
                var toSpot = new PlayfieldSpot(area_for_type, 0);
                var testMoveTo = new SolitaireMove(testCard, fromSpot, toSpot);
                SolitaireGameState mock_game_state = game.GetGameState();
                var isValid = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);

                // * TABLEAU_TO_WASTE        WhenUndoRedo
                // * TABLEAU_TO_HAND         WhenHandIsEmpty (HandCanReceiveCard)
                // * TABLEAU_TO_STOCK        Invalid
                // * TABLEAU_TO_FOUNDATION   WhenFoundationCanReceiveCard
                // * TABLEAU_TO_TABLEAU      Invalid
                // * TABLEAU_TO_DECK         IsCollecting

                if (toSpot.area == PlayfieldArea.HAND)
                {
                    // assume valid since our hand is empty
                    Assert.IsTrue(isValid, "valid for empty hand");
                    // test would fail if hand not empty
                    mock_game_state.HandPile.Add(game.deck.GetCardBySuitRank(new SuitRank(Suit.SPADES, Rank.THREE)));
                    var isValidWithCardAlreadyInHand = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                    Assert.IsFalse(isValidWithCardAlreadyInHand, "[WASTE_TO_HAND] invalid for occupied hand");
                    continue;
                }

                Assert.IsFalse(isValid, "assert all invalid by default");

                // Test special state triggers that make the move valid:
                switch (area_for_type)
                {
                    case PlayfieldArea.DECK:
                        // assert only valid if "collecting"
                        mock_game_state.SetMockIsCollectingCardsToDeck(true);
                        bool isValidWhenCollecting = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenCollecting, "valid when collecting");
                        break;
                    case PlayfieldArea.FOUNDATION:
                        // important: make sure we have the right foundation index set (not 0)
                        toSpot = new(PlayfieldArea.FOUNDATION, (int)testCard.GetSuit());
                        testMoveTo = new SolitaireMove(testCard, testCard.playfieldSpot, toSpot);
                        SolitaireCard aceOfSpades = game.deck.GetCardBySuitRank(new SuitRank(Suit.SPADES, Rank.ACE));
                        mock_game_state.FoundationPileGroup[(int)testCard.GetSuit()].Add(aceOfSpades);
                        bool isValidWhenFoundationCanReceive = SolitaireMoveValidator.IsValidMove(mock_game_state, testMoveTo);
                        Assert.IsTrue(isValidWhenFoundationCanReceive);
                        break;

                }
            }
            
            Debug.LogWarning("[SolitaireGameTests > TestFromTableauMoves] All Assertions Passed!");
        }
    }
}
