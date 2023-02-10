﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.CardBox.Animations;
using PoweredOn.CardBox.PlayingCards;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Assertions;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireGame
    {
        // TODO: maybe Move to GameManager.Instance.Dealer.Deck
        // or GameManager.Instance.DeckManager.Deck
        private SolitaireDeck _deck;

        bool _isDealing = false;

        DebugOutput iDebug
        {
            get
            {
                return DebugOutput.Instance ?? GameObject.FindObjectOfType<DebugOutput>();
            }
        }
        
        public bool IsDealing { get { return _isDealing; } }

        public bool IsRecyclingWasteToStock { get; internal set; }

        public static SolitaireGame TestGame
        {
            get
            {
                var game = new SolitaireGame();
                game.SetToTestMode();
                return game;
            }
        }

        // when test mode is activated, code paths that relate to GameObjects are skipped
        private bool runningInTestMode = false; 

        private Dictionary<SolitaireGameObject, GameObject> gameObjectReferences;
        public SolitaireDeck deck {
            get {
                return _deck; // todo: return a clone?
            }
            set
            {
                _deck = value;
            }
        }

        public void SetToTestMode()
        {
            runningInTestMode = true;
        }

        public bool IsRunningInTestMode
        {
            get
            {
                return this.runningInTestMode;
            }
        }

        public bool IsPickingUpSubstack { get; internal set; }

        // TODO: maybe make this a method of SolitaireDealer or SolitaireDeckManager
        public SolitaireDeck BuildDeck()
        {
            _deck = new SolitaireDeck(this);
            return _deck; // todo: return a clone?
        }

        public GameObject GetGameObjectByType(SolitaireGameObject gameObjectType)
        {
            if(gameObjectType == SolitaireGameObject.None)
            {
                Debug.LogWarning("Requested None SolitaireGameObject");
                return null;
            }
            //Debug.LogWarning("[GetGameObjectByType] IsRunningInTestMode? " + IsRunningInTestMode);
            if (IsRunningInTestMode)
            {
                //return new GameObject(); // this spams tree with objects
                return null; 
            }
            var heldReference = gameObjectReferences?[gameObjectType];
            if(heldReference == null)
            {
                Debug.LogError($"{gameObjectType} missing from gameObjectReferences");
            }
            /*string heldRefStr = (heldReference == null) ? "false" : "true";
            Debug.LogWarning($"held reference for type? {heldRefStr} {gameObjectType}");*/
            return heldReference;
        }

        // TODO: these constants should probably be moved to a config file
        // OR: they could be moved to SolitairePlayfield.cs
        // OR: they could be moved to SolitaireDealer.cs
        public const float TAB_SPACING = 0.1f;
        const float RANDOM_AMPLITUDE = 1f;
        public const float CARD_THICKNESS = 0.001f;
        public const float TAB_VERT_OFFSET = 0.02f;

        // z-axis rotation is temporary until i fix the orientation of the mesh in blender
        public Quaternion CARD_DEFAULT_ROTATION = Quaternion.Euler(0, 180, 90);

        List<SuitRank> dealtOrder = new List<SuitRank>();
        FoundationCardPileGroup foundationCardPileGroup = FoundationCardPileGroup.EMPTY;
        TableauCardPileGroup tableauCardPileGroup = TableauCardPileGroup.EMPTY;
        StockCardPile stockCardPile = StockCardPile.EMPTY;
        WasteCardPile wasteCardPile = WasteCardPile.EMPTY;
        HandCardPile handCardPile = HandCardPile.EMPTY;

        private bool autoplaying = false;

        bool autoPlaceEnabled = true;

        // card, from, to
        SolitaireMoveList moveLog;

        //public DebugOutput m_DebugOutput;

        int m_Moves = 0;
        public SolitaireGame(){}

        public PlayingCardIDList GetDealtOrder()
        {
            return new PlayingCardIDList(dealtOrder);
        }

        public FoundationCardPile GetFoundationCardPileForSuit(Suit suit)
        {
            return this.foundationCardPileGroup.GetFoundationCardPileForSuit(suit);
        }

        public FoundationCardPileGroup GetFoundationCardPileGroup()
        {
            return this.foundationCardPileGroup;
        }

        public DeckCardPile GetDeckCardPile()
        {
            return new DeckCardPile(deck.DeckCardPile);
        }

        public HandCardPile GetHandCardPile()
        {
            return handCardPile.Clone();
        }

        public TableauCardPileGroup GetTableauCardPileGroup()
        {
            return tableauCardPileGroup.Clone();
        }

        public StockCardPile GetStockCardPile()
        {
            return stockCardPile.Clone();
        }

        public SolitaireCard GetTopStockCard()
        {
            // NOTE: i tried adding a stockCardPile.GetTopCard() method
            // but it has no sense of the current game object
            // and i either need to pass a reference into each ctor of a pile
            // or keep the lookup here on the game class for now
            // i did have what i thought was a neat trick with a GameManager.game.GetCardBySuitRank(id) singleton shortcut
            // but GameManager.Instance is null when i call RunTest from the editor using GameManagerGUI so /shrug
            // ugly extra methods it is for now
            // sucks, cause this was the whole point of the pile classes... to treat the piles like objects
            // but if they don't know how to retreive a Card for the CardIDs(SuitRanks) they contain,
            // it's kind of useless overhead at this point that I wasted a day refactoring out /shrug /sigh /still learning
            SuitRank id = stockCardPile.Last();
            return deck.GetCardBySuitRank(id);
        }

        public SolitaireCard GetTopWasteCard()
        {
            return deck.GetCardBySuitRank(wasteCardPile.Last());
        }

        public SolitaireCard GetTopCardForFoundation(int findex)
        {
            return deck.GetCardBySuitRank(foundationCardPileGroup[findex].DefaultIfEmpty(SuitRank.NONE).LastOrDefault());
        }

        public WasteCardPile GetWasteCardPile()
        {
            return wasteCardPile.Clone();
        }

        public void NewGame()
        {
            // reset game object references
            UpdateGameObjectReferences();

            dealtOrder = new List<SuitRank>();

            // reset move count / log
            m_Moves = 0;
            moveLog = new SolitaireMoveList();

            // BuildStock
            stockCardPile = new StockCardPile();

            // Build Waste
            wasteCardPile = new WasteCardPile();

            // Build Hand
            handCardPile = new HandCardPile();

            BuildFoundations();
            BuildTableaus();
            BuildDeck();
        }

        public void ToggleAutoPlace()
        {
            autoPlaceEnabled = !autoPlaceEnabled;
        }

        public void AutoPlayNextMove()
        {
            SolitaireMoveList moves = GetNextBestMoves();
        }

        public SolitaireMoveList GetNextBestMoves()
        {
            SolitaireMoveList moves = new SolitaireMoveList();

            return moves;
        }

        public SolitaireMoveList GetMovesToFoundation()
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        public void StartAutoPlay()
        {
            this.autoplaying = true;
        }

        public void StopAutoPlay()
        {
            this.autoplaying = false;
        }
        
        // TODO: put this in a dealer class
        public void Deal()
        {
            NewGame();

            // first, we want to collect all the cards into a stack
            deck.CollectCardsToDeck();

            //await Task.Delay(1000);

            // let's shuffle the card order 3 times (todo: artifically delay and animate this)
            deck.Shuffle(3);

            // Flag for Move Validator
            _isDealing = true;

            //await Task.Delay(1000);

            if (deck.cards.Count != 52)
                throw new Exception($"[DEAL] invalid deck card count after shuffling {deck.cards.Count}");

            if (deck.deckOrderList.Count != 52)
                throw new Exception($"[DEAL] invalid deck deckOrderList count after shuffling {deck.deckOrderList.Count}");

            // move cards from the deck to the stock pile
            // capture all SuitRanks in the "stockCards" pile to begin with
            for (int initial_loop_index = 0; initial_loop_index < 52; initial_loop_index++)
            {
                /*try
                {*/
                SolitaireCard card = deck.GetCardBySuitRank(deck.deckOrderList[initial_loop_index]);
                //MoveCardToNewSpot(ref card, new PlayfieldSpot(PlayfieldArea.STOCK, initial_loop_index), false);
                deck.RemoveCardFromDeck(card.GetSuitRank());
                stockCardPile.Add(card.GetSuitRank());
                card.SetPlayfieldSpot(new PlayfieldSpot(PlayfieldArea.STOCK, initial_loop_index));
                /*}
                catch (Exception e)
                {
                    iDebug.LogError(e.ToString());
                }*/
            }

            Debug.LogWarning("---------------");
            
            if (deck.DeckCardPile.Count != 0)
            {
                throw new Exception($"[DEAL] invalid deck card count after dealing. no cards should be in the deck list");
            }

            if (stockCardPile.Count != 52)
            {
                throw new Exception($"[DEAL] invalid stock card count after shuffling and moving from deck to stockCards list {stockCardPile.Count}");
            }

            /* Deal 28 cards */

            for (int round = 0; round < 7; round++)
            {
                for (int pile = 0; pile < 7; pile++)
                {
                    // Skip piles before the current round
                    if (pile < round)
                    {
                        continue;
                    }

                    // Get the next card from the stock pile and deal it
                    SuitRank suitRankToDeal = stockCardPile[0];

                    SolitaireCard card = deck.GetCardBySuitRank(suitRankToDeal);

                    bool faceUp = pile == round;

                    dealtOrder.Add(suitRankToDeal);

                    // NOTE: inside this method, we handle adding SuitRank to the proper Tableau list
                    // we also handle removing it from the previous spot (PlayfieldArea.Stock)
                    MoveCardToNewSpot(ref card, new PlayfieldSpot(PlayfieldArea.TABLEAU, pile, round), faceUp, 0.1f * dealtOrder.Count);

                    iDebug.Log($"Dealing {dealtOrder.Count}: {card} | round:{round} pile:{pile} faceup:{faceUp}");
                    Debug.LogWarning($"Dealing {dealtOrder.Count}: {card} | round:{round} pile:{pile} faceup:{faceUp}");

                    // assert the playfield spot updated
                    Assert.IsTrue(card.playfieldSpot.area == PlayfieldArea.TABLEAU);
                    
                    // assert the index within the tableau is correct
                    Assert.IsTrue(card.playfieldSpot.index == pile);
                    
                    // assert the IsFaceUp value is correctly set
                    Assert.IsTrue(card.IsFaceUp == faceUp);
                }
            }

            if (dealtOrder.Count != 28)
            {
                throw new Exception($"[DEAL] invalid dealtOrder count after moving cards from stock to tableau {stockCardPile.Count}");
            }

            Debug.LogWarning("--------------- tableau cards set -------");

            // then, for the remaining cards, update their GoalIdentity to place them where the "Stock" Pile should go
            // remember to offset the local Z position a bit to make the cards appear as tho they're stacked on top of each other.
            // the last card should be the lowest z-position (same z as the stock pile guide object) and then each card up to the 0th should be the highest
            // so we should loop backwards through the remaining Stock when setting the positions
            if (stockCardPile.Count != 24)
            {
                throw new Exception($"[DEAL] invalid stock card count after moving cards from stock to tableau {stockCardPile.Count}");
            }

            float deal_delay = 0.1f * dealtOrder.Count;

            // Loop over ALL stock cards and set goal
            for (int sc2 = stockCardPile.Count-1; sc2 >= 0; sc2--)
            {
                Debug.LogWarning("SC2: " + stockCardPile.Count + " " + sc2); //
                SolitaireCard card = deck.GetCardBySuitRank(stockCardPile[sc2]);
                float delay = deal_delay + (0.025f * (sc2));

                // NOTE: inside this method we handle adding SuitRank to the stockCards list
                /* always face down when adding to stock */
                MoveCardToNewSpot(ref card, new PlayfieldSpot(PlayfieldArea.STOCK, sc2), false, delay);
            }

            // flag as done
            _isDealing = false;
        }

        public void TryAutoPlay()
        {
            if (autoplaying)
            {
                // find next best move
                SolitaireMoveList nextBestMoves = SolitaireMoveSuggestor.SuggestMoves(this);
                if(nextBestMoves.Count == 0)
                {
                    Debug.LogWarning("[AutoPlay] Stopping. No Moves Suggested.");
                    StopAutoPlay();
                }
                else
                {
                    Debug.LogWarning($"[AutoPlay] Playing best move out of {nextBestMoves.Count}: {nextBestMoves.First()}");
                }
            }
        }

        public void SetCardGoalsToRandomPositions()
        {
            var GO = deck.DeckCardPile.gameObject;
            if(GO == null)
            {
                var pile = this.deck.DeckCardPile;
                Debug.LogWarning($"deck.DeckCardPile is being weird {(SolitaireGameObject)DeckCardPile.gameObjectType}");
                
                UpdateGameObjectReferences();
                
                // yuck: manual binding/lookup instead of cached lookup
                GO = GameObject.Find("DeckOfCards");

                Debug.LogWarning($"is it available now? {GetGameObjectByType(SolitaireGameObject.Deck_Base)}");
            }
            // get the deck world position
            var deckOfCards = GO;// GetGameObjectByType(SolitaireGameObject.Deck_Base);
            Vector3 deckPosition = deckOfCards.transform.position + Vector3.zero;

            // loop through our cards, and give them a new GoalIdentity based on our calculations
            foreach (SolitaireCard card in deck.cards)
            {
                Transform cardTransform = card.gameObject.transform;

                // get the deck order of the current card
                //int deckOrder = card.GetDeckOrder();

                Vector3 randomVec3 = new Vector3(
                    UnityEngine.Random.Range(-RANDOM_AMPLITUDE, RANDOM_AMPLITUDE),
                    UnityEngine.Random.Range(-RANDOM_AMPLITUDE, RANDOM_AMPLITUDE),
                    UnityEngine.Random.Range(-RANDOM_AMPLITUDE, RANDOM_AMPLITUDE)
                );

                // calculate the goal position of the current card
                Vector3 newPosition = deckPosition + randomVec3;

                Vector3 worldToLocal = cardTransform.InverseTransformPoint(newPosition);

                GoalIdentity goalID = new GoalIdentity(
                    card.gameObject,
                    worldToLocal,
                    cardTransform.localRotation,
                    cardTransform.localScale);

                // set the goal position of the current card
                card.SetGoalIdentity(goalID);
                card.SetIsFaceUp(true);
            }
        }

        // when the user single-click's on a card, we auto-move it to the next best spot
        public PlayfieldSpot GetNextValidPlayfieldSpotForSuitRank(SuitRank suitrank)
        {
            if (!autoPlaceEnabled)
            {
                return PlayfieldSpot.INVALID;
            }

            // check the top card of the foundation first
            iDebug.LogWarning("[GetNextValidPlayfieldSpotForSuitRank] Checking foundation card list for rank: " + suitrank.rank + " suit int: " + (int)suitrank.suit + " cardGroupCount:" + foundationCardPileGroup.Count);
            FoundationCardPile foundationPile = foundationCardPileGroup[(int)suitrank.suit];
            SuitRank topCardSR;
            iDebug.LogWarning($"[GetNextValidPlayfieldSpotForSuitRank] Foundation List[{(int)suitrank.suit}].Count: " + foundationPile.Count);

            // TODO: .IsEmpty
            if (foundationPile.Count == 0)
            {
                // if the foundation list is empty, that's our first choice spot
                // if the rank is Rank.ace (0), return the foundation pile for the suit
                if (suitrank.rank == Rank.ACE) { 
                    return new PlayfieldSpot(PlayfieldArea.FOUNDATION, (int)suitrank.rank, 0); 
                }
            }
            else
            {
                // TODO: .GetTopCard
                topCardSR = foundationPile.Last();
                iDebug.LogWarning($"[GetNextValidPlayfieldSpotForSuitRank] checking if top card in foundation list is one rank less than the card we are trying to place {(int)topCardSR.rank} {(int)suitrank.rank}");
                if ((int)topCardSR.rank + 1 == (int)suitrank.rank)
                {
                    // if the top card in the foundation list is one less than this card's rank, that's our first choice spot
                    return new PlayfieldSpot(PlayfieldArea.FOUNDATION, (int)suitrank.suit, foundationPile.Count);
                }
            }

            // next, loop through the tableau piles from left to right and see if there's a valid spot for this card to go to
            for (int i = 0; i < 7; i++)
            {
                if (tableauCardPileGroup[i].Count == 0)
                {
                    // if the rank is Rank.king (12), see if we have an open tableau spot and put it there
                    if (suitrank.rank == Rank.KING)
                    {
                        // empty tableau found for you, my king!
                        return new PlayfieldSpot(PlayfieldArea.TABLEAU, i, 0);
                    }
                }
                else
                {
                    topCardSR = tableauCardPileGroup[i].Last();
                    if (
                       (int)topCardSR.rank - 1 == (int)suitrank.rank
                       && SolitaireDeck.SuitColorsAreOpposite(topCardSR.suit, suitrank.suit)
                    )
                    {
                        return new PlayfieldSpot(PlayfieldArea.TABLEAU, i, tableauCardPileGroup[i].Count);
                    }
                }
            }

            iDebug.LogWarning($"no suggested playfield spot found for {suitrank}");

            return PlayfieldSpot.INVALID;
        }

        public SolitaireGameState GetGameState()
        {
            // convert the current state of the game into an immutable copy
            // then, we can convert it to a bit mask for move validation
            return new SolitaireGameState(this);
        }

        // this method is being refactored...
        public void MoveCardToNewSpot(ref SolitaireCard card, PlayfieldSpot spot, bool faceUp, float delay = 0.0f, int substackIndex = 0)
        {
            //if(runningInTestMode) Debug.Log($"MoveCardToNewSpot {card.GetGameObjectName()} from {card.playfieldSpot} to {spot} faceup:{faceUp} delay:{delay}");
            moveLog.Add(new SolitaireMove(card, card.previousPlayfieldSpot, spot, substackIndex));

            /* Remove from previous spot (pile) if applicable */
            if (card.playfieldSpot.area != PlayfieldArea.INVALID)
            {
                card.SetPreviousPlayfieldSpot(card.playfieldSpot.Clone());
                try
                {
                    RemoveCardFromCurrentPlayfieldSpot(card);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    iDebug.LogError(e.ToString());
                }
            }


            /* 
                TODO: fix it so we can reference game objects in Editor mode when running tests
                for now we just conditionally reference the game object
             */
            GameObject cardGO = card.gameObject;
            GoalIdentity goalID = null;
            Transform cardTX;
            Vector3 gPos = Vector3.zero;
            if (cardGO != null) { 
                goalID = new(cardGO, Vector3.zero, Quaternion.identity, Vector3.one);
                cardTX = cardGO.transform;
            }
            else
            {
                if(!IsRunningInTestMode) Debug.LogWarning($"CardGameObject is null {card}");
            }

            /* Depending on the destination spot, record various offsets to the desired card positions */
            switch (spot.area)
            {
                case PlayfieldArea.STOCK:
                    faceUp = false; // always face down when adding to stock
                    stockCardPile.Add(card.GetSuitRank());
                    // todo: move this per-pile offset logic into the different pile types
                    // like stockCardPile.GetGoalIDAsCard(card) // returns the proper goal for the card based on it's position in the pile
                    if (goalID != null && stockCardPile.gameObject != null)
                    {
                        Vector3 offset = new(0, 0, (-0.1f) + (stockCardPile.Count * -CARD_THICKNESS));
                        goalID = new GoalIdentity(card.gameObject, stockCardPile.gameObject, offset);
                    }
                    break;

                    
                case PlayfieldArea.WASTE:
                    wasteCardPile.Add(card.GetSuitRank());
                    if (goalID != null && wasteCardPile.gameObject != null)
                    {
                        /*gPos = wasteCardPile.gameObject.transform.TransformPoint(Vector3.forward * (wasteCardPile.Count * -CARD_THICKNESS));
                        goalID.position = gPos;*/

                        Vector3 offset = new(0, 0, -0.1f + (wasteCardPile.Count * -CARD_THICKNESS));
                        goalID = new GoalIdentity(card.gameObject, wasteCardPile.gameObject, offset);

                    }
                    break;

                    
                case PlayfieldArea.FOUNDATION:
                    foundationCardPileGroup[spot.index].Add(card.GetSuitRank());
                    var baseGO = foundationCardPileGroup[spot.index].gameObject;
                    Debug.LogWarning($"moving to foundation pile: {spot.index} {baseGO?.name} {foundationCardPileGroup[spot.index].gameObjectType}");
                    if (goalID != null && baseGO != null)
                    {
                        goalID = new GoalIdentity(card.gameObject, baseGO, new Vector3(
                            0,
                            0,
                            -0.1f + (foundationCardPileGroup[spot.index].Count * -CARD_THICKNESS)
                        ));
                    }
                    break;

                    
                case PlayfieldArea.TABLEAU:
                    tableauCardPileGroup[spot.index].Add(card.GetSuitRank());
                    if (goalID != null && tableauCardPileGroup[spot.index].gameObject != null)
                    {
                        /*gPos = tableauCardPileGroup[spot.index].gameObject.transform.position;
                        gPos.z = (tableauCardPileGroup[spot.index].Count) * -CARD_THICKNESS - .02f;;
                        gPos.y = (tableauCardPileGroup[spot.index].Count) * -TAB_VERT_OFFSET;
                        goalID.position = gPos;*/
                        goalID = new GoalIdentity(card.gameObject, tableauCardPileGroup[spot.index].gameObject, new Vector3(
                            0,
                            (tableauCardPileGroup[spot.index].Count) * -TAB_VERT_OFFSET,
                            -0.1f + (tableauCardPileGroup[spot.index].Count) * -CARD_THICKNESS
                        ));
                    }
                    break;

                    
                case PlayfieldArea.HAND:
                    faceUp = true; // always face up when adding to hand
                    handCardPile.Add(card.GetSuitRank());
                    iDebug.Log("added card to hand " + handCardPile.Count);
                    //var hand = GetGameObjectByType(SolitaireGameObject.Hand_Base);
                    if (goalID != null && card.gameObject != null && handCardPile.gameObject != null)
                    {
                        goalID = new GoalIdentity(card.gameObject, handCardPile.gameObject, new Vector3(0.0f, -1.0f, 0.0f));
                    }
                    break;

                    
                case PlayfieldArea.DECK:
                    deck.AddCardToDeck(card.GetSuitRank());
                    if (goalID != null && card.gameObject != null && deck.DeckCardPile.gameObject != null)
                    {
                        goalID = new GoalIdentity(card.gameObject, deck.DeckCardPile.gameObject);
                    }
                    break;
            }

            // Define desired rotation
            // note: z-axis rotation is temporary until i fix the orientation of the mesh in blender
            Quaternion[] options = new Quaternion[2] { Quaternion.Euler(0, 0, 90), CARD_DEFAULT_ROTATION };
            if(goalID != null)
            {
                goalID.rotation = (spot.area == PlayfieldArea.HAND) ? (faceUp ? options[0] : options[1]) : (faceUp ? options[1] : options[0]);
                goalID.SetUseCustomRotation(true); // use our custom rotation instead of a goalObjects (if applicable) rotation
                goalID.SetDelay(delay);
            }
            card.SetGoalIdentity(goalID);
            card.SetPlayfieldSpot(spot);
            card.SetIsFaceUp(faceUp);
        }

        public void RemoveCardFromCurrentPlayfieldSpot(SolitaireCard card)
        {
            string dbugstring = $"{card}";

#nullable enable
            if (card.playfieldSpot.area == PlayfieldArea.INVALID)
            {
                iDebug.Log("card has no playfield spot. skipping list removal attempt");
                return;
            }
            
            SolitaireCardPile pile = SolitaireCardPile.EMPTY;

            PlayfieldSpot pfspot = card.playfieldSpot;
            switch (pfspot.area)
            {
                case PlayfieldArea.STOCK:
                    pile = stockCardPile;
                    break; ;
                case PlayfieldArea.DECK:
                    // removing it from an immutable clone won't help much, call deck directly
                    deck.RemoveCardFromDeck(card.GetSuitRank());
                    return;
                    //break;
                case PlayfieldArea.HAND:
                    pile = handCardPile;
                    break;
                case PlayfieldArea.WASTE:
                    pile = wasteCardPile;
                    break;
                case PlayfieldArea.FOUNDATION:
                    pile = foundationCardPileGroup[pfspot.index];
                    break;
                case PlayfieldArea.TABLEAU:
                    pile = tableauCardPileGroup[pfspot.index];
                    break;
            }

            if (pile == null)
            {
                iDebug.LogWarning($"no matching pile found for playfield spot {pfspot}");
                return;
            }

            if (pile.Count > 0)
            {
                string pilename = Enum.GetName(typeof(PlayfieldArea), pfspot.area);
                int index = pile.IndexOf(card.GetSuitRank());

                int topIndex = pile.Count - 1;
                if (pfspot.area == PlayfieldArea.STOCK || pfspot.area == PlayfieldArea.WASTE)
                {
                    if (index != topIndex)
                    {
                        // note, this isn't ALWAYS an error
                        // example, when passing waste cards back to stock
                        iDebug.LogWarning($"warn: trying to remove NON-TOP card from Stock or Waste pile.indexof(card):{index} topIndex:{topIndex}");
                    }
                }

                if (
                    pfspot.area == PlayfieldArea.TABLEAU 
                    || pfspot.area == PlayfieldArea.FOUNDATION
                )
                {
                    if (pfspot.subindex != index)
                    {
                        iDebug.LogWarning($"card {card} | subindex does not match pile.indexof(card){index} spot.subindex:{pfspot.subindex}");
                    }
                }

                if (index > -1)
                {
                    iDebug.LogWarning($"removing card from {pilename} {dbugstring}");
                    pile.RemoveAt(index);
                }
                else
                {
                    iDebug.LogWarning($"unable to find card in {pilename} {dbugstring}");
                }
            }
        }

        public void PickUpCards(PlayingCardIDList cards)
        {
            if(handCardPile.Count > 0)
            {
                iDebug.LogError("hand card pile is not empty, refusing to pick up cards...");
                return;
            }
            
            iDebug.LogWarning("picking up cards: " + cards.Count);
            foreach (SuitRank id in cards)
            {
                iDebug.Log(id.ToString());
            }
            
            int i = 0;
            // set the goal identity of each card to the hand
            foreach (SuitRank id in cards)
            {
                PlayfieldSpot handSpot = new PlayfieldSpot(PlayfieldArea.HAND, i);
                SolitaireCard card = deck.GetCardBySuitRank(id);
                MoveCardToNewSpot(ref card, handSpot, card.IsFaceUp);
                i++;
            }
        }

        /**
         TODO: verify face-up-ness
        */
        public PlayingCardIDList CollectSubStack(SolitaireCard card)
        {
            if (card.playfieldSpot.area == PlayfieldArea.INVALID)
            {
                iDebug.LogError("[CollectSubStack] got card with invalid spot");
                return PlayingCardIDList.EMPTY;
            }
            else
            {
                PlayfieldSpot spot = card.playfieldSpot;
                PlayingCardIDList cardGroup = new PlayingCardIDList(1) { card.GetSuitRank() };
                Debug.Log($"collect substack {card}");
                TableauCardPile pile = tableauCardPileGroup[spot.index];

                int cardIndex = pile.IndexOf(card.GetSuitRank());
                Debug.Log($"card index: {cardIndex}");
                if(cardIndex == -1)
                {
                    Debug.LogError($"card not found in pile {card} | tabPile: {spot.index}");
                }
                else
                {
                    if (cardIndex == pile.Count - 1)
                    {
                        return cardGroup; // top card? picking up single card
                    }
                
                    // else, multi-card: get the rest of the cards (if any) at a higher index in the list
                    for (int i = cardIndex; i < pile.Count; i++)
                    {
                        cardGroup.Add(pile[i]);
                    }
                }

                string msg = "picked up cards: ";
                foreach (SuitRank id in cardGroup)
                {
                    msg += "\n" + id.ToString();
                }
                Debug.Log(msg);

                return cardGroup;
            }
        }

        public void UpdateGameObjectReferences()
        {
            Debug.LogWarning("[UpdatingGameObjectReferences] IsRunningInTestMode:"+IsRunningInTestMode);
            gameObjectReferences = new Dictionary<SolitaireGameObject, GameObject>();

            if (IsRunningInTestMode)
            {
                Debug.LogWarning("[Skipping updating references]");
                return;
            }
            
            // TODO Change to FindObjectOfType
            var stock = GameObject.Find("PlayPlane/PlayPlaneOffset/Stock");
            if (stock == null)
                iDebug.LogError("stock not found");
            else
                gameObjectReferences[SolitaireGameObject.Stock_Base] = stock;

            var waste = GameObject.Find("PlayPlane/PlayPlaneOffset/Waste");
            if (waste == null)
                iDebug.LogError("waste not found");
            else
                gameObjectReferences[SolitaireGameObject.Waste_Base] = waste;

            var hand = GameObject.Find("Hand");
            if (hand == null)
                iDebug.LogError("hand not found");
            else
                gameObjectReferences[SolitaireGameObject.Hand_Base] = hand;

            var deckOfCards = GameObject.Find("DeckOfCards");
            if (deckOfCards == null)
            {
                Debug.LogError("DeckOfCards game object not found?!");
                iDebug.LogError("deckOfCards not found");
            }
            else
            {
                gameObjectReferences[SolitaireGameObject.Deck_Base] = deckOfCards;
            }

            Debug.LogWarning("gameObjectReferences count after update:" + gameObjectReferences.Count);
        }



        // TODO: make them suit agnostic until first Ace is placed
        public void BuildFoundations()
        {
            foundationCardPileGroup = new FoundationCardPileGroup();
            Assert.IsTrue(foundationCardPileGroup.Count == 4, "expect 4 piles in group");

            if(runningInTestMode)
            {
                return;
            }

            // Gather Game Object References:
            int i = 0;
            Vector3 foundationOrigin = Vector3.zero;
            foreach (int suit in Enum.GetValues(typeof(Suit)))
            {
                string suitName = Enum.GetName(typeof(Suit), suit);
                if(suitName.ToLower() == "none")
                {
                    continue;
                }
                string goName = "PlayPlane/PlayPlaneOffset/Foundations/" + suitName.ToLower();
                // TODO Change to FindObjectOfType
                GameObject foundation = GameObject.Find(goName);
                if (foundation == null)
                {
                    iDebug.LogError("foundation not found " + goName);
                    continue;
                }
                else
                {
                    string enumName = $"Foundation{i+1}_Base";
                    SolitaireGameObject.TryParse(enumName, out SolitaireGameObject solitaireGameObject);
                    gameObjectReferences[solitaireGameObject] = foundation;
                    
                    if (i == 0)
                    {
                        foundationOrigin = foundation.transform.localPosition;
                    }

                    // make sure the foundations are spaced evenly apart
                    Vector3 newPos = new Vector3(foundationOrigin.x, foundationOrigin.y, foundationOrigin.z);
                    newPos.x = i * (TAB_SPACING * 0.5f);
                    foundation.transform.localPosition = newPos;
                }
                i++;
            }
        }

        public void BuildTableaus()
        {
            tableauCardPileGroup = new TableauCardPileGroup();

            if (runningInTestMode)
            {
                return;
            }

            // Game Object Stuff...
            Vector3 tabOrigin = Vector3.zero;
            for (int i = 0; i < 7; i++)
            {
                // TODO Change to FindObjectOfType
                GameObject tab = GameObject.Find("PlayPlane/PlayPlaneOffset/Tableau/t" + i.ToString());
                //iDebug.Log("tab? " + (tab is null));

                string enumName = $"Tableau{i+1}_Base";
                SolitaireGameObject.TryParse(enumName, out SolitaireGameObject solitaireGameObject);
                gameObjectReferences[solitaireGameObject] = tab;

                if (tab != null)
                {
                    if (i == 0)
                    {
                        tabOrigin = new Vector3(
                            tab.transform.localPosition.x,
                            tab.transform.localPosition.y,
                            tab.transform.localPosition.z
                        );
                        //iDebug.LogWarning("tabOrigin " + tabOrigin);
                    }
                    else
                    {
                        // make sure the tableaus are evenly spaced apart
                        Vector3 newPos = new Vector3(tabOrigin.x, tabOrigin.y, tabOrigin.z);
                        newPos.x = TAB_SPACING * i;
                        //iDebug.LogWarning($"tabPos: {i} {newPos}");
                        tab.transform.localPosition = newPos; // tab.transform.InverseTransformPoint(newPos);
                    }
                }
            }
        }

        public string GetDebugText()
        {
            string textBlock = "Debug Output";

            textBlock += $"\n Move count {m_Moves}";

            if (stockCardPile != null)
            {
                textBlock += $"\n Stock count {stockCardPile.Count}";
            }

            if (wasteCardPile != null)
            {
                textBlock += $"\n Waste count {wasteCardPile.Count}";
            }

            if (handCardPile != null)
            {
                textBlock += $"\n Hand count {handCardPile.Count}";
            }

            if (foundationCardPileGroup != null)
            {
                textBlock += "\n";
                for (int i = 0; i < foundationCardPileGroup.Count; i++)
                {
                    textBlock += $"F:{i}:{foundationCardPileGroup[i].Count} ";
                }
            }

            if (tableauCardPileGroup != null)
            {
                textBlock += "\n";
                for (int i = 0; i < tableauCardPileGroup.Count; i++)
                {
                    textBlock += $"T:{i}:{tableauCardPileGroup[i].Count} ";
                }
            }

            return textBlock;
        }

        public void FlipCardFaceUp(SolitaireCard card)
        {
            GameObject cardGO = card.gameObject;
            Transform cardTX = cardGO.transform;
            // retain position, just flip over y axis
            // TODO: use rotate()
            card.SetGoalIdentity(new GoalIdentity(
                cardGO,
                cardTX.position, //+ Vector3.zero,
                cardTX.localRotation * Quaternion.Euler(0.0f, 180.0f, 0.0f),
                cardTX.localScale + Vector3.zero
            ));
            card.SetIsFaceUp(true);
        }

        public void TryPickupCardInSpot(PlayfieldSpot spot, SolitaireCard card)
        {

            iDebug.LogWarning($"try pickup card in spot {spot}");

            // pick up the card
            switch (spot.area)
            {
                case PlayfieldArea.TABLEAU:
                    // try and pick up one or more cards from a tab pile
                    //TableauCardPile tabPile = tableauCardPileGroup[spot.index];

                    if (card.IsFaceUp)
                    {
                        // if it IS face up, try to collect it and any additional cards aka substack
                        // into the hand
                        PlayingCardIDList subStack = CollectSubStack(card);
                        PickUpCards(subStack);
                    }
                    else
                    {
                        // did we try to click the top card?
                        SolitaireCard topCard = tableauCardPileGroup[spot.index].GetTopCard();
                        bool IsTopCard = card == topCard;
                        if(IsTopCard && !topCard.IsFaceUp)
                        {
                            FlipCardFaceUp(topCard);
                        }

                        // if they didn't click the top card, but rather, some other face-down card in the tableau
                        // just see if the top one is face down and flip it if not just in case
                        if (!IsTopCard)
                        {
                            if (!topCard.IsFaceUp)
                            {
                                FlipCardFaceUp(topCard);
                            }
                        }

                        // see if there's a face up card to grab?
                    }
                    return;
                    //break;

                case PlayfieldArea.STOCK:
                    
                    // Do a waste -> stock recycle maneuver if need be
                    if (stockCardPile.Count == 0)
                    {
                        iDebug.LogError("error picking up stock card, stock pile is empty");

                        if(wasteCardPile.Count > 0)
                        {
                            WasteToStock();
                            return;
                        }
                        else
                        {
                            iDebug.LogWarning("no cards in stock or waste? :G");
                        }
                        return;
                    }

                    // enforce only being able to pick up the topmost card:
                    card = stockCardPile.GetTopCard();

                    // move it to the waste pile; face up
                    MoveCardToNewSpot(ref card,
                        new PlayfieldSpot(PlayfieldArea.WASTE, wasteCardPile.Count), true);

                    return;

                case PlayfieldArea.FOUNDATION:
                    // enforce only being able to pick up the topmost card:
                    if (foundationCardPileGroup[spot.index].Count == 0)
                    {
                        iDebug.LogError("error picking up foundation card, foundation pile is empty");
                        return;
                    }
                    card = foundationCardPileGroup[spot.index].GetTopCard();
                    break;

                case PlayfieldArea.WASTE:
                    // enforce only being able to pick up the topmost card:
                    if (wasteCardPile.Count == 0)
                    {
                        iDebug.LogError("error picking up waste card, waste pile is empty");

                        // Do a stock to waste op here?
                        return;
                    }

                    card = wasteCardPile.GetTopCard();
                    break;
            }

            // this method will add the card to the handCards list
            MoveCardToNewSpot(ref card, new PlayfieldSpot(PlayfieldArea.HAND, 0), true);
        }

        public void TryPlaceHandCardToSpot(PlayfieldSpot spot)
        {
            if (handCardPile.Count < 1)
            {
                iDebug.LogWarning("no cards in hand.");
                return;
            }

            // we have a card in our hand, try placing it...
            SolitaireCard cardInHand = handCardPile.FirstCard();

            SolitaireMove move = new SolitaireMove(cardInHand, cardInHand.playfieldSpot, spot);
            // validate the move
            bool isValid = SolitaireMoveValidator.IsValidMove(
                GetGameState(),
                move
            );
            iDebug.LogWarning($"IsValidMove? {cardInHand.GetGameObjectName()} to {spot}: valid: {isValid}");

            if (isValid)
            {
                m_Moves++;
                // move it valid, execute it
                bool faceUp = true;

                int substackIndex = 0;
                SolitaireCard first_card = deck.GetCardBySuitRank(handCardPile.First());
                foreach (SuitRank id in handCardPile)
                {
                    SolitaireCard card = deck.GetCardBySuitRank(id);
                    // they will be removed from handCards list as they're added to the new destination spot
                    MoveCardToNewSpot(ref card, spot, faceUp, 0.1f * substackIndex, substackIndex);
                    substackIndex++;
                }
                CheckFlipOverTopCardInTableauCardJustLeft(first_card);
            }
            else
            {
                // move is invalid...
                iDebug.LogWarning("Invalid move, try again.");
            }
        }
        public void CheckFlipOverTopCardInTableauCardJustLeft(SolitaireCard card)
        {
            // refer back to the tableau we just came from and see if we need to auto-flip over a card
            if (card.previousPlayfieldSpot.area == PlayfieldArea.TABLEAU)
            {
                Debug.Log("CheckFlipOverTopCardInTableauCardJustLeft");
                TableauCardPile tPile = tableauCardPileGroup[card.previousPlayfieldSpot.index];
                if (tPile.Count > 0)
                {
                    SolitaireCard topMostCard = tPile.GetTopCard();
                    if (!topMostCard.IsFaceUp)
                    {
                        FlipCardFaceUp(topMostCard);
                    }
                    else
                    {
                        iDebug.LogWarning("tableau we left already had a face-up card as its top card?");
                    }
                }
                else
                {
                    iDebug.LogWarning("tableau we left is empty");
                }
            }
            else
            {
                Debug.Log("CheckFlipOverTopCardInTableauCardJustLeft: nothing to do. card prevSpot was not Tableau");
            }
        }

        public void OnSingleClickCardPileBase(MonoSolitaireCardPileBase pileBase)
        {
            switch (pileBase.playfieldArea)
            {
                case PlayfieldArea.STOCK:
                    if(handCardPile.Count == 0 && stockCardPile.Count < 1 && wasteCardPile.Count > 0)
                    {
                        WasteToStock();
                    }
                    break;
                case PlayfieldArea.WASTE:
                    if (handCardPile.Count > 0)
                    {
                        TryPlaceHandCardToSpot(new PlayfieldSpot(pileBase.playfieldArea, pileBase.index));
                    }
                    else if (stockCardPile.Count > 0)
                    {
                        StockToWaste();
                    }
                    break;
                case PlayfieldArea.FOUNDATION:
                    if(handCardPile.Count > 0)
                        TryPlaceHandCardToSpot(new PlayfieldSpot(pileBase.playfieldArea, pileBase.index));
                    break;
                case PlayfieldArea.TABLEAU:
                    if (handCardPile.Count > 0)
                        TryPlaceHandCardToSpot(new PlayfieldSpot(pileBase.playfieldArea, pileBase.index));
                    break;
            }
        }

        public void OnSingleClickCard(SolitaireCard card)
        {
            Debug.Log($"SolitaireGame@OnSingleClickCard {card}");

            if (card.playfieldSpot.area == PlayfieldArea.STOCK)
            {
                // send to waste
                StockToWaste();
                return;
            }

            if (card.playfieldSpot.area == PlayfieldArea.INVALID || card.playfieldSpot.area == PlayfieldArea.DECK)
            {
                Debug.LogError("invalid or deck card clicked, ignoring");
                return;
            }

            if (card.playfieldSpot.area == PlayfieldArea.HAND)
            {
                Debug.LogError("single-clicked card in hand.. could try to autoplace, but lets just ignore for now");
                // TODO; if you implement auto-place, make sure you place the 0th card in the hand, and let any other cards be placed down on top of the ideal spot
                // really clicking card in hand shouldn't be a thing right now tho.
                // maybe flinging it :D
                return;
            }

            // otherwise it's in Waste, Foundation, or Tableau, and we should try to auto-place it

            // make sure it's face up
            bool isTopCard = IsTopCardInPlayfieldSpot(card); // see if it's the top card in it's current playfield spot
            Debug.Log($"isTopCard? {isTopCard} isFaceUp? {card.IsFaceUp}");
            if (!card.IsFaceUp && card.playfieldSpot.area == PlayfieldArea.TABLEAU)
            {
                Debug.LogWarning("cannot move a face down card in a tableau, we can only flip it over, and it should've already flipped over");
                if (isTopCard)
                {
                    FlipCardFaceUp(card);
                    return;
                }
                else
                {
                    Debug.LogWarning("oh, it wasn't even the top card, yeah no, you can't act on this card. ignoring...");
                    return;
                }
            }

            // auto-place
            PlayfieldSpot next_spot = GetNextValidPlayfieldSpotForSuitRank(card.GetSuitRank());
            Debug.LogWarning($"single-click: autoplace: {card.GetSuitRank()} -> {next_spot}");
            if (next_spot.area == PlayfieldArea.INVALID)
            {
                iDebug.LogWarning("no valid spot found for card, ignoring");
                return;
            }
            else
            {
                if (card.IsFaceUp)
                {
                    PlayingCardIDList subStackCards = card.playfieldSpot.area == PlayfieldArea.TABLEAU ? CollectSubStack(card) : new(1) { card.GetSuitRank() };
                    foreach (SuitRank suitRank in subStackCards)
                    {
                        SolitaireCard hand_card = deck.GetCardBySuitRank(suitRank);
                        MoveCardToNewSpot(ref hand_card, next_spot, true);/* true = faceUp */
                    }
                    CheckFlipOverTopCardInTableauCardJustLeft(deck.GetCardBySuitRank(card.GetSuitRank()));
                }
            }
        }
        public void OnLongPressCard(SolitaireCard card)
        {
            iDebug.Log("OnLongPressCard handCards Count " + card + " | handCardsCount:" + handCardPile.Count);
            if (handCardPile.Count < 1)
            {
                TryPickupCardInSpot(card.playfieldSpot, card);
            }
            else
            {
                TryPlaceHandCardToSpot(card.playfieldSpot);
            }
        }

        // TODO: track in Move Log for undo/redo
        public void StockToWaste()
        {
            // if no cards left in stock, return
            if (stockCardPile.Count < 1)
            {
                iDebug.LogWarning("StockToWaste: No cards in Stock pile");
                //call WasteToStock(); here???
                return;
            }
            // take top card (0th) from stockCards list, remove it, and append it to the Waste pile
            // TODO: support 3 at a time mode
            SuitRank cardSuitRank = stockCardPile[0];

            // this now happens inside SetCardGoal
            //stockCardPile.RemoveAt(0);

            SolitaireCard card = deck.GetCardBySuitRank(cardSuitRank);
            MoveCardToNewSpot(ref card, new PlayfieldSpot(PlayfieldArea.WASTE, wasteCardPile.Count), true);
        }

        public void WasteToStock()
        {
            // Flag Op Start (for move validation)
            IsRecyclingWasteToStock = true;
            
            // if no cards left in waste, return
            if (wasteCardPile.Count < 1)
            {
                iDebug.LogWarning("WasteToStock: No cards in Waste pile");
                return;
            }
            // return all wasteCards to the stockCards list (in reverse order)
            int order_i = 0;
            int countAtStartOfOperation = wasteCardPile.Count;
            Assert.IsTrue(stockCardPile.Count == 0);
            for (int i = wasteCardPile.Count - 1; i > -1; i--)
            {
                PlayfieldSpot stockSpot = new PlayfieldSpot(PlayfieldArea.STOCK, order_i);

                SolitaireCard card = deck.GetCardBySuitRank(wasteCardPile[i]);
                MoveCardToNewSpot(ref card, stockSpot, false);
                order_i++;
            }
            // verify the waste card pile is empty
            Assert.IsTrue(wasteCardPile.Count == 0);
            Assert.IsTrue(stockCardPile.Count == countAtStartOfOperation);

            // Flag Op End
            IsRecyclingWasteToStock = false;
        }

        public bool IsTopCardInPlayfieldSpot(SolitaireCard card)
        {
            switch (card.playfieldSpot.area)
            {
                case PlayfieldArea.TABLEAU:
                    if (tableauCardPileGroup[card.playfieldSpot.index].Count > 0)
                    {
                        var topCard = tableauCardPileGroup[card.playfieldSpot.index].GetTopCard();
                        Debug.Log($"IsTopCardInPlayfieldSpot {card.playfieldSpot} | topCard: {topCard}");
                        if (topCard == card)
                            return true;
                        else
                            Debug.LogWarning("is not top card!");
                    }
                    break;

                case PlayfieldArea.FOUNDATION:
                    if (foundationCardPileGroup[card.playfieldSpot.index].Count > 0)
                    {
                        SolitaireCard topCard = GetTopCardForFoundation(card.playfieldSpot.index);
                        if (topCard == card)
                            return true;
                    }
                    break;

                case PlayfieldArea.WASTE:
                    if (wasteCardPile.Count > 0)
                    {
                        SolitaireCard topCard = wasteCardPile.GetTopCard();
                        if (topCard == card)
                            return true;
                    }
                    break;
            }
            return false;
        }

        internal void OnDoubleClickCard(SolitaireCard card)
        {
            if (!card.IsFaceUp)
            {
                iDebug.Log("Ignoring double-click on face-down card");
                return;
            }
            else
            {
                iDebug.Log("Ignoring double-click on face-up card. did you expect autoplace?");
            }

        }
    }
}
