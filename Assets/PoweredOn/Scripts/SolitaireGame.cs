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

        List<SuitRank> dealtOrder;
        List<SuitRank> deckOrder;
        List<List<SuitRank>> tableauCards;
        List<SuitRank> wasteCards;
        List<List<SuitRank>> foundationCards;
        List<SuitRank> stockCards;
        public List<SuitRank> handCards;
        List<SuitRank> deckCards;

        bool autoPlaceEnabled = true;

        // card, from, to
        List<Tuple<Card, PlayfieldSpot, PlayfieldSpot>> moveLog;

        public DebugOutput m_DebugOutput;

        int m_Moves = 0;
        public SolitaireGame()
        {
            deckOfCards = GameObject.Find("DeckOfCards");
            m_DebugOutput = GameObject.Find("DebugOutput").GetComponent<DebugOutput>();
        }

        /*
         * Reinitialize all of our tracking variables
         * 
         */
        public void Reset()
        {
            m_Moves = 0;
            moveLog = new List<Tuple<Card, PlayfieldSpot, PlayfieldSpot>>();
            stockCards = new List<SuitRank>();
            wasteCards = new List<SuitRank>();
            foundationCards = new List<List<SuitRank>>();
            tableauCards = new List<List<SuitRank>>();
            handCards = new List<SuitRank>();
            dealtOrder = new List<SuitRank>();
            deckCards = GetDeckDefaultCardOrderList();
        }

        public void NewGame()
        {
            UpdateGameObjectReferences();
            Reset();
            BuildFoundations();
            BuildTableaus();
            BuildDeck();
        }        

        public void ToggleAutoPlace()
        {
            autoPlaceEnabled = !autoPlaceEnabled;
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

                    dealtOrder.Add(suitRankToDeal);

                    // NOTE: inside this method, we handle adding SuitRank to the proper Tableau list
                    SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Tableau, pile), faceUp, 0.1f * dealtOrder.Count);

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
        public PlayfieldSpot GetNextValidPlayfieldSpotForSuitRank(SuitRank suitrank)
        {

            // check the top card of the foundation first
            m_DebugOutput.LogWarning("[GetNextValidPlayfieldSpotForSuitRank] Checking foundation card list for rank: " + suitrank.rank + " suit int: " + (int)suitrank.suit + " " + foundationCards.Count);
            List<SuitRank> foundationList = foundationCards[(int)suitrank.suit];
            SuitRank topCardSR;
            m_DebugOutput.LogWarning("[GetNextValidPlayfieldSpotForSuitRank] Foundation List: " + foundationList.Count);
            if (foundationList.Count == 0)
            {
                // if the foundation list is empty, that's our first choice spot
                // if the rank is Rank.ace (0), return the foundation pile for the suit
                if (suitrank.rank == Rank.ace) { return new PlayfieldSpot(PlayfieldArea.Foundation, (int)suitrank.rank, 0); }
            }
            else
            {
                topCardSR = foundationList.Last();
                m_DebugOutput.LogWarning($"[GetNextValidPlayfieldSpotForSuitRank] checking if top card in foundation list is one rank less than the card we are trying to place {(int)topCardSR.rank} {(int)suitrank.rank}");
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

            return PlayfieldSpot.Invalid;
        }

        public bool CheckMoveIsValid(Card handCard, PlayfieldSpot destinationSpot)
        {
            // first validate the move is valid
            m_DebugOutput.LogWarning("CheckMoveIsValid " + handCard.GetGameObjectName() + " to " + destinationSpot.ToString());

            // placing it back down
            if (
                handCard.previousPlayfieldSpot.area != PlayfieldArea.Invalid)
            {
                if (handCard.previousPlayfieldSpot.area == destinationSpot.area)
                {
                    m_DebugOutput.LogWarning($"valid to place card back down where it just came from {handCard.previousPlayfieldSpot}");
                    return true;
                }
            }

            switch (destinationSpot.area)
            {
                case PlayfieldArea.Hand:
                    if (handCards.Count > 0)
                    {
                        m_DebugOutput.LogWarning("already have a card in your hand, cant hold more than one unless you pick up a substack on a tableau");
                        return false;
                    }
                    return true;

                case PlayfieldArea.Stock:
                    m_DebugOutput.LogWarning("invalid dest: Stock. cannot move card back to stock");
                    return false;

                case PlayfieldArea.Waste:
                    m_DebugOutput.LogWarning("todo: if card came from top of waste, we should've already allowed putting it back");
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

        public void SetCardGoalIDToPlayfieldSpot(Card card, PlayfieldSpot spot, bool faceUp, float delay = 0.0f)
        {
            if(card.playfieldSpot.area != PlayfieldArea.Invalid)
            {
                card.SetPreviousPlayfieldSpot(card.playfieldSpot.Clone());
                try
                {
                    RemoveCardFromCurrentPlayfieldSpot(card);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    m_DebugOutput.LogError(e.ToString());
                }
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
                    gPos.z = (-0.05f) + (stockCards.Count * -CARD_THICKNESS);
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
            Quaternion[] options = new Quaternion[2] { Quaternion.Euler(0, 0, 90), CARD_DEFAULT_ROTATION };
            goalID.rotation = (spot.area == PlayfieldArea.Hand) ? (faceUp ? options[0] : options[1]) : (faceUp ? options[1] : options[0]);
            goalID.SetDelay(delay);
            card.SetGoalIdentity(goalID);
            card.SetPlayfieldSpot(spot);
            card.SetIsFaceUp(faceUp);
        }

        public void RemoveCardFromCurrentPlayfieldSpot(Card card)
        {
            string dbugstring = $"{card.GetGameObjectName()} {card.playfieldSpot}";

#nullable enable
            List < SuitRank >? pile = null;
            if(card.playfieldSpot.area == PlayfieldArea.Invalid)
            {
                m_DebugOutput.Log("card has no playfield spot. skipping list removal attempt");
                return;
            }


            PlayfieldSpot pfspot = card.playfieldSpot;
            switch (pfspot.area)
            {
                case PlayfieldArea.Deck:
                    pile = deckCards;
                    return;
                case PlayfieldArea.Hand:
                    pile = handCards;
                    break;
                case PlayfieldArea.Waste:
                    pile = wasteCards;
                    break;
                case PlayfieldArea.Foundation:
                    pile = foundationCards[pfspot.index];
                    break;
                case PlayfieldArea.Tableau:
                    pile = tableauCards[pfspot.index];
                    break;
            }

            if(pile == null)
            {
                m_DebugOutput.LogWarning($"no matching pile found for playfield spot {pfspot}");
                return;
            }

            if (pile.Count > 0)
            {
                string pilename = Enum.GetName(typeof(PlayfieldArea), pfspot.area);
                int index = pile.IndexOf(card.GetSuitRank());

                int topIndex = pile.Count - 1;
                if(pfspot.area == PlayfieldArea.Stock || pfspot.area == PlayfieldArea.Waste)
                {
                    if(index != topIndex)
                    {
                        // note, this isn't ALWAYS an error
                        // example, when passing waste cards back to stock
                        m_DebugOutput.LogError($"error: trying to remove NON-TOP card from Stock or Waste {pfspot.subindex} got {index}");
                    }
                }

                if(pfspot.subindex != index)
                {
                    m_DebugOutput.LogError($"card subindex does not match expectation found: {pfspot.subindex} got {index}");
                }
                if (index > -1)
                {
                    m_DebugOutput.LogWarning($"removing card from {pilename} {dbugstring}");
                    pile.RemoveAt(index);
                }
                else
                {
                    m_DebugOutput.LogWarning($"unable to find card in {pilename} {dbugstring}");
                }
            }
        }

        public void PickUpCards(List<SuitRank> cards)
        {
            m_DebugOutput.LogWarning("picking up cards: " + cards.Count);
            foreach(SuitRank id in cards)
            {
                m_DebugOutput.LogWarning(id.ToString());
            }
            // empty list (will be populated by calls to SetCardGoalID...)
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

        public List<SuitRank> CollectSubStack(Card card)
        {
            if(card.playfieldSpot.area == PlayfieldArea.Invalid)
            {
                throw new Exception("got card with Invalid playfield spot");
            }
            else
            {
                PlayfieldSpot spot = card.playfieldSpot;
                List<SuitRank> cardGroup = new List<SuitRank>(1) { card.GetSuitRank() };
                List<SuitRank> tableauCardList = tableauCards[spot.index];

                int cardIndex = tableauCardList.IndexOf(card.GetSuitRank());
                if(cardIndex == tableauCardList.Count - 1)
                {
                    return cardGroup; // single card
                }
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

            deckOfCards = GameObject.Find("DeckOfCards");
            if (deckOfCards == null)
                m_DebugOutput.LogError("deckOfCards not found");
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

        public void FlipCardFaceUp(Card card)
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
        }
        
        public void TryPickupCardInSpot(PlayfieldSpot spot, Card card)
        {
            SuitRank topCardId;

            m_DebugOutput.LogWarning($"try pickup card in spot {spot}");

            // pick up the card
            switch (spot.area)
            {
                case PlayfieldArea.Tableau:
                    // try and pick up one or more cards from a tab pile
                    List<SuitRank> tableauCardList = tableauCards[spot.index];
                    if (!card.IsFaceUp)
                    {
                        bool IsTopCard = IsTopCardInPlayfieldSpot(card, spot);
                        // if the card IS the top card, turn it over
                        m_DebugOutput.LogWarning($"is top card? {IsTopCard}");
                        if (IsTopCard)
                        {
                            FlipCardFaceUp(card);
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
                        // if it IS face up, try to collect it and any additional cards aka substack
                        // into the hand
                        List<SuitRank> subStack = CollectSubStack(card);
                        PickUpCards(subStack);
                        return;
                    }

                case PlayfieldArea.Stock:
                    // enforce only being able to pick up the topmost card:
                    if (stockCards.Count == 0)
                    {
                        m_DebugOutput.LogError("error picking up stock card, stock pile is empty");
                    }
                    topCardId = stockCards.Last();
                    card = deck.GetCardBySuitRank(topCardId);

                    // remove top card from list
                    stockCards.RemoveAt(stockCards.Count - 1);

                    // move it to the waste pile; face up
                    SetCardGoalIDToPlayfieldSpot(card,
                        new PlayfieldSpot(PlayfieldArea.Waste, 0), true);

                    return;

                case PlayfieldArea.Foundation:
                    // enforce only being able to pick up the topmost card:
                    if (foundationCards[spot.index].Count == 0)
                    {
                        m_DebugOutput.LogError("error picking up foundation card, foundation pile is empty");
                    }
                    topCardId = foundationCards[spot.index].Last();
                    card = deck.GetCardBySuitRank(topCardId);
                    break;

                case PlayfieldArea.Waste:
                    // enforce only being able to pick up the topmost card:
                    if(wasteCards.Count == 0)
                    {
                        m_DebugOutput.LogError("error picking up waste card, waste pile is empty");
                    }
                    topCardId = wasteCards.Last();
                    m_DebugOutput.LogWarning($"pickup from waste {topCardId}");
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
                
                foreach(SuitRank id in handCards)
                {
                    Card card = deck.GetCardBySuitRank(id);
                    // they will be removed from handCards list as they're added to the new destination spot
                    SetCardGoalIDToPlayfieldSpot(card, spot, faceUp);
                }

                CheckFlipOverTopCardInTableauCardJustLeft(cardInHand);
            }
            else
            {
                // move is invalid...
                m_DebugOutput.LogWarning("Invalid move, try again.");
            }
        }
        public void CheckFlipOverTopCardInTableauCardJustLeft(Card card)
        {
            // refer back to the tableau we just came from and see if we need to auto-flip over a card
            if (card.previousPlayfieldSpot.area == PlayfieldArea.Tableau)
            {
                List<SuitRank> tCardList = tableauCards[card.previousPlayfieldSpot.index];
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
        public void OnSingleClickCard(Card card)
        {
            if (card.playfieldSpot.area == PlayfieldArea.Stock)
            {
                // send to waste
                StockToWaste();
                return;
            }

            // otherwise it's in Waste,Foundation, or Tableau, and we should try to auto-place it

            if (card.playfieldSpot.area == PlayfieldArea.Invalid || card.playfieldSpot.area == PlayfieldArea.Deck)
            {
                m_DebugOutput.LogError("invalid or deck card clicked, ignoring");
                return;
            }

            if (card.playfieldSpot.area == PlayfieldArea.Hand)
            {
                m_DebugOutput.LogError("single-clicked card in hand.. could try to autoplace, but lets just ignore for now");
                // TODO; if you implement auto-place, make sure you place the 0th card in the hand, and let any other cards be placed down on top of the ideal spot
                return;
            }


            // auto-place
            Game.PlayfieldSpot next_spot = GetNextValidPlayfieldSpotForSuitRank(card.GetSuitRank());
            m_DebugOutput.LogWarning($"double-click {card.GetSuitRank()} -> {next_spot}");
            if (next_spot.area == Game.PlayfieldArea.Invalid)
            {
                m_DebugOutput.LogWarning("no valid spot found for card, ignoring");
                return;
            }
            else
            {
                bool isTopCard = IsTopCardInPlayfieldSpot(card, next_spot);

                if (card.IsFaceUp)
                {
                    List<SuitRank> subStackCards = CollectSubStack(card);
                    foreach (SuitRank suitRank in subStackCards)
                    {
                        Card hand_card = deck.GetCardBySuitRank(suitRank);
                        SetCardGoalIDToPlayfieldSpot(hand_card, next_spot, true);/* true = faceUp */
                    }
                    CheckFlipOverTopCardInTableauCardJustLeft(card);
                }
                else
                {
                    m_DebugOutput.LogWarning("cannot move a face down card in a tableau, we can only flip it over, and it should've already flipped over");
                    if (isTopCard)
                    {
                        FlipCardFaceUp(card);
                    }else
                    {
                        m_DebugOutput.LogWarning("oh, it wasn't even the top card, yeah no, you can't act on this card. ignoring...");
                        return;
                    }
                }
            }
        }
        public void OnLongPressCard(Card card)
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
            m_DebugOutput.LogWarning($"OnSingleClickEmptyStack {destinationSpot}");
            switch (destinationSpot.area)
            {
                case PlayfieldArea.Tableau:
                    break;
                case PlayfieldArea.Waste:
                    break;
                case PlayfieldArea.Foundation:
                    break;
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

        public void StockToWaste()
        {
            // if no cards left in stock, return
            if (stockCards.Count < 1)
            {
                m_DebugOutput.LogWarning("StockToWaste: No cards in Stock pile");
                //call WasteToStock(); here???
                return;
            }
            // take top card (0th) from stockCards list, remove it, and append it to the Waste pile
            // TODO: support 3 at a time mode
            SuitRank cardSuitRank = stockCards[0];
            stockCards.RemoveAt(0);

            Card card = deck.GetCardBySuitRank(cardSuitRank);
            SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Waste, wasteCards.Count), true);
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

        internal void OnDoubleClickCard(Card card)
        {
            if (!card.IsFaceUp)
            {
                m_DebugOutput.LogWarning("Ignoring double click on face-down card");
                return;
            }
            
        }
    }
    
}
