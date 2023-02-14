using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using PoweredOn.CardBox.Animations;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.CardBox.Games;
using NRKernal;
using static PoweredOn.Animations.EasingFunction;
//using UnityEngine.InputSystem;

namespace PoweredOn.Managers
{
    /*
        
        This singleton instance should be globally available so we don't have to pass around game references to subclassses
        
     */
    [ExecuteInEditMode]
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance { 
            get {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<GameManager>();
                }
                return _instance;
            }
            private set {
                _instance = value;
            } 
        }

        public MonoSolitaireDeck monoDeck;
        bool nrealModeEnabled = false;
        Camera mainCamera;
        Camera nrealCamera;
        public float gmi_id;
        GameObject menuGroup;
        
        private void Awake()
        {
            gmi_id = UnityEngine.Random.Range(-10.0f, 10.0f); // System.Guid.NewGuid();
            //Debug.LogWarning("GMI Awake: set id: "+ gmi_id);
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            if (DebugOutput.Instance == null)
            {
                // be the first one to initialize the singleton if need be
                _ = GameObject.FindObjectOfType<DebugOutput>();
            }

            menuGroup = GameObject.Find("MainCanvas/MenuGroup");

            Camera[] finds = Resources.FindObjectsOfTypeAll<Camera>();
            foreach (var _camera in finds)
            {
                if (_camera.gameObject.name == "MainCamera")
                {
                    mainCamera = _camera;
                }
            }

#if !UNITY_EDITOR
            MyInit();
#endif
        }

        public void SetGame(SolitaireGame game)
        {
            this._game = game;
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            (Instance ?? GameObject.FindObjectOfType<GameManager>())?.Reset();
        }
#endif

        public SolitaireGame _game;
        public SolitaireGame game {
            get {
                if (_game == null)
                {
                    Reset();
                }
                return _game;
            }
        }

        private void Reset()
        {
            _game = new SolitaireGame();
            _game.NewGame();
        }

