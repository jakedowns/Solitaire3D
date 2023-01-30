using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using PoweredOn.Objects;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using PoweredOn.Animations;

namespace PoweredOn.Managers
{
    public class DeckManager : MonoBehaviour
    {

        CardInteractive cardInteractive;

        List<Card> cards;

        List<SuitRank> deckOrder;
        List<List<SuitRank>> tableauCards;
        List<SuitRank> wasteCards;
        List<List<SuitRank>> foundationCards;
        List<SuitRank> stockCards;
        List<SuitRank> handCards;
        List<SuitRank> deckCards;

        List<GameObject> foundations;
        List<GameObject> tableaus;
        GameObject stock;
        GameObject waste;
        GameObject hand;

        List<List<int>> log;
        Dictionary<SuitRank, int> cardIndexLookup;

#nullable enable
        IEnumerator? m_animateCardsRoutine = null;

        // suit enum
        public enum Suit
        {
            clubs,    // 0 black
            diamonds, // 1 black
            hearts,   // 2 red
            spades    // 3 red
        }

        // rank enum
        public enum Rank
        {
            ace,
            two,
            three,
            four,
            five,
            six,
            seven,
            eight,
            nine,
            ten,
            jack,
            queen,
            king
        }

        public string GetDebugText()
        {
            string textBlock = "Debug Output";
            
            textBlock += $"\n Move count {m_Moves}";
            
            if(stockCards != null)
                textBlock += $"\n Stock count {stockCards.Count}";
            
            if(wasteCards != null)
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

        // Special Tuple
        public struct SuitRank
        {
            public Suit suit { get; set; }
            public Rank rank { get; set; }

            public SuitRank(Suit suit, Rank rank)
            {
                this.suit = suit;
                this.rank = rank;
            }

            // custom ToString method
            public override string ToString()
            {
                return Enum.GetName(typeof(Suit), suit) + " " + Enum.GetName(typeof(Rank), rank);
            }
        }

        public const float TAB_SPACING = 0.1f;

        private void Awake()
        {
            stockCards = new List<SuitRank>();
            wasteCards = new List<SuitRank>();
            handCards = new List<SuitRank>();
            foundationCards = new List<List<SuitRank>>();
            tableauCards = new List<List<SuitRank>>();
        }

        // Start is called before the first frame update
        void Start()
        {
            m_animateCardsRoutine = null;

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
                    Debug.LogError("foundation not found " + goName);
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


            tableaus = new List<GameObject>(7);
            Vector3 tabOrigin = Vector3.zero;
            for (i = 0; i < 7; i++)
            {
                // TODO Change to FindObjectOfType
                GameObject tab = GameObject.Find("PlayPlane/PlayPlaneOffset/Tableau/t" + i.ToString());
                Debug.Log("tab? " + (tab is null));
                if (tab != null)
                {
                    if (i == 0)
                    {
                        tabOrigin = new Vector3(
                            tab.transform.localPosition.x,
                            tab.transform.localPosition.y,
                            tab.transform.localPosition.z
                        );
                        Debug.LogWarning("tabOrigin " + tabOrigin);
                    }
                    else
                    {
                        // make sure the tableaus are evenly spaced apart
                        Vector3 newPos = new Vector3(tabOrigin.x, tabOrigin.y, tabOrigin.z);
                        newPos.x = TAB_SPACING * i;
                        Debug.LogWarning($"tabPos: {i} {newPos}");
                        tab.transform.localPosition = newPos; // tab.transform.InverseTransformPoint(newPos);
                    }

                    tableaus.Add(tab);
                }
            }

            // TODO Change to FindObjectOfType
            stock = GameObject.Find("PlayPlane/PlayPlaneOffset/Stock");
            if (stock == null)
                Debug.LogError("stock not found");
            waste = GameObject.Find("PlayPlane/PlayPlaneOffset/Waste");
            if (waste == null)
                Debug.LogError("waste not found");
            hand = GameObject.Find("PlayPlane/Hand");
            if (hand == null)
                Debug.LogError("hand not found");

            // Instantiate in-memory cards
            cards = new List<Card>();
            log = new List<List<int>>();
            cardIndexLookup = new Dictionary<SuitRank, int>();

            // for 4 suits, and 13 ranks, create 52 cards
            int deckOrder = 0;  
            for (i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Card newCard = new Card((Suit)i, (Rank)j, deckOrder);
                    cards.Add(newCard);

                    cardIndexLookup.Add(newCard.GetSuitRank(), deckOrder);

                    deckOrder++;

                    // generate a string of the game object name based on the current suit & rank
                    // ex: "ace_of_clubs" or "king_of_spades" or "2_of_diamonds"
                    string gameObjectName = newCard.GetGameObjectName();

                    Transform child = transform.Find(gameObjectName);
                    if (!child)
                    {
                        Debug.LogError("child not found " + gameObjectName);
                        continue;
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

            // Now that the cards are instantiated, we want to update the goal position of the card GameObjects to match their deck order
            //SetCardGoalsToDeckPositions();
            Deal();

            // Now that they have goal positions, we want to start a coroutine that animates them
            // TODO: add a StopCoroutine we can call via the UI
            RefreshAnimationCoroutine();


            //Shuffle(1);
            //StartAnimateCardsInfinity();

            //StartCoroutine(AnimateCardsTwo());

        }

        public void Reset()
        {
            m_Moves = 0;
        }

        public void RefreshAnimationCoroutine()
        {
            if (m_animateCardsRoutine != null)
            {
                StopCoroutine(m_animateCardsRoutine);
            }
            m_animateCardsRoutine = AnimateCards();
            StartCoroutine(m_animateCardsRoutine);
        }

        const float RANDOM_AMPLITUDE = 5f;

        public void SetCardGoalsToRandomPositions()
        {
            // get the deck world position
            Vector3 deckPosition = transform.position + Vector3.zero;

            // loop through our cards, and give them a new GoalIdentity based on our calculations
            foreach (Card card in cards)
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
            Vector3 deckPosition = transform.position + Vector3.zero;

            // loop through our cards, and give them a new GoalIdentity based on our calculations
            foreach (Card card in cards)
            {
                Transform cardTransform = card.GetGameObject().transform;

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

        public void Deal()
        {
            // first, we want to collect all the cards into a stack
            SetCardGoalsToDeckPositions();

            //await Task.Delay(1000);

            // let's shuffle the card order 3 times (todo: artifically delay and animate this)
            Shuffle(3);

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
                    stockCards.Add(cards[i].GetSuitRank());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
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

                    Card card = GetCardBySuitRank(suitRankToDeal);

                    bool faceUp = pile == round;

                    // NOTE: inside this method, we handle adding SuitRank to the proper Tableau list
                    SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Tableau, pile), faceUp);

                    //Debug.Log($"Dealing {card.GetGameObjectName()} {round} {pile} {faceUp}");
                }
            }

            // then, for the remaining cards, update their GoalIdentity to place them where the "Stock" Pile should go
            // remember to offset the local Z position a bit to make the cards appear as tho they're stacked on top of each other.
            // the last card should be the lowest z-position (same z as the stock pile guide object) and then each card up to the 0th should be the highest
            // so we should loop backwards through the remaining Stock when setting the positions
            for (int i = stockCards.Count - 1; i >= 0; i--)
            {
                SuitRank cardSuitRank = stockCards[i];
                Card card = GetCardBySuitRank(cardSuitRank);
                // NOTE: inside this method we handle adding SuitRank to the stockCards list
                SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Stock, i), false); /* always face down when adding to stock */
            }
        }

