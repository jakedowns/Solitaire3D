using PoweredOn.Animations;
using PoweredOn.Objects;
using static PoweredOn.Game;
using static PoweredOn.Managers.DeckManager;
using static PoweredOn.PlayingCards;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine;

namespace PoweredOn
{
    public class SolitaireGame: Game
    {
        public const float TAB_SPACING = 0.1f;
        const float RANDOM_AMPLITUDE = 1f;
        public const float CARD_THICKNESS = 0.0002f;
        public const float TAB_VERT_OFFSET = 0.01f;

        // z-axis rotation is temporary until i fix the orientation of the mesh in blender
        public Quaternion CARD_DEFAULT_ROTATION = Quaternion.Euler(0, 180, 90);

        public Deck deck;
        
        List<GameObject> foundations;
        List<GameObject> tableaus;
        GameObject stock;
        GameObject waste;
        GameObject hand;
        GameObject deckOfCards;

        List<SuitRank> deckOrder;
        List<List<SuitRank>> tableauCards;
        List<SuitRank> wasteCards;
        List<List<SuitRank>> foundationCards;
        List<SuitRank> stockCards;
        public List<SuitRank> handCards;
        List<SuitRank> deckCards;

        // card, from, to
        List<Tuple<Card, PlayfieldSpot, PlayfieldSpot>> moveLog;

        public DebugOutput m_DebugOutput;

        int m_Moves = 0;
        public SolitaireGame()
        {
            deckOfCards = GameObject.Find("DeckOfCards");
            m_DebugOutput = GameObject.Find("DebugOutput").GetComponent<DebugOutput>();
        }

        public void Reset()
        {
            m_Moves = 0;
            stockCards = new List<SuitRank>();
            wasteCards = new List<SuitRank>();
            handCards = new List<SuitRank>();
            foundationCards = new List<List<SuitRank>>();
        }

        public void NewGame()
        {
            BuildFoundations();
            BuildTableaus();
            UpdateGameObjectReferences();
            BuildDeck();
        }        

        public void Deal()
        {
            // first, we want to collect all the cards into a stack
            SetCardGoalsToDeckPositions();

            //await Task.Delay(1000);

            // let's shuffle the card order 3 times (todo: artifically delay and animate this)
            deck.Shuffle(3);

            //await Task.Delay(1000);

            // empty lists
            handCards = new List<SuitRank>();
            deckCards = new List<SuitRank>();

            // then, we need to update our tracking lists so that all cards are in the "stock" list
            tableauCards = new List<List<SuitRank>>(7);
            for (int i = 0; i < 7; i++)
            {
                // i think 19 is the max cards you could have in a tableau right?
                // because the 7th tableau can have 6 face down cards + 13 face up
                tableauCards.Add(new List<SuitRank>(19));
            }

            wasteCards = new List<SuitRank>(52);
            foundationCards = new List<List<SuitRank>>(4);
            for (int i = 0; i < 4; i++)
            {
                foundationCards.Add(new List<SuitRank>(13));
            }

            stockCards = new List<SuitRank>(52);
            for (int i = 0; i < 52; i++)
            {
                // capture all SuitRanks in the "stockCards" pile to begin with
                try
                {
                    stockCards.Add(deck.cards[i].GetSuitRank());
                }
                catch (Exception e)
                {
                    m_DebugOutput.LogError(e.ToString());
                }
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
                    stockCards.RemoveAt(0);

                    Card card = deck.GetCardBySuitRank(suitRankToDeal);

                    bool faceUp = pile == round;

                    // NOTE: inside this method, we handle adding SuitRank to the proper Tableau list
                    SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Tableau, pile), faceUp);

