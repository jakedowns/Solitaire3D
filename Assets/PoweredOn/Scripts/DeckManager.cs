using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using PoweredOn.Animations;
using PoweredOn;
using PoweredOn.PlayingCards;

namespace PoweredOn.Managers
{
    public class DeckManager : MonoBehaviour
    {

        public SolitaireGame game;

#nullable enable
        IEnumerator? m_animateCardsRoutine = null;

        Solitaire3DTests testManager = new Solitaire3DTests();

        float lerpSpeed = 5f;

        private void Awake()
        {

        }
        
        public void RunTests()
        {
            testManager.Run();
        }        

        // Start is called before the first frame update
        void Start()
        {
            m_animateCardsRoutine = null;

            game = new SolitaireGame();
            game.NewGame();

            // Now that the cards are instantiated, we want to update the goal position of the card GameObjects to match their deck order
            //SetCardGoalsToDeckPositions();
            
            // instead, just deal

            //todo let's wait for user to click deck to deal cards
            game.Deal();

            // Now that they have goal positions, we want to start a coroutine that animates them
            // TODO: add a StopCoroutine we can call via the UI
            RefreshAnimationCoroutine();
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

        

        // for this type of animation, we don't set goalID directly
        // we set an Animation and each time we sample the animation, we get a new goalID
        /*public void StartAnimateCardsInfinity()
        {
            int i = 0;
            foreach (Card card in game.deck.cards)
            {
                AnimationInfinity cardAnim = new AnimationInfinity(card.GetGameObject());
                cardAnim.delayStart = i * 0.1f;
                cardAnim.delaySetAt = Time.realtimeSinceStartup;
                cardAnim.scaleAnimation = 1.0f;
                card.SetAnimation(cardAnim);
                i++;
            }
        }*/

        IEnumerator AnimateCards()
        {
            // Initialize the arrays with the start and goal positions, rotations and scales of the cards
            NativeArray<Vector3> startPositions = new NativeArray<Vector3>(game.deck.cards.Count, Allocator.Persistent);
            NativeArray<Vector3> goalPositions = new NativeArray<Vector3>(game.deck.cards.Count, Allocator.Persistent);
            NativeArray<Quaternion> startRotations = new NativeArray<Quaternion>(game.deck.cards.Count, Allocator.Persistent);
            NativeArray<Quaternion> goalRotations = new NativeArray<Quaternion>(game.deck.cards.Count, Allocator.Persistent);
            NativeArray<Vector3> startScales = new NativeArray<Vector3>(game.deck.cards.Count, Allocator.Persistent);
            NativeArray<Vector3> goalScales = new NativeArray<Vector3>(game.deck.cards.Count, Allocator.Persistent);
            NativeArray<bool> delayTimings = new NativeArray<bool>(game.deck.cards.Count, Allocator.Persistent);
            while (true)
            {
                float nowTime = Time.realtimeSinceStartup;
                for (int i = 0; i < game.deck.cards.Count; i++)
                {
                    GoalIdentity goalID = game.deck.cards[i].GetGoalIdentity();
                    Transform cardTX = game.deck.cards[i].GetGameObject().transform;
                    
                    startPositions[i] = cardTX.localPosition;
                    startRotations[i] = cardTX.localRotation;
                    startScales[i] = cardTX.localScale;

                    goalPositions[i] = goalID.position;
                    goalRotations[i] = goalID.rotation;
                    goalScales[i] = goalID.scale;

                    float delaySetAt = goalID.delaySetAt;

                    if(goalID.delayStart > 0.0f)
                    {
                        delayTimings[i] = (nowTime - delaySetAt) > 0 ? true : false;
                    }
                    else
                    {
                        delayTimings[i] = false;
                    }
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
                    deltaTime = Time.deltaTime,
                    delayTimings = delayTimings
                };
                JobHandle handle = job.Schedule(startPositions.Length, game.deck.cards.Count);



                // Wait for the job to complete
                //yield return new WaitUntil(() => handle.IsCompleted);

                handle.Complete();

                // Update the card transforms with the new positions, rotations, and scales
                for (int i = 0; i < game.deck.cards.Count; i++)
                {
                    // NOTE: the lerp function writes back to the startX arrays with the lerped values
                    Transform cardTx = game.deck.cards[i].GetGameObject().transform;
                    cardTx.localPosition = job.startPositions[i];
                    cardTx.localRotation = job.startRotations[i];
                    cardTx.localScale = job.startScales[i];
                }

                yield return new WaitForFixedUpdate();
                //yield return new WaitForSeconds(0.5f);
                //yield return new WaitForEndOfFrame();
            }
        }

        /*IEnumerator AnimateCardsTwo()
        {
            while (true)
            {
                int i = 0;
                foreach (Card card in game.deck.cards)
                {
                    if (card.animation is not null && card.animation.IsPlaying)
                    {
                        card.animation.Tick(Time.realtimeSinceStartupAsDouble);
                        GoalIdentity goal = card.animation.GetGoalIdentity();

                        if (i == 0)
                        {
                            game.m_DebugOutput.Log(card.GetGameObjectName() + " " + goal.position);
                        }

                        Transform card_tx = card.GetGameObject().transform;
                        card_tx.position = goal.position;
                    }
                    i++;
                }
                yield return new WaitForFixedUpdate();
            }
        }*/

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
            public NativeArray<bool> delayTimings;

            public void Execute(int i)
            {
                /*if (delayTimings[i]){
                    return;
                }*/
                startPositions[i] = Vector3.Lerp(startPositions[i], goalPositions[i], deltaTime * lerpSpeed);
                startRotations[i] = Quaternion.Lerp(startRotations[i], goalRotations[i], deltaTime * lerpSpeed);
                startScales[i] = Vector3.Lerp(startScales[i], goalScales[i], deltaTime * lerpSpeed);
            }
        }

        public void NewGame()
        {
            game = new SolitaireGame();
            game.NewGame();
        }

        public void Collect()
        {
            game.SetCardGoalsToDeckPositions();
        }

        public void UIRandomize()
        {
            game.SetCardGoalsToRandomPositions();
        }
        

        public void OnSingleClickCard(Card card)
        {
            game.OnSingleClickCard(card);
        }
        

        /*public void TryPlaceCards(CardList cards, PlayfieldSpot spot)
        {
            foreach(SuitRank id in cards)
            {
                Card card = GetCardBySuitRank(id);
                SetCardGoalIDToPlayfieldSpot(card, spot, card.IsFaceUp); // retain current face-up-ness
            }
        }*/

        

        // Animation Coroutine which animates cards from their current positions to a sorted deck stacked
        // cards are .0002 m thick, so the stack should account for offsetting them on the local y axis according to their "depth" in the deck      

        

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

        /*void CheckCardAnimationsShouldPlay()
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
        }*/
    }

}