        public void StockToWaste()
        {
            // if no cards left in stock, return
            if (stockCards.Count < 1)
            {
                Debug.LogWarning("StockToWaste: No cards in Stock pile");
                return;
            }
            // take top card (0th) from stockCards list, remove it, and append it to the Waste pile
            // TODO: support 3 at a time mode
            SuitRank cardSuitRank = stockCards[0];
            stockCards.RemoveAt(0);

            Card card = GetCardBySuitRank(cardSuitRank);
            SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Waste, wasteCards.Count), true);
        }

        public Card GetCardBySuitRank(SuitRank suitrank)
        {
            int cardIndex = cardIndexLookup[suitrank];
            return cards[cardIndex];
        }

        public void WasteToStock()
        {
            // if no cards left in waste, return
            if (wasteCards.Count < 1)
            {
                Debug.LogWarning("WasteToStock: No cards in Waste pile");
                return;
            }
            // return all wasteCards to the stockCards list (in reverse order)
            for (int i = wasteCards.Count - 1; i > -1; i--)
            {
                stockCards.Add(wasteCards[i]);
            }
            // reset to empty list
            wasteCards = new List<SuitRank>();
        }

        public enum PlayfieldArea
        {
            Foundation,
            Tableau,
            Stock,
            Waste,
            Hand,
            Deck
        }

        public struct PlayfieldSpot
        {
            public PlayfieldArea area;
            public int index;

            public PlayfieldSpot(PlayfieldArea area, int index)
            {
                this.area = area;
                this.index = index;
            }

            override public string ToString()
            {
                return $"PlayfieldSpot: {area} {index}";
            }
        }

        // when the user double-clicks on a card, we auto-move it to the next best spot
        public PlayfieldSpot? GetNextValidPlayfieldSpotForSuitRank(SuitRank suitrank)
        {

            // check the top card of the foundation first
            Debug.LogWarning("Checking foundation card list for rank: " + suitrank.rank + " suit int: " + (int)suitrank.suit + " " + foundationCards.Count);
            List<SuitRank> foundationList = foundationCards[(int)suitrank.suit];
            SuitRank topCardSR;
            Debug.LogWarning("Foundation List: " + foundationList.Count);
            if (foundationList.Count == 0)
            {
                // if the foundation list is empty, that's our first choice spot
                // if the rank is Rank.ace (0), return the foundation pile for the suit
                if (suitrank.rank == Rank.ace) { return new PlayfieldSpot(PlayfieldArea.Foundation, (int)suitrank.rank); }
            }
            else
            {
                topCardSR = foundationList.Last();
                if ((int)topCardSR.rank == (int)suitrank.rank - 1)
                {
                    // if the top card in the foundation list is one less than this card's rank, that's our first choice spot
                    return new PlayfieldSpot(PlayfieldArea.Foundation, (int)suitrank.rank);
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
                        return new PlayfieldSpot(PlayfieldArea.Tableau, i);
                    }
                }
                else
                {
                    topCardSR = tableauCards[i].Last();
                    if ((int)topCardSR.rank == (int)suitrank.rank - 1 && SuitsAreOpposite(topCardSR.suit, suitrank.suit))
                    {
                        return new PlayfieldSpot(PlayfieldArea.Tableau, i);
                    }
                }
            }

            Debug.LogWarning($"no suggested playfield spot found for {suitrank.ToString()}");

            return null;
        }

        public const float CARD_THICKNESS = 0.0002f;
        public const float TAB_VERT_OFFSET = 0.01f;

        // z-axis rotation is temporary until i fix the orientation of the mesh in blender
        public Quaternion CARD_DEFAULT_ROTATION = Quaternion.Euler(0, 180, 90);

        public void SetCardGoalIDToPlayfieldSpot(Card card, PlayfieldSpot spot, bool faceUp)
        {
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
                    gPos.y = wasteCards.Count * -(CARD_THICKNESS * 5);
                    goalID.SetGoalPositionFromWorldPosition(gPos);
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
                    Debug.Log("added card to hand " + handCards.Count);
                    goalID = new GoalIdentity(card.GetGameObject(), hand);
                    break;
                case PlayfieldArea.Deck:
                    deckCards.Add(card.GetSuitRank());
                    goalID = new GoalIdentity(card.GetGameObject(), this.gameObject);
                    break;
            }

            // z-axis rotation is temporary until i fix the orientation of the mesh in blender
            goalID.rotation = faceUp ? CARD_DEFAULT_ROTATION : Quaternion.Euler(0, 0, 90);

            card.SetGoalIdentity(goalID);
            card.SetPlayfieldSpot(spot);
            card.SetIsFaceUp(faceUp);
        }

        /*[BurstCompile]
        public struct SetCardGoalIDToPlayfieldSpotJob : IJob
        {
            public Card card;
            public PlayfieldSpot spot;
            public bool faceUp;
            public NativeArray<float3> stockCards;
            public NativeArray<float3> wasteCards;
            public NativeArray<NativeArray<float3>> foundationCards;
            public NativeArray<NativeArray<float3>> tableauCards;
            public NativeArray<float3> foundationPositions;
            public NativeArray<float3> tableauPositions;

            public void Execute()
            {
                var cardTX = card.GetGameObject().transform;
                var goalID = new float3(0, 0, 0);
                var index = spot.index;
                var area = spot.area;

                if (area == PlayfieldArea.Stock)
                {
                    faceUp = false; // always face down when adding to stock
                    stockCards.Add(card.GetSuitRank());
                    goalID = stock.transform.position;
                    goalID = math.transform(cardTX.worldToLocalMatrix, goalID);
                    goalID.z += stockCards.Length * -CARD_THICKNESS;
                }
                else if (area == PlayfieldArea.Waste)
                {
                    wasteCards.Add(card.GetSuitRank());
                    goalID = waste.transform.position;
                    goalID = math.transform(cardTX.worldToLocalMatrix, goalID);
                    goalID.y = wasteCards.Length * CARD_THICKNESS;
                }
                else if (area == PlayfieldArea.Foundation)
                {
                    foundationCards[index].Add(card.GetSuitRank());
                    goalID = foundationPositions[index];
                    goalID = math.transform(cardTX.worldToLocalMatrix, goalID);
                    goalID.z = foundationCards[index].Length * CARD_THICKNESS;
                }
                else if (area == PlayfieldArea.Tableau)
                {
                    tableauCards[index].Add(card.GetSuitRank());
                    goalID = tableauPositions[index];
                    goalID = math.transform(cardTX.worldToLocalMatrix, goalID);
                    goalID.z = (tableauCards[index].Length + 1) * -CARD_THICKNESS;
                    goalID.y = (tableauCards[index].Length + 1) * -0.05f;
                }

                // z-axis rotation is temporary until i fix the orientation of the mesh in blender
                var rotation = faceUp ? CARD_DEFAULT_ROTATION : new quaternion(0, 0, math.sin(math.PI / 4), math.cos(math.PI / 4));

                card.SetGoalIdentity(goalID, rotation);
                card.SetPlayfieldSpot(spot);
                card.SetIsFaceUp(faceUp);
            }
        }*/

        public bool SuitsAreOpposite(Suit suitA, Suit suitB)
        {
            if (
                (int)suitA < 2 && (int)suitB > 1 
                || (int)suitA > 1 && (int)suitB < 2)
            {
                return true;
            }
            return false;
        }

        // for this type of animation, we don't set goalID directly
        // we set an Animation and each time we sample the animation, we get a new goalID
        public void StartAnimateCardsInfinity()
        {
            int i = 0;
            foreach (Card card in cards)
            {
                AnimationInfinity cardAnim = new AnimationInfinity(card.GetGameObject());
                cardAnim.delayStart = i * 0.1f;
                cardAnim.delaySetAt = Time.realtimeSinceStartup;
                cardAnim.scaleAnimation = 1.0f;
                card.SetAnimation(cardAnim);
                i++;
            }
        }

        float lerpSpeed = 5f;
        private int m_Moves;

        IEnumerator AnimateCards()
        {
            // Initialize the arrays with the start and goal positions, rotations and scales of the cards
            NativeArray<Vector3> startPositions = new NativeArray<Vector3>(cards.Count, Allocator.Persistent);
            NativeArray<Vector3> goalPositions = new NativeArray<Vector3>(cards.Count, Allocator.Persistent);
            NativeArray<Quaternion> startRotations = new NativeArray<Quaternion>(cards.Count, Allocator.Persistent);
            NativeArray<Quaternion> goalRotations = new NativeArray<Quaternion>(cards.Count, Allocator.Persistent);
            NativeArray<Vector3> startScales = new NativeArray<Vector3>(cards.Count, Allocator.Persistent);
            NativeArray<Vector3> goalScales = new NativeArray<Vector3>(cards.Count, Allocator.Persistent);
            while (true)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    GoalIdentity goalID = cards[i].GetGoalIdentity();
                    Transform cardTX = cards[i].GetGameObject().transform;
                    
                    startPositions[i] = cardTX.localPosition;
                    startRotations[i] = cardTX.localRotation;
                    startScales[i] = cardTX.localScale;

                    goalPositions[i] = goalID.position;
                    goalRotations[i] = goalID.rotation;
                    goalScales[i] = goalID.scale;
                }

                // Create and schedule the job
                // wonder if i could instantiate it once and re-use it...
                var job = new CardLerpJob
                {
                    startPositions = startPositions,
                    goalPositions = goalPositions,
                    startRotations = startRotations,
                    goalRotations = goalRotations,
                    startScales = startScales,
                    goalScales = goalScales,
                    lerpSpeed = lerpSpeed,
                    deltaTime = Time.deltaTime
                };
                JobHandle handle = job.Schedule(startPositions.Length, cards.Count);



                // Wait for the job to complete
                //yield return new WaitUntil(() => handle.IsCompleted);

                handle.Complete();

                // Update the card transforms with the new positions, rotations, and scales
                for (int i = 0; i < cards.Count; i++)
                {
                    // NOTE: the lerp function writes back to the startX arrays with the lerped values
                    Transform cardTx = cards[i].GetGameObject().transform;
                    cardTx.localPosition = job.startPositions[i];
                    cardTx.localRotation = job.startRotations[i];
                    cardTx.localScale = job.startScales[i];
                }

                yield return new WaitForFixedUpdate();
                //yield return new WaitForSeconds(0.5f);
                //yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator AnimateCardsTwo()
        {
            while (true)
            {
                int i = 0;
                foreach (Card card in cards)
                {
                    if (card.animation is not null && card.animation.IsPlaying)
                    {
                        card.animation.Tick(Time.realtimeSinceStartupAsDouble);
                        GoalIdentity goal = card.animation.GetGoalIdentity();

                        if (i == 0)
                        {
                            Debug.Log(card.GetGameObjectName() + " " + goal.position);
                        }

                        Transform card_tx = card.GetGameObject().transform;
                        card_tx.position = goal.position;
                    }
                    i++;
                }
                yield return new WaitForFixedUpdate();
            }
        }

        [BurstCompile]
        struct CardLerpJob : IJobParallelFor
        {
            public NativeArray<Vector3> startPositions;
            public NativeArray<Vector3> goalPositions;
            public NativeArray<Quaternion> startRotations;
            public NativeArray<Quaternion> goalRotations;
            public NativeArray<Vector3> startScales;
            public NativeArray<Vector3> goalScales;
            public float lerpSpeed;
            public float deltaTime;

            public void Execute(int i)
            {
                float laggedTime = deltaTime; // - (i * 1f);
                //Debug.Log(i + " deltaTime "+deltaTime);
                startPositions[i] = Vector3.Lerp(startPositions[i], goalPositions[i], laggedTime * lerpSpeed);
                startRotations[i] = Quaternion.Lerp(startRotations[i], goalRotations[i], laggedTime * lerpSpeed);
                startScales[i] = Vector3.Lerp(startScales[i], goalScales[i], laggedTime * lerpSpeed);
            }
        }
        public int GetCardIndex(SuitRank suitRank)
        {
            return cardIndexLookup[suitRank];
        }

        public void OnSingleClickCard(Card card)
        {
            Debug.Log("OnSingleClickCard handCards Count " + handCards.Count);
            if(handCards.Count < 1)
            {
                TryPickupCardInSpot(card.playfieldSpot, card);
            }
            else
            {
                TryPlaceHandCardToSpot(card.playfieldSpot);
            }
        }

