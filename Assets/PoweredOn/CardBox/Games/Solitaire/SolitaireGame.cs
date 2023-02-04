using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.CardBox.Animations;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Games.Solitaire.Piles;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireGame
    {
        SolitaireDeck deck;
        public SolitaireDeck BuildDeck()
        {
            deck = new SolitaireDeck();
            return deck;
        }

        public const float TAB_SPACING = 0.1f;
        const float RANDOM_AMPLITUDE = 1f;
        public const float CARD_THICKNESS = 0.0002f;
        public const float TAB_VERT_OFFSET = 0.01f;

        // z-axis rotation is temporary until i fix the orientation of the mesh in blender
        public Quaternion CARD_DEFAULT_ROTATION = Quaternion.Euler(0, 180, 90);

        List<GameObject> foundations;
        List<GameObject> tableaus;
        GameObject stock;
        GameObject waste;
        GameObject hand;
        GameObject deckOfCards;

        PlayingCardIDList dealtOrder;
        PlayingCardIDList deckOrder;
        PlayingCardIDListGroup tableauCards;
        PlayingCardIDList wasteCards;
        PlayingCardIDList stockCards;
        PlayingCardIDList handCards;
        PlayingCardIDList deckCards;

        //PlayingCardIDListGroup foundationCards; // old
        FoundationCardPileGroup foundationCardPileGroup; // new

        private bool autoplaying = false;

        bool autoPlaceEnabled = true;

        // card, from, to
        SolitaireMoveList moveLog;

        public DebugOutput m_DebugOutput;

        int m_Moves = 0;
        public SolitaireGame()
        {
            UpdateGameObjectReferences();
        }

        /*
         * Reinitialize all of our tracking variables
         * 
         */
        public void Reset()
        {
            m_Moves = 0;
            moveLog = new SolitaireMoveList();
            stockCards = new PlayingCardIDList();
            wasteCards = new PlayingCardIDList();
            
            handCards = new PlayingCardIDList();
            dealtOrder = new PlayingCardIDList();
            deckCards = SolitaireDeck.DEFAULT_DECK_ORDER;

            BuildFoundations();
            BuildTableaus();
            BuildDeck();
        }

        /*public struct Move
        {
            public SolitaireCard card;
            public PlayfieldSpot from;
            public PlayfieldSpot to;
            public Move(SolitaireCard card, PlayfieldSpot from, PlayfieldSpot to)
            {
                this.card = card;
                this.from = from;
                this.to = to;
            }
        }*/

        public PlayingCardIDList GetStockCardsImmutable()
        {
            return new PlayingCardIDList(stockCards);
        }

        public PlayingCardIDListGroup GetTableauListImmutable()
        {
            return tableauCards.Clone();
        }

        public PlayingCardIDList GetWasteCardsImmutable()
        {
            return new PlayingCardIDList(wasteCards);
        }

        public PlayingCardIDList GetHandCardsImmutable()
        {
            return new PlayingCardIDList(handCards);
        }

        /*public PlayingCardIDListGroup GetFoundationCardsImmutable()
        {
            return new PlayingCardIDListGroup(foundationCards);
        }*/

        public PlayingCardIDList GetDealtOrderImmutable()
        {
            return new PlayingCardIDList(dealtOrder);
        }

        public FoundationCardPile GetFoundationCardPileForSuit(Suit suit)
        {
            return this.foundationCardPileGroup.GetFoundationCardPileForSuit(suit);
        }

        public void NewGame()
        {
            UpdateGameObjectReferences();
            Reset();
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
        public void Deal()
        {
            NewGame();

            // first, we want to collect all the cards into a stack
            SetCardGoalsToDeckPositions();

            //await Task.Delay(1000);

            // let's shuffle the card order 3 times (todo: artifically delay and animate this)
            deck.Shuffle(3);

            //await Task.Delay(1000);

            if (deck.cards.Count != 52)
                throw new Exception($"[DEAL] invalid deck card count after shuffling {deck.cards.Count}");

            if (deck.deckOrderList.Count != 52)
                throw new Exception($"[DEAL] invalid deck deckOrderList count after shuffling {deck.deckOrderList.Count}");

            stockCards = new PlayingCardIDList(new List<SuitRank>());
            for (int initial_loop_index = 0; initial_loop_index < 52; initial_loop_index++)
            {
                // capture all SuitRanks in the "stockCards" pile to begin with
                /*try
                {*/
                SolitaireCard card = deck.GetCardBySuitRank(deck.deckOrderList[initial_loop_index]);
                SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.STOCK, initial_loop_index), false);
                /*}
                catch (Exception e)
                {
                    DebugOutput.Instance.LogError(e.ToString());
                }*/
            }

            if (stockCards.Count != 52)
            {
                throw new Exception($"[DEAL] invalid stock card count after shuffling and moving from deck to stockCards list {stockCards.Count}");
            }

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
                    SuitRank suitRankToDeal = stockCards[0];

                    SolitaireCard card = deck.GetCardBySuitRank(suitRankToDeal);

                    bool faceUp = pile == round;

                    dealtOrder.Add(suitRankToDeal);

                    // NOTE: inside this method, we handle adding SuitRank to the proper Tableau list
                    // we also handle removing it from the previous spot (PlayfieldArea.Stock)
                    SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Tableau, pile, round), faceUp, 0.1f * dealtOrder.Count);

                    //DebugOutput.Instance.Log($"Dealing {dealtOrder.Count}: {card} | round:{round} pile:{pile} faceup:{faceUp}");
                }
            }

            if (dealtOrder.Count != 28)
            {
                throw new Exception($"[DEAL] invalid dealtOrder count after moving cards from stock to tableau {stockCards.Count}");
            }

            // then, for the remaining cards, update their GoalIdentity to place them where the "Stock" Pile should go
            // remember to offset the local Z position a bit to make the cards appear as tho they're stacked on top of each other.
            // the last card should be the lowest z-position (same z as the stock pile guide object) and then each card up to the 0th should be the highest
            // so we should loop backwards through the remaining Stock when setting the positions
            if (stockCards.Count != 24)
            {
                throw new Exception($"[DEAL] invalid stock card count after moving cards from stock to tableau {stockCards.Count}");
            }
            for (int sc = stockCards.Count - 1; sc > -1; sc--)
            {
                SuitRank cardSuitRank = stockCards[sc];
                SolitaireCard card = deck.GetCardBySuitRank(cardSuitRank);
                // NOTE: inside this method we handle adding SuitRank to the stockCards list
                SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Stock, sc), false); /* always face down when adding to stock */
            }
        }

        // SetCardGoalsToDeckPositions is called when we want to collect all the cards into a stack
        // This GameManager class lives on the parent GameObject, and has all the card GameObjects as children
        // we want to use the GameManager's transform as the local zero position for the center of bottommost card
        // each card should be given a goal position, offset by a positive .0002 m on the local y for each card "below" it in the deck,
        // so the topmost card would have a y position of .0002 (card thickness) * 51 (51 cards below it)
        // the deckOrder property (GetDeckOrder) of each Card is what determines this offset, and the 0th card is the bottom,
        // with the 51st deck position being the top
        public void SetCardGoalsToDeckPositions()
        {
            // get the deck world position
            Vector3 deckPosition = deckOfCards.transform.position + Vector3.zero;

            // loop through our cards, and give them a new GoalIdentity based on our calculations
            foreach (SolitaireCard card in deck.cards)
            {
                Transform cardTransform = card.GetGameObject().transform;

                cardTransform.position = Vector3.zero;
                cardTransform.rotation = Quaternion.identity;

                // get the deck order of the current card
                int deckOrder = card.GetDeckOrder();

                // calculate the goal position of the current card
                // what effect does applying this transform BEFORE the offset have, vs the opposite.
                Vector3 worldToLocal = cardTransform.InverseTransformPoint(deckPosition);
                worldToLocal += new Vector3(0, 0, CARD_THICKNESS * deckOrder);

                GoalIdentity goalID = new GoalIdentity(
                    card.GetGameObject(),
                    worldToLocal,
                    cardTransform.localRotation,
                    cardTransform.localScale + Vector3.zero);

                // set the goal position of the current card
                card.SetGoalIdentity(goalID);
                card.SetIsFaceUp(false);
            }
        }

        public void SetCardGoalsToRandomPositions()
        {
            // get the deck world position
            Vector3 deckPosition = deckOfCards.transform.position + Vector3.zero;

            // loop through our cards, and give them a new GoalIdentity based on our calculations
            foreach (SolitaireCard card in deck.cards)
            {
                Transform cardTransform = card.GetGameObject().transform;

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
                    card.GetGameObject(),
                    worldToLocal,
                    cardTransform.localRotation,
                    cardTransform.localScale);

                // set the goal position of the current card
                card.SetGoalIdentity(goalID);
                card.SetIsFaceUp(true);
            }
        }

        // when the user double-clicks on a card, we auto-move it to the next best spot
        public PlayfieldSpot GetNextValidPlayfieldSpotForSuitRank(SuitRank suitrank)
        {

            // check the top card of the foundation first
            DebugOutput.Instance.LogWarning("[GetNextValidPlayfieldSpotForSuitRank] Checking foundation card list for rank: " + suitrank.rank + " suit int: " + (int)suitrank.suit + " " + foundationCards.Count);
            PlayingCardIDList foundationList = foundationCards[(int)suitrank.suit];
            SuitRank topCardSR;
            DebugOutput.Instance.LogWarning("[GetNextValidPlayfieldSpotForSuitRank] Foundation List: " + foundationList.Count);
            if (foundationList.Count == 0)
            {
                // if the foundation list is empty, that's our first choice spot
                // if the rank is Rank.ace (0), return the foundation pile for the suit
                if (suitrank.rank == Rank.ACE) { return new PlayfieldSpot(PlayfieldArea.FOUNDATION, (int)suitrank.rank, 0); }
            }
            else
            {
                topCardSR = foundationList.Last();
                DebugOutput.Instance.LogWarning($"[GetNextValidPlayfieldSpotForSuitRank] checking if top card in foundation list is one rank less than the card we are trying to place {(int)topCardSR.rank} {(int)suitrank.rank}");
                if ((int)topCardSR.rank + 1 == (int)suitrank.rank)
                {
                    // if the top card in the foundation list is one less than this card's rank, that's our first choice spot
                    return new PlayfieldSpot(PlayfieldArea.FOUNDATION, (int)suitrank.suit, foundationList.Count);
                }
            }

            // next, loop through the tableau piles from left to right and see if there's a valid spot for this card to go to
            for (int i = 0; i < 7; i++)
            {
                if (tableauCards[i].Count == 0)
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
                    topCardSR = tableauCards[i].Last();
                    if (
                       (int)topCardSR.rank - 1 == (int)suitrank.rank
                       && SolitaireDeck.SuitColorsAreOpposite(topCardSR.suit, suitrank.suit)
                    )
                    {
                        return new PlayfieldSpot(PlayfieldArea.TABLEAU, i, tableauCards[i].Count);
                    }
                }
            }

            DebugOutput.Instance.LogWarning($"no suggested playfield spot found for {suitrank.ToString()}");

            return PlayfieldSpot.INVALID;
        }

        public bool CheckHandToPlayfieldMoveIsValid(SolitaireCard handCard, PlayfieldSpot destinationSpot)
        {
            // first validate the move is valid
            DebugOutput.Instance.LogWarning("CheckHandToPlayfieldMoveIsValid " + handCard.GetGameObjectName() + " to " + destinationSpot.ToString());

            // placing it back down
            if (
                handCard.previousPlayfieldSpot.area != PlayfieldSpot.INVALID)
            {
                if (handCard.previousPlayfieldSpot.area == destinationSpot.area)
                {
                    DebugOutput.Instance.LogWarning($"valid to place card back down where it just came from {handCard.previousPlayfieldSpot}");
                    return true;
                }
            }

            switch (destinationSpot.area)
            {
                case PlayfieldArea.HAND:
                    if (handCards.Count > 0)
                    {
                        DebugOutput.Instance.LogWarning("already have a card in your hand, cant hold more than one unless you pick up a substack on a tableau");
                        return false;
                    }
                    return true;

                case PlayfieldArea.STOCK:
                    DebugOutput.Instance.LogWarning("invalid dest: Stock. cannot move card back to stock");
                    return false;

                case PlayfieldArea.WASTE:
                    DebugOutput.Instance.LogWarning("todo: if card came from top of waste, we should've already allowed putting it back");
                    break;

                case PlayfieldArea.TABLEAU:
                    PlayingCardIDList tabCardList = tableauCards[destinationSpot.index];
                    DebugOutput.Instance.LogWarning("tabCardList Count" + tabCardList.Count);
                    if (tabCardList.Count == 0)
                    {
                        // tableau is empty, kings only
                        if (handCard.GetRank() == Rank.KING)
                        {
                            // king is valid
                            return true;
                        }
                        DebugOutput.Instance.LogWarning("invalid dest: cannot place non-king cards in empty tab spots");
                    }
                    else
                    {
                        // validate the top card in the tableau is one rank higher, and opposite suit
                        SuitRank topCardInTabSuitRank = tabCardList.Last();
                        SolitaireCard topCardInTab = deck.GetCardBySuitRank(topCardInTabSuitRank);
                        bool suitsAreOpposite = SolitaireDeck.SuitColorsAreOpposite(
                                handCard.GetSuit(),
                                topCardInTab.GetSuit()
                                );
                        DebugOutput.Instance.LogWarning($"comparing ranks:\n" +
                            $"topcard rank: {(int)topCardInTab.GetRank()}\n" +
                            $"handcard rank: {(int)handCard.GetRank()}\n" +
                            $"suits are opposite ${suitsAreOpposite}");
                        if (
                            (int)topCardInTab.GetRank() == ((int)handCard.GetRank() + 1)
                            && suitsAreOpposite
                        )
                        {
                            // card we're trying to place on top of is valid
                            return true;
                        }
                        DebugOutput.Instance.LogWarning($"invalid dest: cannot place {handCard.GetGameObjectName()} on {topCardInTab.GetGameObjectName()} ");
                    }
                    break;
                case PlayfieldArea.FOUNDATION:
                    DebugOutput.Instance.LogWarning($"trying to place {handCard.GetGameObjectName()} on {destinationSpot}");
                    PlayingCardIDList fCardList = foundationCards[destinationSpot.index];
                    if (fCardList.Count < 1)
                    {
                        // empty, only aces are valid
                        DebugOutput.Instance.LogWarning($"suit int: {(int)handCard.GetSuit()} dest index:{destinationSpot.index}");
                        if ((int)handCard.GetSuit() == destinationSpot.index && handCard.GetRank() == Rank.ACE)
                        {
                            return true;
                        }
                        DebugOutput.Instance.LogWarning($"invalid dest: cannot place {handCard.GetGameObjectName()} in empty foundation @ {destinationSpot.index}. aces only.");
                    }
                    else
                    {
                        // validate the top card in the foundation is one rank lower than handCard and the SAME suit
                        SuitRank topCardInFoundationSuitRank = fCardList.Last();
                        SolitaireCard topCardInFoundation = deck.GetCardBySuitRank(topCardInFoundationSuitRank);
                        if (
                            (int)handCard.GetSuit() == (int)topCardInFoundation.GetSuit()
                            && (int)handCard.GetRank() == (int)topCardInFoundation.GetRank() + 1
                        )
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false; // default to fales
        }

        public void SetCardGoalIDToPlayfieldSpot(SolitaireCard card, PlayfieldSpot spot, bool faceUp, float delay = 0.0f)
        {
            //Debug.Log($"SetCardGoalIDToPlayfieldSpot {card.GetGameObjectName()} to {spot} faceup:{faceUp} delay:{delay}");
            moveLog.Add(new SolitaireMove(card, card.previousPlayfieldSpot, spot));

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
                    DebugOutput.Instance.LogError(e.ToString());
                }
            }


            GoalIdentity goalID = new(card.GetGameObject(), Vector3.zero, Quaternion.identity, Vector3.one);
            Transform cardTX = card.GetGameObject().transform;
            Vector3 gPos = Vector3.zero;

            switch (spot.area)
            {
                case PlayfieldArea.STOCK:
                    faceUp = false; // always face down when adding to stock
                    stockCards.Add(card.GetSuitRank());
                    gPos = stock.transform.position;
                    gPos.z = (-0.1f) + (stockCards.Count * -CARD_THICKNESS);
                    Debug.Log($"stockCardsCountNow {stockCards.Count} z:{gPos.z}");
                    goalID.SetGoalPositionFromWorldPosition(gPos);
                    break;
                case PlayfieldArea.WASTE:
                    wasteCards.Add(card.GetSuitRank());
                    gPos = waste.transform.position;
                    goalID.SetGoalPositionFromWorldPosition(gPos);
                    goalID.position = new Vector3(
                        goalID.position.x,
                        goalID.position.y,
                        goalID.position.z + (wasteCards.Count * -CARD_THICKNESS)
                    );
                    break;
                case PlayfieldArea.FOUNDATION:
                    foundationCards[spot.index].Add(card.GetSuitRank());
                    GameObject foundation = foundations[spot.index];
                    gPos = foundation.transform.position;
                    gPos.z = foundationCards[spot.index].Count * CARD_THICKNESS;
                    goalID.SetGoalPositionFromWorldPosition(gPos);
                    break;
                case PlayfieldArea.TABLEAU:
                    tableauCards[spot.index].Add(card.GetSuitRank());
                    GameObject tableau = tableaus[spot.index];
                    gPos = tableau.transform.position;
                    gPos.z = (tableauCards[spot.index].Count) * -CARD_THICKNESS;
                    gPos.y = (tableauCards[spot.index].Count) * -TAB_VERT_OFFSET + .02f;
                    //goalID.SetGoalPositionFromWorldPosition(gPos);
                    goalID.position = gPos;// cardTX.InverseTransformPoint(gPos);
                    break;
                case PlayfieldArea.HAND:
                    faceUp = true; // always face up when adding to hand
                    handCards.Add(card.GetSuitRank());
                    DebugOutput.Instance.Log("added card to hand " + handCards.Count);
                    goalID = new GoalIdentity(card.GetGameObject(), hand, new Vector3(0.0f, -1.0f, 0.0f));
                    break;
                case PlayfieldArea.DECK:
                    deckCards.Add(card.GetSuitRank());
                    goalID = new GoalIdentity(card.GetGameObject(), deckOfCards);
                    break;
            }

            // z-axis rotation is temporary until i fix the orientation of the mesh in blender
            Quaternion[] options = new Quaternion[2] { Quaternion.Euler(0, 0, 90), CARD_DEFAULT_ROTATION };
            goalID.rotation = (spot.area == PlayfieldArea.HAND) ? (faceUp ? options[0] : options[1]) : (faceUp ? options[1] : options[0]);
            goalID.SetDelay(delay);
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
                DebugOutput.Instance.Log("card has no playfield spot. skipping list removal attempt");
                return;
            }
            
            PlayingCardIDList? pile = null;

            PlayfieldSpot pfspot = card.playfieldSpot;
            switch (pfspot.area)
            {
                case PlayfieldArea.STOCK:
                    pile = stockCards;
                    break; ;
                case PlayfieldArea.DECK:
                    pile = deckCards;
                    break;
                case PlayfieldArea.HAND:
                    pile = handCards;
                    break;
                case PlayfieldArea.WASTE:
                    pile = wasteCards;
                    break;
                case PlayfieldArea.FOUNDATION:
                    pile = foundationCards[pfspot.index];
                    break;
                case PlayfieldArea.TABLEAU:
                    pile = tableauCards[pfspot.index];
                    break;
            }

            if (pile == null)
            {
                DebugOutput.Instance.LogWarning($"no matching pile found for playfield spot {pfspot}");
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
                        DebugOutput.Instance.LogWarning($"warn: trying to remove NON-TOP card from Stock or Waste pile.indexof(card):{index} topIndex:{topIndex}");
                    }
                }

                if (pfspot.area == PlayfieldArea.TABLEAU || pfspot.area == PlayfieldArea.FOUNDATION)
                {
                    if (pfspot.subindex != index)
                    {
                        DebugOutput.Instance.LogWarning($"card {card} | subindex does not match pile.indexof(card){index} spot.subindex:{pfspot.subindex}");
                    }
                }

                if (index > -1)
                {
                    DebugOutput.Instance.LogWarning($"removing card from {pilename} {dbugstring}");
                    pile.RemoveAt(index);
                }
                else
                {
                    DebugOutput.Instance.LogWarning($"unable to find card in {pilename} {dbugstring}");
                }
            }
        }

        public void PickUpCards(PlayingCardIDList cards)
        {
            DebugOutput.Instance.LogWarning("picking up cards: " + cards.Count);
            foreach (SuitRank id in cards)
            {
                DebugOutput.Instance.LogWarning(id.ToString());
            }
            // empty list (will be populated by calls to SetCardGoalID...)
            handCards = new PlayingCardIDList();
            int i = 0;
            // set the goal identity of each card to the hand
            foreach (SuitRank id in cards)
            {
                PlayfieldSpot handSpot = new PlayfieldSpot(PlayfieldArea.Hand, i);
                SolitaireCard card = deck.GetCardBySuitRank(id);
                SetCardGoalIDToPlayfieldSpot(card, handSpot, card.IsFaceUp);
                i++;
            }
        }

        public PlayingCardIDList CollectSubStack(SolitaireCard card)
        {
            if (card.playfieldSpot.area == PlayfieldArea.INVALID)
            {
                throw new Exception("got card with Invalid playfield spot");
            }
            else
            {
                PlayfieldSpot spot = card.playfieldSpot;
                PlayingCardIDList cardGroup = new PlayingCardIDList(1) { card.GetSuitRank() };
                PlayingCardIDList tableauCardList = tableauCards[spot.index];

                int cardIndex = tableauCardList.IndexOf(card.GetSuitRank());
                if (cardIndex == tableauCardList.Count - 1)
                {
                    return cardGroup; // single card
                }
                // get the rest of the cards (if any) at a higher index in the list
                for (int i = cardIndex; i < tableauCardList.Count; i++)
                {
                    cardGroup.Add(tableauCardList[i]);
                }

                DebugOutput.Instance.Log("picked up cards: ");
                foreach (SuitRank id in cardGroup)
                {
                    DebugOutput.Instance.Log(id.ToString());
                }

                return cardGroup;
            }
        }

        public void UpdateGameObjectReferences()
        {
            var debugOutputGO = GameObject.Find("DebugOutput");
            if (debugOutputGO == null)
            {
                Debug.LogError("DebugOutput Game Object not found");
            }
            else
            {
                m_DebugOutput = debugOutputGO.GetComponent<DebugOutput>();
                if (m_DebugOutput == null)
                {
                    Debug.LogError("DebugOutput component not found on debugOutputGO");
                }
            }

            // TODO Change to FindObjectOfType
            stock = GameObject.Find("PlayPlane/PlayPlaneOffset/Stock");
            if (stock == null)
                DebugOutput.Instance.LogError("stock not found");

            waste = GameObject.Find("PlayPlane/PlayPlaneOffset/Waste");
            if (waste == null)
                DebugOutput.Instance.LogError("waste not found");

            hand = GameObject.Find("PlayPlane/Hand");
            if (hand == null)
                DebugOutput.Instance.LogError("hand not found");

            deckOfCards = GameObject.Find("DeckOfCards");
            if (deckOfCards == null)
                DebugOutput.Instance.LogError("deckOfCards not found");
        }



        public void BuildFoundations()
        {
            // TODO: make them suit agnostic until first Ace is placed
            foundations = new List<GameObject>(4);
            int i = 0;
            Vector3 foundationOrigin = Vector3.zero;
            foreach (int suit in Enum.GetValues(typeof(Suit)))
            {
                string suitName = Enum.GetName(typeof(Suit), suit);
                string goName = "PlayPlane/PlayPlaneOffset/Foundations/" + suitName;
                // TODO Change to FindObjectOfType
                GameObject foundation = GameObject.Find(goName);
                if (foundation == null)
                {
                    DebugOutput.Instance.LogError("foundation not found " + goName);
                    throw new Exception("foundation GameObject not found");
                }
                else
                {
                    if (i == 0)
                    {
                        foundationOrigin = foundation.transform.localPosition;
                    }

                    // make sure the foundations are spaced evenly apart
                    Vector3 newPos = new Vector3(foundationOrigin.x, foundationOrigin.y, foundationOrigin.z);
                    newPos.x = i * (TAB_SPACING * 0.5f);
                    foundation.transform.localPosition = newPos;
                    foundations.Add(foundation);
                }
                i++;
            }

            
            foundationCardPileGroup = new FoundationCardPileGroup();
        }

        public void BuildTableaus()
        {
            tableaus = new List<GameObject>(7);
            Vector3 tabOrigin = Vector3.zero;
            for (int i = 0; i < 7; i++)
            {
                // TODO Change to FindObjectOfType
                GameObject tab = GameObject.Find("PlayPlane/PlayPlaneOffset/Tableau/t" + i.ToString());
                //DebugOutput.Instance.Log("tab? " + (tab is null));
                if (tab != null)
                {
                    if (i == 0)
                    {
                        tabOrigin = new Vector3(
                            tab.transform.localPosition.x,
                            tab.transform.localPosition.y,
                            tab.transform.localPosition.z
                        );
                        //DebugOutput.Instance.LogWarning("tabOrigin " + tabOrigin);
                    }
                    else
                    {
                        // make sure the tableaus are evenly spaced apart
                        Vector3 newPos = new Vector3(tabOrigin.x, tabOrigin.y, tabOrigin.z);
                        newPos.x = TAB_SPACING * i;
                        //DebugOutput.Instance.LogWarning($"tabPos: {i} {newPos}");
                        tab.transform.localPosition = newPos; // tab.transform.InverseTransformPoint(newPos);
                    }

                    tableaus.Add(tab);
                }
            }

            tableauCards = new PlayingCardIDListGroup(7);
            for (int i = 0; i < 7; i++)
            {
                // i think 19 is the max cards you could have in a tableau right?
                // because the 7th tableau can have 6 face down cards + 13 face up
                tableauCards.Add(new PlayingCardIDList(19));
            }
        }

        public string GetDebugText()
        {
            string textBlock = "Debug Output";

            textBlock += $"\n Move count {m_Moves}";

            if (stockCards != null)
                textBlock += $"\n Stock count {stockCards.Count}";

            if (wasteCards != null)
                textBlock += $"\n Waste count {wasteCards.Count}";

            if (handCards != null)
                textBlock += $"\n Hand count {handCards.Count}";

            textBlock += "\n";
            for (int i = 0; i < foundationCards.Count; i++)
            {
                textBlock += $"F:{i}:{foundationCards[i].Count} ";
            }

            textBlock += "\n";
            for (int i = 0; i < tableauCards.Count; i++)
            {
                textBlock += $"T:{i}:{tableauCards[i].Count} ";
            }
            return textBlock;
        }

        public void FlipCardFaceUp(SolitaireCard card)
        {
            GameObject cardGO = card.GetGameObject();
            Transform cardTX = cardGO.transform;
            // retain position, just flip over y axis
            // TODO: use rotate()
            card.SetGoalIdentity(new GoalIdentity(
                cardGO,
                cardTX.localPosition + Vector3.zero,
                cardTX.localRotation * Quaternion.Euler(0.0f, 180.0f, 0.0f),
                cardTX.localScale + Vector3.zero
            ));
            card.SetIsFaceUp(true);
        }

        public void TryPickupCardInSpot(PlayfieldSpot spot, SolitaireCard card)
        {
            SuitRank topCardId;

            DebugOutput.Instance.LogWarning($"try pickup card in spot {spot}");

            // pick up the card
            switch (spot.area)
            {
                case PlayfieldArea.TABLEAU:
                    // try and pick up one or more cards from a tab pile
                    PlayingCardIDList tableauCardList = tableauCards[spot.index];
                    if (!card.IsFaceUp)
                    {
                        bool IsTopCard = IsTopCardInPlayfieldSpot(card, spot);
                        // if the card IS the top card, turn it over
                        DebugOutput.Instance.LogWarning($"is top card? {IsTopCard}");
                        if (IsTopCard)
                        {
                            FlipCardFaceUp(card);
                            return;
                        }
                        else
                        {
                            DebugOutput.Instance.LogWarning("Cannot pick up face down tableau card");
                        }
                        return;
                    }
                    else
                    {
                        // if it IS face up, try to collect it and any additional cards aka substack
                        // into the hand
                        PlayingCardIDList subStack = CollectSubStack(card);
                        PickUpCards(subStack);
                        return;
                    }

                case PlayfieldArea.Stock:
                    // enforce only being able to pick up the topmost card:
                    if (stockCards.Count == 0)
                    {
                        DebugOutput.Instance.LogError("error picking up stock card, stock pile is empty");
                    }
                    topCardId = stockCards.Last();
                    card = deck.GetCardBySuitRank(topCardId);

                    // remove top card from list
                    // this now happens inside set card goal
                    //stockCards.RemoveAt(stockCards.Count - 1);

                    // move it to the waste pile; face up
                    SetCardGoalIDToPlayfieldSpot(card,
                        new PlayfieldSpot(PlayfieldArea.Waste, wasteCards.Count), true);

                    return;

                case PlayfieldArea.Foundation:
                    // enforce only being able to pick up the topmost card:
                    if (foundationCards[spot.index].Count == 0)
                    {
                        DebugOutput.Instance.LogError("error picking up foundation card, foundation pile is empty");
                    }
                    topCardId = foundationCards[spot.index].Last();
                    card = deck.GetCardBySuitRank(topCardId);
                    break;

                case PlayfieldArea.Waste:
                    // enforce only being able to pick up the topmost card:
                    if (wasteCards.Count == 0)
                    {
                        DebugOutput.Instance.LogError("error picking up waste card, waste pile is empty");
                    }
                    topCardId = wasteCards.Last();
                    DebugOutput.Instance.LogWarning($"pickup from waste {topCardId}");
                    card = deck.GetCardBySuitRank(topCardId);
                    break;
            }

            // this method will add the card to the handCards list
            SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Hand, 0), true);
        }

        public void TryPlaceHandCardToSpot(PlayfieldSpot spot)
        {
            if (handCards.Count < 1)
            {
                DebugOutput.Instance.LogWarning("no cards in hand.");
                return;
            }

            // we already have a card in our hand, try placing it...
            SuitRank cardInHandSuitRank = handCards.First();
            SolitaireCard cardInHand = deck.GetCardBySuitRank(cardInHandSuitRank);
            if (CheckHandToPlayfieldMoveIsValid(cardInHand, spot))
            {
                m_Moves++;
                // move it valid, execute it
                bool faceUp = true;

                foreach (SuitRank id in handCards)
                {
                    SolitaireCard card = deck.GetCardBySuitRank(id);
                    // they will be removed from handCards list as they're added to the new destination spot
                    SetCardGoalIDToPlayfieldSpot(card, spot, faceUp);
                }

                CheckFlipOverTopCardInTableauCardJustLeft(cardInHand);
            }
            else
            {
                // move is invalid...
                DebugOutput.Instance.LogWarning("Invalid move, try again.");
            }
        }
        public void CheckFlipOverTopCardInTableauCardJustLeft(SolitaireCard card)
        {
            // refer back to the tableau we just came from and see if we need to auto-flip over a card
            if (card.previousPlayfieldSpot.area == PlayfieldArea.Tableau)
            {
                PlayingCardIDList tCardList = tableauCards[card.previousPlayfieldSpot.index];
                if (tCardList.Count > 0)
                {
                    SuitRank topmost = tCardList.Last();
                    Card topMostCard = deck.GetCardBySuitRank(topmost);
                    if (!topMostCard.IsFaceUp)
                    {
                        FlipCardFaceUp(topMostCard);
                    }
                }
            }
        }
        public void OnSingleClickCard(SolitaireCard card)
        {
            DebugOutput.Instance.LogWarning($"on single click card {card}");
            if (card.playfieldSpot.area == PlayfieldArea.Stock)
            {
                // send to waste
                StockToWaste();
                return;
            }

            // otherwise it's in Waste,Foundation, or Tableau, and we should try to auto-place it

            if (card.playfieldSpot.area == PlayfieldArea.INVALID || card.playfieldSpot.area == PlayfieldArea.DECK)
            {
                DebugOutput.Instance.LogError("invalid or deck card clicked, ignoring");
                return;
            }

            if (card.playfieldSpot.area == PlayfieldArea.HAND)
            {
                DebugOutput.Instance.LogError("single-clicked card in hand.. could try to autoplace, but lets just ignore for now");
                // TODO; if you implement auto-place, make sure you place the 0th card in the hand, and let any other cards be placed down on top of the ideal spot
                return;
            }


            // auto-place
            PlayfieldSpot next_spot = GetNextValidPlayfieldSpotForSuitRank(card.GetSuitRank());
            DebugOutput.Instance.LogWarning($"double-click {card.GetSuitRank()} -> {next_spot}");
            if (next_spot.area == PlayfieldArea.INVALID)
            {
                DebugOutput.Instance.LogWarning("no valid spot found for card, ignoring");
                return;
            }
            else
            {
                bool isTopCard = IsTopCardInPlayfieldSpot(card, next_spot);

                if (card.IsFaceUp)
                {
                    PlayingCardIDList subStackCards = CollectSubStack(card);
                    foreach (SuitRank suitRank in subStackCards)
                    {
                        SolitaireCard hand_card = deck.GetCardBySuitRank(suitRank);
                        SetCardGoalIDToPlayfieldSpot(hand_card, next_spot, true);/* true = faceUp */
                    }
                    CheckFlipOverTopCardInTableauCardJustLeft(card);
                }
                else
                {
                    DebugOutput.Instance.LogWarning("cannot move a face down card in a tableau, we can only flip it over, and it should've already flipped over");
                    if (isTopCard)
                    {
                        FlipCardFaceUp(card);
                    }
                    else
                    {
                        DebugOutput.Instance.LogWarning("oh, it wasn't even the top card, yeah no, you can't act on this card. ignoring...");
                        return;
                    }
                }
            }
        }
        public void OnLongPressCard(SolitaireCard card)
        {
            DebugOutput.Instance.Log("OnLongPressCard handCards Count " + card + " | handCardsCount:" + handCards.Count);
            if (handCards.Count < 1)
            {
                TryPickupCardInSpot(card.playfieldSpot, card);
            }
            else
            {
                TryPlaceHandCardToSpot(card.playfieldSpot);
            }
        }
        public void OnSingleClickEmptyStack(PlayfieldSpot destinationSpot)
        {
            DebugOutput.Instance.LogWarning($"OnSingleClickEmptyStack {destinationSpot}");
            switch (destinationSpot.area)
            {
                case PlayfieldArea.TABLEAU:
                    break;
                case PlayfieldArea.WASTE:
                    break;
                case PlayfieldArea.FOUNDATION:
                    break;
                case PlayfieldArea.STOCK:
                    if (stockCards.Count == 0)
                    {
                        DebugOutput.Instance.LogWarning("resetting waste to stock");
                        // move waste to stock
                        WasteToStock();
                        return;
                    }
                    DebugOutput.Instance.LogWarning("ignoring click on stock, cards still in stock");
                    break;
            }
            TryPlaceHandCardToSpot(destinationSpot);
        }

        public void StockToWaste()
        {
            // if no cards left in stock, return
            if (stockCards.Count < 1)
            {
                DebugOutput.Instance.LogWarning("StockToWaste: No cards in Stock pile");
                //call WasteToStock(); here???
                return;
            }
            // take top card (0th) from stockCards list, remove it, and append it to the Waste pile
            // TODO: support 3 at a time mode
            SuitRank cardSuitRank = stockCards[0];

            // this now happens inside SetCardGoal
            //stockCards.RemoveAt(0);

            SolitaireCard card = deck.GetCardBySuitRank(cardSuitRank);
            SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Waste, wasteCards.Count), true);
        }

        public void WasteToStock()
        {
            // if no cards left in waste, return
            if (wasteCards.Count < 1)
            {
                DebugOutput.Instance.LogWarning("WasteToStock: No cards in Waste pile");
                return;
            }
            // return all wasteCards to the stockCards list (in reverse order)
            int order_i = 0;
            for (int i = wasteCards.Count - 1; i > -1; i--)
            {
                PlayfieldSpot stockSpot = new PlayfieldSpot(PlayfieldArea.STOCK, order_i);

                SolitaireCard card = deck.GetCardBySuitRank(wasteCards[i]);
                SetCardGoalIDToPlayfieldSpot(card, stockSpot, false);
                order_i++;
            }
            // reset to empty list
            wasteCards = new PlayingCardIDList();
        }

        public bool IsTopCardInPlayfieldSpot(SolitaireCard card, PlayfieldSpot next_spot)
        {
            switch (next_spot.area)
            {
                case PlayfieldArea.TABLEAU:
                    if (tableauCards[next_spot.index].Count > 0)
                    {
                        SuitRank topCardId = tableauCards[next_spot.index].Last();
                        SolitaireCard topCard = deck.GetCardBySuitRank(topCardId);
                        if (topCard == card)
                            return true;
                    }
                    break;

                case PlayfieldArea.FOUNDATION:
                    if (foundationCards[next_spot.index].Count > 0)
                    {
                        SuitRank topCardId = foundationCards[next_spot.index].Last();
                        SolitaireCard topCard = deck.GetCardBySuitRank(topCardId);
                        if (topCard == card)
                            return true;
                    }
                    break;

                case PlayfieldArea.WASTE:
                    if (wasteCards.Count > 0)
                    {
                        SuitRank topCardId = wasteCards.Last();
                        SolitaireCard topCard = deck.GetCardBySuitRank(topCardId);
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
                DebugOutput.Instance.LogWarning("Ignoring double click on face-down card");
                return;
            }

        }
    }
}