                    //m_DebugOutput.Log($"Dealing {card.GetGameObjectName()} {round} {pile} {faceUp}");
                }
            }

            // then, for the remaining cards, update their GoalIdentity to place them where the "Stock" Pile should go
            // remember to offset the local Z position a bit to make the cards appear as tho they're stacked on top of each other.
            // the last card should be the lowest z-position (same z as the stock pile guide object) and then each card up to the 0th should be the highest
            // so we should loop backwards through the remaining Stock when setting the positions
            for (int i = stockCards.Count - 1; i >= 0; i--)
            {
                SuitRank cardSuitRank = stockCards[i];
                Card card = deck.GetCardBySuitRank(cardSuitRank);
                // NOTE: inside this method we handle adding SuitRank to the stockCards list
                SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Stock, i), false); /* always face down when adding to stock */
            }
        }

        // SetCardGoalsToDeckPositions is called when we want to collect all the cards into a stack
        // This DeckManager class lives on the parent GameObject, and has all the card GameObjects as children
        // we want to use the DeckManager's transform as the local zero position for the center of bottommost card
        // each card should be given a goal position, offset by a positive .0002 m on the local y for each card "below" it in the deck,
        // so the topmost card would have a y position of .0002 (card thickness) * 51 (51 cards below it)
        // the deckOrder property (GetDeckOrder) of each Card is what determines this offset, and the 0th card is the bottom,
        // with the 51st deck position being the top
        public void SetCardGoalsToDeckPositions()
        {
            // get the deck world position
            Vector3 deckPosition = deckOfCards.transform.position + Vector3.zero;

            // loop through our cards, and give them a new GoalIdentity based on our calculations
            foreach (Card card in deck.cards)
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
                    cardTransform.localScale);

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
            foreach (Card card in deck.cards)
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
        public PlayfieldSpot? GetNextValidPlayfieldSpotForSuitRank(SuitRank suitrank)
        {

            // check the top card of the foundation first
            m_DebugOutput.LogWarning("Checking foundation card list for rank: " + suitrank.rank + " suit int: " + (int)suitrank.suit + " " + foundationCards.Count);
            List<SuitRank> foundationList = foundationCards[(int)suitrank.suit];
            SuitRank topCardSR;
            m_DebugOutput.LogWarning("Foundation List: " + foundationList.Count);
            if (foundationList.Count == 0)
            {
                // if the foundation list is empty, that's our first choice spot
                // if the rank is Rank.ace (0), return the foundation pile for the suit
                if (suitrank.rank == Rank.ace) { return new PlayfieldSpot(PlayfieldArea.Foundation, (int)suitrank.rank, 0); }
            }
            else
            {
                topCardSR = foundationList.Last();
                m_DebugOutput.LogWarning($"checking if top card in foundation list is one rank less than the card we are trying to place {(int)topCardSR.rank} {(int)suitrank.rank}");
                if ((int)topCardSR.rank + 1 == (int)suitrank.rank)
                {
                    // if the top card in the foundation list is one less than this card's rank, that's our first choice spot
                    return new PlayfieldSpot(PlayfieldArea.Foundation, (int)suitrank.suit, foundationList.Count);
                }
            }

            // next, loop through the tableau piles from left to right and see if there's a valid spot for this card to go to
            for (int i = 0; i < 7; i++)
            {
                if (tableauCards[i].Count == 0)
                {
                    // if the rank is Rank.king (12), see if we have an open tableau spot and put it there
                    if (suitrank.rank == Rank.king)
                    {
                        // empty tableau found for you, my king!
                        return new PlayfieldSpot(PlayfieldArea.Tableau, i, 0);
                    }
                }
                else
                {
                    topCardSR = tableauCards[i].Last();
                    if (
                       (int)topCardSR.rank - 1 == (int)suitrank.rank
                       && Deck.SuitsAreOpposite(topCardSR.suit, suitrank.suit)
                    )
                    {
                        return new PlayfieldSpot(PlayfieldArea.Tableau, i, tableauCards[i].Count);
                    }
                }
            }

            m_DebugOutput.LogWarning($"no suggested playfield spot found for {suitrank.ToString()}");

            return null;
        }

        public bool CheckMoveIsValid(Card handCard, PlayfieldSpot destinationSpot)
        {
            // first validate the move is valid
            m_DebugOutput.LogWarning("CheckMoveIsValid " + handCard.GetGameObjectName() + " to " + destinationSpot.ToString());

            // placing it back down
            if (
                handCard.previousPlayfieldSpot != null)
            {
                PlayfieldSpot ppfs = (PlayfieldSpot)handCard.previousPlayfieldSpot;
                if (ppfs.area == destinationSpot.area)
                {
                    return true;
                }
            }

            switch (destinationSpot.area)
            {
                case PlayfieldArea.Hand:
                    if (handCards.Count > 0)
                    {
                        m_DebugOutput.LogWarning("already have a card in your hand");
                        return false;
                    }
                    return true;
                case PlayfieldArea.Stock:
                    m_DebugOutput.LogWarning("invalid dest: Stock. cannot move card back to stock");
                    return false;
                case PlayfieldArea.Waste:
                    m_DebugOutput.LogWarning("todo: if card came from top of waste, allow putting it back");
                    break;
                case PlayfieldArea.Tableau:
                    List<SuitRank> tabCardList = tableauCards[destinationSpot.index];
                    m_DebugOutput.LogWarning("tabCardList Count" + tabCardList.Count);
                    if (tabCardList.Count == 0)
                    {
                        // tableau is empty, kings only
                        if (handCard.GetRank() == Rank.king)
                        {
                            // king is valid
                            return true;
                        }
                        m_DebugOutput.LogWarning("invalid dest: cannot place non-king cards in empty tab spots");
                    }
                    else
                    {
                        // validate the top card in the tableau is one rank higher, and opposite suit
                        SuitRank topCardInTabSuitRank = tabCardList.Last();
                        Card topCardInTab = deck.GetCardBySuitRank(topCardInTabSuitRank);
                        bool suitsAreOpposite = Deck.SuitsAreOpposite(
                                handCard.GetSuit(),
                                topCardInTab.GetSuit()
                                );
                        m_DebugOutput.LogWarning($"comparing ranks:\n" +
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
                        m_DebugOutput.LogWarning($"invalid dest: cannot place {handCard.GetGameObjectName()} on {topCardInTab.GetGameObjectName()} ");
                    }
                    break;
                case PlayfieldArea.Foundation:
                    m_DebugOutput.LogWarning($"trying to place {handCard.GetGameObjectName()} on {destinationSpot}");
                    List<SuitRank> fCardList = foundationCards[destinationSpot.index];
                    if (fCardList.Count < 1)
                    {
                        // empty, only aces are valid
                        if ((int)handCard.GetSuit() == destinationSpot.index && handCard.GetRank() == Rank.ace)
                        {
                            return true;
                        }
                        m_DebugOutput.LogWarning($"invalid dest: cannot place {handCard.GetGameObjectName()} in empty foundation @ {destinationSpot.index}. aces only.");
                    }
                    else
                    {
                        // validate the top card in the foundation is one rank lower than handCard and the SAME suit
                        SuitRank topCardInFoundationSuitRank = fCardList.Last();
                        Card topCardInFoundation = deck.GetCardBySuitRank(topCardInFoundationSuitRank);
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

        public void SetCardGoalIDToPlayfieldSpot(Card card, PlayfieldSpot spot, bool faceUp)
        {
            card.SetPreviousPlayfieldSpot(card.playfieldSpot);
            try
            {
                RemoveCardFromCurrentPlayfieldSpot(card);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                m_DebugOutput.LogError(e.ToString());
            }


            GoalIdentity goalID = new(card.GetGameObject(), Vector3.zero, Quaternion.identity, Vector3.one);
            Transform cardTX = card.GetGameObject().transform;
            Vector3 gPos = Vector3.zero;

            switch (spot.area)
            {
                case PlayfieldArea.Stock:
                    faceUp = false; // always face down when adding to stock
                    stockCards.Add(card.GetSuitRank());
                    gPos = stock.transform.position;
                    gPos.z += stockCards.Count * -CARD_THICKNESS;
                    goalID.SetGoalPositionFromWorldPosition(gPos);
                    break;
                case PlayfieldArea.Waste:
                    wasteCards.Add(card.GetSuitRank());
                    gPos = waste.transform.position;
                    goalID.SetGoalPositionFromWorldPosition(gPos);
                    goalID.position = new Vector3(
                        goalID.position.x,
                        goalID.position.y,
                        goalID.position.z + wasteCards.Count * -(CARD_THICKNESS * 5)
                    );
                    break;
                case PlayfieldArea.Foundation:
                    foundationCards[spot.index].Add(card.GetSuitRank());
                    GameObject foundation = foundations[spot.index];
                    gPos = foundation.transform.position;
                    gPos.z = foundationCards[spot.index].Count * CARD_THICKNESS;
                    goalID.SetGoalPositionFromWorldPosition(gPos);
                    break;
                case PlayfieldArea.Tableau:
                    tableauCards[spot.index].Add(card.GetSuitRank());
                    GameObject tableau = tableaus[spot.index];
                    gPos = tableau.transform.position;
                    gPos.z = (tableauCards[spot.index].Count) * -CARD_THICKNESS;
                    gPos.y = (tableauCards[spot.index].Count) * -TAB_VERT_OFFSET + .01f;
                    //goalID.SetGoalPositionFromWorldPosition(gPos);
                    goalID.position = gPos;// cardTX.InverseTransformPoint(gPos);
                    break;
                case PlayfieldArea.Hand:
                    faceUp = true; // always face up when adding to hand
                    handCards.Add(card.GetSuitRank());
                    m_DebugOutput.Log("added card to hand " + handCards.Count);
                    goalID = new GoalIdentity(card.GetGameObject(), hand);
                    break;
                case PlayfieldArea.Deck:
                    deckCards.Add(card.GetSuitRank());
                    goalID = new GoalIdentity(card.GetGameObject(), deckOfCards);
                    break;
            }

            // z-axis rotation is temporary until i fix the orientation of the mesh in blender
            goalID.rotation = faceUp ? CARD_DEFAULT_ROTATION : Quaternion.Euler(0, 0, 90);

            card.SetGoalIdentity(goalID);
            card.SetPlayfieldSpot(spot);
            card.SetIsFaceUp(faceUp);
        }

        public void RemoveCardFromCurrentPlayfieldSpot(Card card)
        {
            switch (card.playfieldSpot.area)
            {
                case PlayfieldArea.Hand:
                    // clear handcard list
                    handCards.RemoveAt(card.playfieldSpot.subindex);
                    break;
                case PlayfieldArea.Waste:
                    //wasteCards.RemoveAt(card.playfieldSpot.index);
                    wasteCards.RemoveAt(wasteCards.Count - 1);
                    break;
                case PlayfieldArea.Foundation:
                    foundationCards[card.playfieldSpot.index].RemoveAt(foundationCards[card.playfieldSpot.index].Count - 1);
                    break;
                case PlayfieldArea.Tableau:
                    tableauCards[card.playfieldSpot.index].RemoveAt(tableauCards[card.playfieldSpot.index].Count - 1);
                    break;
            }
        }

        public void PickUpCards(List<SuitRank> cards)
        {
            m_DebugOutput.LogWarning("picking up cards: " + cards.Count);
            foreach(SuitRank id in cards)
            {
                m_DebugOutput.LogWarning(id.ToString());
            }
            // empty list (will be populated by calls to SetCardGoalID...
            handCards = new List<SuitRank>(0);
            int i = 0;
            // set the goal identity of each card to the hand
            foreach (SuitRank id in cards)
            {
                PlayfieldSpot handSpot = new PlayfieldSpot(PlayfieldArea.Hand, i);
                Card card = deck.GetCardBySuitRank(id);
                SetCardGoalIDToPlayfieldSpot(card, handSpot, card.IsFaceUp);
                i++;
            }
        }

        public List<SuitRank> CollectCardsAboveFromTab(Card card)
        {
            PlayfieldSpot spot = card.playfieldSpot;
            List<SuitRank> cardGroup = new List<SuitRank>();
            List<SuitRank> tableauCardList = tableauCards[spot.index];

            int cardIndex = tableauCardList.IndexOf(card.GetSuitRank());
            // get the rest of the cards (if any) at a higher index in the list
            for (int i = cardIndex; i < tableauCardList.Count; i++)
            {
                cardGroup.Add(tableauCardList[i]);
            }
            // remove the cards from the tableau
            // note: it'll get removed when the card has it's goal identity updated
            // tableauCardList.RemoveRange(cardIndex, cardGroup.Count);

            // note moveing this out of this function
            // this function is just responsible for getting the list of cards above the current card
            // add the cards to the hand
            //PickUpCards(cardGroup);
            
            m_DebugOutput.Log("picked up cards: ");
            foreach (SuitRank id in cardGroup)
            {
                m_DebugOutput.Log(id.ToString());
            }

            return cardGroup;
        }

        void UpdateGameObjectReferences()
        {
            // TODO Change to FindObjectOfType
            stock = GameObject.Find("PlayPlane/PlayPlaneOffset/Stock");
            if (stock == null)
                m_DebugOutput.LogError("stock not found");
            waste = GameObject.Find("PlayPlane/PlayPlaneOffset/Waste");
            if (waste == null)
                m_DebugOutput.LogError("waste not found");
            hand = GameObject.Find("PlayPlane/Hand");
            if (hand == null)
                m_DebugOutput.LogError("hand not found");
        }

        void BuildDeck()
        {
            deck = new Deck(deckOfCards,m_DebugOutput);
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
                    m_DebugOutput.LogError("foundation not found " + goName);
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
        }

        public void BuildTableaus()
        {
            tableaus = new List<GameObject>(7);
            Vector3 tabOrigin = Vector3.zero;
            for (int i = 0; i < 7; i++)
            {
                // TODO Change to FindObjectOfType
                GameObject tab = GameObject.Find("PlayPlane/PlayPlaneOffset/Tableau/t" + i.ToString());
                //m_DebugOutput.Log("tab? " + (tab is null));
                if (tab != null)
                {
                    if (i == 0)
                    {
                        tabOrigin = new Vector3(
                            tab.transform.localPosition.x,
                            tab.transform.localPosition.y,
                            tab.transform.localPosition.z
                        );
                        m_DebugOutput.LogWarning("tabOrigin " + tabOrigin);
                    }
                    else
                    {
                        // make sure the tableaus are evenly spaced apart
                        Vector3 newPos = new Vector3(tabOrigin.x, tabOrigin.y, tabOrigin.z);
                        newPos.x = TAB_SPACING * i;
                        m_DebugOutput.LogWarning($"tabPos: {i} {newPos}");
                        tab.transform.localPosition = newPos; // tab.transform.InverseTransformPoint(newPos);
                    }

                    tableaus.Add(tab);
                }
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

            for (int i = 0; i < foundationCards.Count; i++)
            {
                textBlock += $"\n Foundation {i} count {foundationCards[i].Count}";
            }
            for (int i = 0; i < tableauCards.Count; i++)
            {
                textBlock += $"\n Tableau {i} count {tableauCards[i].Count}";
            }
            return textBlock;
        }

#nullable enable
        public void TryPickupCardInSpot(PlayfieldSpot spot, Card? card = null)
        {
            SuitRank topCardId;

            m_DebugOutput.LogWarning($"try pickup card in spot {spot}");

            // pick up the card
            switch (spot.area)
            {
                case PlayfieldArea.Tableau:
                    // try and pick up one or more cards from a tab pile
                    if (card != null)
                    {
                        List<SuitRank> tableauCardList = tableauCards[spot.index];
                        if (!card.IsFaceUp)
                        {
                            bool IsTopCard = IsTopCardInPlayfieldSpot(card, spot);
                            // if the card IS the top card, turn it over
                            m_DebugOutput.LogWarning($"is top card? {IsTopCard}");
                            if (IsTopCard)
                            {
                                GameObject cardGO = card.GetGameObject();
                                Transform cardTX = cardGO.transform;
                                // retain position, just flip over y axis
                                // TODO: use rotate()
                                card.SetGoalIdentity(new GoalIdentity(
                                    cardGO,
                                    cardTX.localPosition,
                                    cardTX.localRotation * Quaternion.Euler(0.0f, 180.0f, 0.0f),
                                    cardTX.localScale
                                ));
                                card.SetIsFaceUp(true);
                                return;
                            }
                            else
                            {
                                m_DebugOutput.LogWarning("Cannot pick up face down tableau card");
                            }
                            return;
                        }
                        else
                        {
                            // if it IS face up, try to collect any additional cards
                            List<SuitRank> cardsAboveCard = CollectCardsAboveFromTab(card);
                            cardsAboveCard.Prepend(card.GetSuitRank());
                            PickUpCards(cardsAboveCard);
                        }
                    }
                    else
                    { 
                        m_DebugOutput.LogWarning("TryPickupCardInSpot need card for tableau pickup attempt");
                    }
                    return;

                case PlayfieldArea.Stock:
                    topCardId = stockCards.Last();
                    card = deck.GetCardBySuitRank(topCardId);

                    // remove top card from list
                    stockCards.RemoveAt(stockCards.Count - 1);

                    // move it to the waste pile; face up
                    SetCardGoalIDToPlayfieldSpot(card,
                        new PlayfieldSpot(PlayfieldArea.Waste, 0), true);

                    return;

                case PlayfieldArea.Foundation:
                    topCardId = foundationCards[spot.index].Last();
                    card = deck.GetCardBySuitRank(topCardId);
                    break;

                case PlayfieldArea.Waste:
                    topCardId = wasteCards.Last();
                    m_DebugOutput.LogWarning($"pickup from waste {topCardId}");
                    card = deck.GetCardBySuitRank(topCardId);
                    break;
            }

            if (card == null)
            {
                m_DebugOutput.LogWarning("TryPickupCardInSpot card is null");
                return;
            }

            // this method will add the card to the handCards list
            SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Hand, 0), true);
        }

        public void TryPlaceHandCardToSpot(PlayfieldSpot spot)
        {
            if (handCards.Count < 1)
            {
                m_DebugOutput.LogWarning("no cards in hand.");
                return;
            }

            // we already have a card in our hand, try placing it...
            SuitRank cardInHandSuitRank = handCards.First();
            Card cardInHand = deck.GetCardBySuitRank(cardInHandSuitRank);
            if (CheckMoveIsValid(cardInHand, spot))
            {
                m_Moves++;
                // move it valid, execute it
                bool faceUp = true;
                //SetCardGoalIDToPlayfieldSpot(cardInHand, spot, faceUp);
                foreach(SuitRank id in handCards)
                {
                    Card card = deck.GetCardBySuitRank(id);
                    // they will be removed from handCards as they're added
                    // this might be problematic
                    SetCardGoalIDToPlayfieldSpot(card, spot, faceUp);
                }
            }
            else
            {
                // move is invalid...
                m_DebugOutput.LogWarning("Invalid move, try again.");
            }
        }
        public void OnSingleClickCard(Card card)
        {
            m_DebugOutput.Log("OnSingleClickCard handCards Count " + handCards.Count);
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
            switch (destinationSpot.area)
            {
                case PlayfieldArea.Stock:
                    if(stockCards.Count == 0)
                    {
                        m_DebugOutput.LogWarning("resetting waste to stock");
                        // move waste to stock
                        WasteToStock();
                        return;
                    }
                    m_DebugOutput.LogWarning("ignoring click on stock, cards still in stock");
                    break;
            }
            TryPlaceHandCardToSpot(destinationSpot);
        }

        public void WasteToStock()
        {
            // if no cards left in waste, return
            if (wasteCards.Count < 1)
            {
                m_DebugOutput.LogWarning("WasteToStock: No cards in Waste pile");
                return;
            }
            // return all wasteCards to the stockCards list (in reverse order)
            int order_i = 0;
            for (int i = wasteCards.Count - 1; i > -1; i--)
            {
                PlayfieldSpot stockSpot = new PlayfieldSpot(PlayfieldArea.Stock, order_i);
                stockCards.Add(wasteCards[i]);
                Card card = deck.GetCardBySuitRank(wasteCards[i]);
                SetCardGoalIDToPlayfieldSpot(card, stockSpot, false);
                order_i++;
            }
            // reset to empty list
            wasteCards = new List<SuitRank>();
        }

        public bool IsTopCardInPlayfieldSpot(Card card, PlayfieldSpot next_spot)
        {
            switch (next_spot.area)
            {
                case PlayfieldArea.Tableau:
                    if (tableauCards[next_spot.index].Count > 0)
                    {
                        SuitRank topCardId = tableauCards[next_spot.index].Last();
                        Card topCard = deck.GetCardBySuitRank(topCardId);
                        if (topCard == card)
                            return true;
                    }
                    break;

                case PlayfieldArea.Foundation:
                    if (foundationCards[next_spot.index].Count > 0)
                    {
                        SuitRank topCardId = foundationCards[next_spot.index].Last();
                        Card topCard = deck.GetCardBySuitRank(topCardId);
                        if (topCard == card)
                            return true;
                    }
                    break;

                case PlayfieldArea.Waste:
                    if (wasteCards.Count > 0)
                    {
                        SuitRank topCardId = wasteCards.Last();
                        Card topCard = deck.GetCardBySuitRank(topCardId);
                        if (topCard == card)
                            return true;
                    }
                    break;
            }
            return false;
        }
    }
    
}