#nullable enable
        public void TryPickupCardInSpot(PlayfieldSpot spot, Card? card = null) {
            SuitRank topCardId;
            
            // pick up the card
            switch (spot.area)
            {
                case PlayfieldArea.Tableau:
                    // try and pick up one or more cards from a tab pile
                    if (card != null)
                        CollectCardsAboveFromTab(card);
                    else
                        Debug.LogWarning("TryPickupCardInSpot need card for tableau pickup attempt");
                    return;

                case PlayfieldArea.Stock:
                    topCardId = stockCards.Last();
                    card = GetCardBySuitRank(topCardId);

                    // remove top card from list
                    stockCards.RemoveAt(stockCards.Count - 1);

                    // move it to the waste pile; face up
                    SetCardGoalIDToPlayfieldSpot(card,
                        new PlayfieldSpot(PlayfieldArea.Waste, 0), true);

                    return;

                case PlayfieldArea.Foundation:
                    topCardId = foundationCards[spot.index].Last();
                    card = GetCardBySuitRank(topCardId);
                    break;

                case PlayfieldArea.Waste:
                    topCardId = wasteCards.Last();
                    card = GetCardBySuitRank(topCardId);
                    break;
            }

            if (card == null)
            {
                Debug.LogWarning("TryPickupCardInSpot card is null");
                return;
            }

            // this method will add the card to the handCards list
            SetCardGoalIDToPlayfieldSpot(card, new PlayfieldSpot(PlayfieldArea.Hand, 0), true);
        }

        public void TryPlaceHandCardToSpot(PlayfieldSpot spot)
        {
            if (handCards.Count < 1)
            {
                Debug.LogWarning("no cards in hand.");
                return;
            }
            
            // we already have a card in our hand, try placing it...
            SuitRank cardInHandSuitRank = handCards.First();
            Card cardInHand = GetCardBySuitRank(cardInHandSuitRank);
            if (CheckMoveIsValid(cardInHand, spot))
            {
                m_Moves++;
                // move it valid, execute it
                bool faceUp = true;
                SetCardGoalIDToPlayfieldSpot(cardInHand, spot, faceUp);
            }
            else
            {
                // move is invalid...
                Debug.LogWarning("Invalid move, try again.");
            }
        }

        public void OnSingleClickEmptyStack(PlayfieldSpot destinationSpot)
        {
            TryPlaceHandCardToSpot(destinationSpot);
        }

        public bool CheckMoveIsValid(Card handCard, PlayfieldSpot destinationSpot)
        {
            // first validate the move is valid
            Debug.LogWarning("CheckMoveIsValid " + handCard.GetGameObjectName() + " to " + destinationSpot.ToString());
            switch (destinationSpot.area)
            {
                case PlayfieldArea.Hand:
                    if(handCards.Count > 0)
                    {
                        Debug.LogWarning("already have a card in your hand");
                        return false;
                    }
                    return true;
                case PlayfieldArea.Stock:
                    Debug.LogWarning("invalid dest: Stock. cannot move card back to stock");
                    return false;
                case PlayfieldArea.Tableau:
                    List<SuitRank> tabCardList = tableauCards[destinationSpot.index];
                    Debug.LogWarning("tabCardList Count" + tabCardList.Count);
                    if (tabCardList.Count == 0)
                    {
                        // tableau is empty, kings only
                        if (handCard.GetRank() == Rank.king)
                        {
                            // king is valid
                            return true;
                        }
                        Debug.LogWarning("invalid dest: cannot place non-king cards in empty tab spots");
                    }
                    else
                    {
                        // validate the top card in the tableau is one rank higher, and opposite suit
                        SuitRank topCardInTabSuitRank = tabCardList.Last();
                        Card topCardInTab = GetCardBySuitRank(topCardInTabSuitRank);
                        bool suitsAreOpposite = SuitsAreOpposite(
                                handCard.GetSuit(),
                                topCardInTab.GetSuit()
                                );
                        Debug.LogWarning($"comparing ranks:\n" +
                            $"topcard rank: {(int)topCardInTab.GetRank()}\n" +
                            $"handcard rank: {(int)handCard.GetRank()}\n" +
                            $"suits are opposite ${suitsAreOpposite}");
                        if (
                            (int) topCardInTab.GetRank() == ((int)handCard.GetRank()+1)
                            && suitsAreOpposite
                        )
                        {
                            // card we're trying to place on top of is valid
                            return true;
                        }
                        Debug.LogWarning($"invalid dest: cannot place {handCard.GetGameObjectName()} on {topCardInTab.GetGameObjectName()} ");
                    }
                    break;
                case PlayfieldArea.Foundation:
                    Debug.LogWarning($"trying to place {handCard.GetGameObjectName()} on {destinationSpot}");
                    List<SuitRank> fCardList = foundationCards[destinationSpot.index];
                    if(fCardList.Count < 1)
                    {
                        // empty, only aces are valid
                        if((int)handCard.GetSuit() == destinationSpot.index && handCard.GetRank() == Rank.ace)
                        {
                            return true;
                        }
                        Debug.LogWarning($"invalid dest: cannot place {handCard.GetGameObjectName()} in empty foundation @ {destinationSpot.index}. aces only.");
                    }
                    else
                    {
                        // validate the top card in the foundation is one rank lower than handCard and the SAME suit
                        SuitRank topCardInFoundationSuitRank = fCardList.Last();
                        Card topCardInFoundation = GetCardBySuitRank(topCardInFoundationSuitRank);
                        if (
                            (int)handCard.GetSuit() == (int)topCardInFoundation.GetSuit()
                            && (int)handCard.GetRank() == (int)topCardInFoundation.GetRank() + 1
                        ) {
                            return true;
                        }
                    }
                    break;
            }
            return false; // default to fales
        }

        public void CollectCardsAboveFromTab(Card card)
        {
            PlayfieldSpot spot = card.playfieldSpot;
            List<SuitRank> cardGroup = new List<SuitRank>();
            List<SuitRank> tableauCardList = tableauCards[spot.index];
            
            if (!card.IsFaceUp)
            {
                // if the card IS the top card, turn it over
                Debug.LogWarning($"is this the top card of the tableau? can we turn it overs? {tableauCardList.IndexOf(card.GetSuitRank())} {tableauCardList.Count}");
                if(tableauCardList.IndexOf(card.GetSuitRank()) == tableauCardList.Count - 1)
                {
                    //card.FlipCard();
                    GameObject cardGO = card.GetGameObject();
                    Transform cardTX = cardGO.transform;
                    card.SetGoalIdentity(new GoalIdentity(
                        cardGO,
                        cardTX.localPosition,
                        cardTX.localRotation * Quaternion.Euler(0.0f, 180.0f, 0.0f),
                        cardTX.localScale
                    ));
                    card.SetIsFaceUp(true);
                    return;
                }    


                Debug.LogWarning("Cannot pick up face down tableau card");
                return;
            }

            
            int cardIndex = tableauCardList.IndexOf(card.GetSuitRank());
            // get the rest of the cards (if any) at a higher index in the list
            for (int i = cardIndex; i < tableauCardList.Count; i++)
            {
                cardGroup.Add(tableauCardList[i]);
            }
            // remove the cards from the tableau
            tableauCardList.RemoveRange(cardIndex, cardGroup.Count);
            // add the cards to the hand
            PickUpCards(cardGroup);
            Debug.Log("picked up cards: ");
            foreach(SuitRank id in cardGroup)
            {
                Debug.Log(id.ToString());
            }
        }

        public void PickUpCards(List<SuitRank> cards)
        {
            // empty list (will be populated by calls to SetCardGoalID...
            handCards = new List<SuitRank>(0);
            int i = 0;
            // set the goal identity of each card to the hand
            foreach (SuitRank id in cards)
            {
                PlayfieldSpot handSpot = new PlayfieldSpot(PlayfieldArea.Hand, i);
                Card card = GetCardBySuitRank(id);
                SetCardGoalIDToPlayfieldSpot(card, handSpot, card.IsFaceUp);
                i++;
            }
        }

        public void TryPlaceCards(List<SuitRank> cards, PlayfieldSpot spot)
        {
            foreach(SuitRank id in cards)
            {
                Card card = GetCardBySuitRank(id);
                SetCardGoalIDToPlayfieldSpot(card, spot, card.IsFaceUp); // retain current face-up-ness
            }
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

        // Animation Coroutine which animates cards from their current positions to a sorted deck stacked
        // cards are .0002 m thick, so the stack should account for offsetting them on the local y axis according to their "depth" in the deck

        // TODO: make this shuffle deckOrder instead of the List<Card> cards list
        public void Shuffle(int iterations)
        {
            log.Clear();
            //List<SuitRank> newDeckOrder = new List<SuitRank>(52);
            for (int i = 0; i < iterations; i++)
            {
                List<int> iteration_log = new List<int>();
                for (int j = 0; j < cards.Count; j++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, cards.Count);
                    Card temp = cards[j];
                    cards[j] = cards[randomIndex];
                    cards[randomIndex] = temp;
                    UpdateCardDeckOrder(cards[j], j);
                    UpdateCardDeckOrder(cards[randomIndex], randomIndex);
                    iteration_log.Add(randomIndex);
                }
                log.Add(iteration_log);
            }
            //deckOrder = newDeckOrder;
        }

        void UpdateCardDeckOrder(Card card, int deckOrder)
        {
            // record the updated order within the cards themselves
            card.SetDeckOrder(deckOrder);
            // update the indexLookup table
            cardIndexLookup[card.GetSuitRank()] = deckOrder;
        }

        // Update is called once per frame
        void Update()
        {
            //if (!m_isDealt)
            //{
                // lock the deck to the players hand to start?
                // TODO: maybe they should pick it up first?
            //}
            // CheckCardAnimationsShouldPlay();   
        }

        void CheckCardAnimationsShouldPlay()
        {
            if (cards.Count == 0)
            {
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;

            foreach (Card card in cards)
            {
                if (card.animation is not null)
                {
                    // play it if it's not playing / was waiting for delay to expire
                    if (!card.animation.IsPlaying)
                    {
                        if (now - card.animation.delaySetAt > card.animation.delayStart)
                        {
                            card.animation.Play();
                        }
                    }
                    else
                    {


                        if (card.animation.playhead > 2.0f)
                        {
                            card.animation.Stop();
                        }
                    }
                }
            }
        }
    }

}