#nullable enable
        IEnumerator? m_animateCardsRoutine = null;

        float lerpSpeed = 5f;
        
        public void RunTests()
        {
            SolitaireGameTests.Run();
        }        

        // Start is called before the first frame update
        void Start()
        {
            // really this only here until I can get Canvas UI buttons responding again.
#if UNITY_ANDROID && !UNITY_EDITOR
            EnableNrealMode();
#endif
            
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            if (DebugOutput.Instance == null)
            {
                Debug.LogWarning("GameManager [Start] DebugOutput.Instance is still null.");
            }
            MyInit();
        }

        public void ToggleMenu()
        {
            menuGroup?.SetActive(!menuGroup.activeSelf);
        }

        public void EnableNrealMode()
        {
            SetNrealMode(true);
        }

        public void DisableNrealMode()
        {
            SetNrealMode(false);
        }

        public void ToggleNrealMode()
        {
            SetNrealMode(!nrealModeEnabled);
        }

        public void ToggleAutoPlay()
        {
            game.ToggleAutoPlay();
        }

        public void SetNrealMode(bool value)
        {
            nrealModeEnabled = value;
            var finds = Resources.FindObjectsOfTypeAll<NRVirtualDisplayer>();
            if (finds.Count() > 0)
            {
                var nrVirtDisplay = finds.First();
                nrVirtDisplay.gameObject.SetActive(value);
            }

            var finds2 = Resources.FindObjectsOfTypeAll<NRHMDPoseTracker>();
            if (finds2.Count() > 0)
            {
                var nrCamera = finds2.First();
                nrCamera.gameObject.SetActive(value);
            }

            var finds3 = Resources.FindObjectsOfTypeAll<NRInput>();
            if (finds3.Count() > 0)
            {
                var nrCamera = finds3.First();
                nrCamera.gameObject.SetActive(value);
            }

            Camera[] findsCameras = Resources.FindObjectsOfTypeAll<Camera>();
            foreach (var _camera in findsCameras)
            {
                if (_camera.gameObject.name == "MainCamera")
                {
                    mainCamera = _camera;
                }
            }

            if(mainCamera == null)
            {
                Debug.LogWarning("main camera not found");
            }
            else
            {
                mainCamera.gameObject.SetActive(!value);
            }

            var mainCanvasCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
            nrealCamera = GameObject.Find("CameraParent/NRCameraRig/CenterCamera")?.GetComponent<Camera>();
            mainCanvasCanvas.worldCamera = nrealModeEnabled ? nrealCamera : mainCamera;
            Debug.Log("main canvas world camera is now " + mainCanvasCanvas.worldCamera.gameObject.name);
        }

        public void MyInit()
        {
            m_animateCardsRoutine = null;

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

        public void FanCardsOut()
        {
            game.deck.FanCardsOut();
        }
        public void CollectCardsToDeck()
        {
            if (game.deck == null)
            {
                game.Deal();
            }
            if(game == null || game.deck == null)
            {
                Debug.LogError("CollectCardsToDeck: game or game deck missing");
            }
            else
            {
                game.deck.CollectCardsToDeck();
            }
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

        public void AutoPlayNextMove()
        {
            game.AutoPlayNextMove();
        }

        public void StartAutoPlay()
        {
            game.StartAutoPlay();
        }

        public void StopAutoPlay()
        {
            game.StopAutoPlay();
        }

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
            NativeArray<float> lerpFactors = new NativeArray<float>(game.deck.cards.Count, Allocator.Persistent);

            // in seconds
            float duration = 1f; // TODO: make speed/duration controllable at a global and per-goal basis (quick,quick,slow)

            Animations.EasingFunction.Ease ease = Animations.EasingFunction.Ease.EaseInOutCubic;
            Function easeFunc = Animations.EasingFunction.GetEasingFunction(ease);
            
            while (true)
            {
                
                float nowTime = Time.realtimeSinceStartup;
                // TODO: make it so we can skip over cards who have flagged that they met their goals to save on animation cycles ("paused/frozen/sleeping")
                for (int i = 0; i < game.deck.cards.Count; i++)
                {
                    SolitaireCard card = game.deck.cards[i];
                    GoalIdentity goalID = card.GetGoalIdentity();
                    GameObject cardGameObj = card.gameObject;
                    if(cardGameObj == null)
                    {
                        Debug.LogWarning("cant animate card, gameObject is null");
                        continue;
                    }
                    Transform cardTX = cardGameObj.transform;

                    // NEW: lerping between a cached "start" position that is cached any time a new GoalID is set
                    // rather than always lerping from the "current" position, which leads to only being able to support "ease-out" easing
                    startPositions[i] = card.prevPosition; // cardTX.localPosition;
                    startRotations[i] = card.prevRotation; // cardTX.localRotation;
                    startScales[i] = card.prevScale; // cardTX.localScale;

                    // TODO: support custom easing functions / curves per-card
                    float deltaT = Time.time - card.goalSetTimestamp;
                    float t; // = (float)(deltaT) / duration;
                    if(deltaT < goalID.delayStart)
                    {
                        t = 0;
                    }
                    else
                    {
                        // offset to keep duration consistent
                        deltaT = Time.time - (card.goalSetTimestamp + goalID.delayStart);
                        t = (float)(deltaT) / duration;
                    }
                    lerpFactors[i] = Mathf.Clamp(easeFunc(0, 1, t), 0, 1);

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
                    delayTimings = delayTimings,
                    lerpFactors = lerpFactors
                };
                JobHandle handle = job.Schedule(startPositions.Length, game.deck.cards.Count);



                // Wait for the job to complete
                //yield return new WaitUntil(() => handle.IsCompleted);

                handle.Complete();

                // Update the card transforms with the new positions, rotations, and scales
                for (int i = 0; i < game.deck.cards.Count; i++)
                {
                    // NOTE: the lerp function writes back to the startX arrays with the lerped values
                    GameObject cardGO = game.deck.cards[i].gameObject;
                    if (cardGO != null)
                    {
                        Transform cardTx = cardGO.transform;
                        cardTx.position = job.startPositions[i];
                        cardTx.localRotation = job.startRotations[i];
                        cardTx.localScale = job.startScales[i];
                    }
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
                            DebugOutput.Instance?.Log(card.GetGameObjectName() + " " + goal.position);
                        }

                        Transform card_tx = card.gameObject.transform;
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
            public NativeArray<float> lerpFactors;
            public NativeArray<bool> delayTimings;

            public void Execute(int i)
            {
                /*if (delayTimings[i]){
                    return;
                }*/

                // write results back to "startPositions"
                // startPositions[i] = Vector3.Lerp(startPositions[i], goalPositions[i], deltaTime * lerpSpeed);
                startPositions[i] = new Vector3(
                    startPositions[i].x + lerpFactors[i] * (goalPositions[i].x - startPositions[i].x),
                    startPositions[i].y + lerpFactors[i] * (goalPositions[i].y - startPositions[i].y),
                    startPositions[i].z + lerpFactors[i] * (goalPositions[i].z - startPositions[i].z)
                );


                /*startRotations[i] = Quaternion.Lerp(startRotations[i], goalRotations[i], deltaTime * lerpSpeed);*/
                startRotations[i] = new Quaternion(
                    startRotations[i].x + lerpFactors[i] * (goalRotations[i].x - startRotations[i].x),
                    startRotations[i].y + lerpFactors[i] * (goalRotations[i].y - startRotations[i].y),
                    startRotations[i].z + lerpFactors[i] * (goalRotations[i].z - startRotations[i].z),
                    startRotations[i].w + lerpFactors[i] * (goalRotations[i].w - startRotations[i].w)
                );


                /*startScales[i] = Vector3.Lerp(startScales[i], goalScales[i], deltaTime * lerpSpeed);*/
                startScales[i] = new Vector3(
                    startScales[i].x + lerpFactors[i] * (goalScales[i].x - startScales[i].x),
                    startScales[i].y + lerpFactors[i] * (goalScales[i].y - startScales[i].y),
                    startScales[i].z + lerpFactors[i] * (goalScales[i].z - startScales[i].z)
                );
            }
        }

        public void NewGame()
        {
            game.NewGame();
        }

        public void Deal()
        {
            game.Deal();
        }

        public void ResetWithPitch() 
        {
            var poseTracker = NRSessionManager.Instance.NRHMDPoseTracker;
            poseTracker.ResetWorldMatrix(true); 
        }

        public void UIRandomize()
        {
            // TODO: move this off game and into Deck
            game.SetCardGoalsToRandomPositions();
        }
        

        public void OnSingleClickCard(SolitaireCard card)
        {
            //Debug.Log($"[GameManager@OnSingleClickCard] {card}");
            game.OnSingleClickCard(card);
        }


        /*public void TryPlaceCards(CardList cards, PlayfieldSpot spot)
        {
            foreach(SuitRank id in cards)
            {
                SolitaireCard card = GetCardBySuitRank(id);
                MoveCardToNewSpot(card, spot, card.IsFaceUp); // retain current face-up-ness
            }
        }*/



        // Animation Coroutine which animates cards from their current positions to a sorted deck stacked
        // cards are .0002 m thick, so the stack should account for offsetting them on the local y axis according to their "depth" in the deck      

        float lastFired = 0;

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            //if (Keyboard.current.f1Key.wasPressedThisFrame)
            if (Input.GetKeyDown(KeyCode.F1))
            {
                UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
            }
#endif

            // Every 1s if autoplay is enabled, play a new move
            if(Time.time - lastFired > 1.0f)
            {
                lastFired = Time.time;
                if (game.autoplaying)
                {
                    game.AutoPlayNextMove();
                }
            }

            /*if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object
                }
                else
                {
                    Debug.Log("you clicked NOTHING");
                }
            }*/
            
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