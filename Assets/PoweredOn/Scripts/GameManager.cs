using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
using PoweredOn.Animations.Effects;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;

namespace PoweredOn.Managers
{
    /*
        
        This singleton instance should be globally available so we don't have to pass around game references to subclassses
        
     */
    /*[ExecuteInEditMode]*/
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance { 
            get {
                if (_instance == null)
                {
                    Debug.LogWarning("Game Manager Instance Reference Is Null");
                }
                return _instance;
            }
            private set {
                _instance = value;
            } 
        }

        public DataStore dataStore { get; private set; } = new DataStore();
        public MonoSolitaireDeck monoDeck;
        public bool nrealModeEnabled = false;
        Camera mainCamera;
        Camera nrealCamera;
        public DifficultyAssistant difficultyAssistant { get; private set; } = new DifficultyAssistant();
        public float gmi_id;
        GameObject menuGroup;
        ParticleSystem ps;
        ParticleSystem foundationOneParticles;
        ParticleSystem foundationTwoParticles;
        ParticleSystem foundationThreeParticles;
        ParticleSystem foundationFourParticles;

        List<ParticleSystem> foundationPS = new List<ParticleSystem>(4);

        AudioSource bgMusicPlayer;
        AudioSource sfxPlayer;
        AudioClip shuffleClip;
        AudioClip dealClip;
        AudioClip hintClip;
        AudioClip errorClip;
        AudioClip clickClip;
        Dropdown assistModeSelect;

        public bool didLoad { get; private set; } = false;
        public bool GoalAnimationSystemEnabled { get; private set; } = true; // disable to use the "joint" system instead of the goal system
        public Camera TargetWorldCam { get; private set; }

        public enum SFX
        {
            Shuffle,
            Deal,
            Hint,
            Error,
            Click
        };

        public Dictionary<SFX, AudioClip> soundDict { get; private set; } = new();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }

            // bind a callback to OnAudioConfigurationChanged
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            // Subscribe to the displaysUpdated event
            //Display.displaysUpdated += OnDisplaysUpdated;

            ps = GameObject.Find("Particle System").GetComponent<ParticleSystem>();

            foundationPS = new List<ParticleSystem>(4) { null, null, null, null };
            // clubs
            foundationOneParticles = GameObject.Find("foundationOneParticles").GetComponent<ParticleSystem>();
            foundationPS[0] = foundationOneParticles;
            // diamonds
            foundationTwoParticles = GameObject.Find("foundationTwoParticles").GetComponent<ParticleSystem>();
            foundationPS[1] = foundationTwoParticles;
            // hearts
            foundationThreeParticles = GameObject.Find("foundationThreeParticles").GetComponent<ParticleSystem>();
            foundationPS[2] = foundationThreeParticles;
            // spades
            foundationFourParticles = GameObject.Find("foundationFourParticles").GetComponent<ParticleSystem>();
            foundationPS[3] = foundationFourParticles;

            GameObject assistModeSelectGameObject = GameObject.Find("AssistMode");
            if (assistModeSelectGameObject != null)
            {
                assistModeSelect = assistModeSelectGameObject.GetComponent<Dropdown>();
                assistModeSelect.onValueChanged.AddListener(delegate
                {
                    DropdownValueChanged(assistModeSelect);
                });
            }

            // shuffle
            shuffleClip = Resources.Load<AudioClip>("Audio/Sfx/Shuffle");
            // deal
            dealClip = Resources.Load<AudioClip>("Audio/Sfx/Deal");
            // hint
            hintClip = Resources.Load<AudioClip>("Audio/Sfx/Hint");
            // error
            errorClip = Resources.Load<AudioClip>("Audio/Sfx/Error");
            // click
            clickClip = Resources.Load<AudioClip>("Audio/Sfx/Click");

            if (new List<AudioClip>(){ shuffleClip, dealClip, hintClip, errorClip, clickClip }.Contains(null)){
                Debug.LogError("failed to load a sound");
            }

            soundDict[SFX.Shuffle] = shuffleClip;
            soundDict[SFX.Deal] = dealClip;
            soundDict[SFX.Hint] = hintClip;
            soundDict[SFX.Error] = errorClip;
            soundDict[SFX.Click] = clickClip;

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

            // Find First in All (incl. Inactive)
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach(var obj in allObjects)
            {
                if ( obj.name == "MenuGroup" && obj.transform.parent.name == "MainCanvas")
                {
                    menuGroup = obj;
                    break;
                }
            }
            if(menuGroup == null)
            {
                Debug.LogWarning("MenuGroup not found");
            }

            Camera[] finds = Resources.FindObjectsOfTypeAll<Camera>();
            foreach (var _camera in finds)
            {
                if (_camera.gameObject.name == "MainCamera")
                {
                    mainCamera = _camera;
                }
            }
        }

        public void OnDisplaysUpdated()
        {
            Debug.Log("OnDisplaysUpdated");
        }

        public void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            Debug.Log("OnAudioConfigurationChanged: deviceWasChanged: " + deviceWasChanged);
            if (deviceWasChanged)
            {
                string[] deviceNames = ListAudioDeviceNames();
            }
        }

        public void RunOneShotFoundationParticlesAfterDelay(int foundationIndex, float delay)
        {
            StartCoroutine(RunOneShotFoundationParticles_delayed(foundationIndex, delay));
        }

        public IEnumerator RunOneShotFoundationParticles_delayed(int foundationIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
            foundationPS[foundationIndex].Play();
            // stop after a small # of seconds
            //StartCoroutine(StopFoundationParticles(foundationIndex, 0.3f));
        }

        // coroutine to stop the foundationPS particle system of a particular index, after a specified amount of time
        /*public IEnumerator StopFoundationParticles(int foundationIndex, float time)
        {
            yield return new WaitForSeconds(time);
            foundationPS[foundationIndex].Stop();
        }*/

        public void SetGame(SolitaireGame game)
        {
            this._game = game;
        }

/*#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            (Instance ?? GameObject.FindObjectOfType<GameManager>())?.Reset();
        }
#endif*/

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

        public void StopParticleSystem()
        {
            ps.Stop();
        }

        public void StartParticleSystem()
        {
            ps.Play();
        }
        
        public void ToggleParticleSystem()
        {
            if (ps.isPlaying)
            {
                StopParticleSystem();
            }
            else
            {
                StartParticleSystem();
            }
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
            // 

            // disable particles at start
            ps.Stop();
            // hide log
#if !UNITY_EDITOR
            DebugOutput.Instance.ToggleLogVisibility(false);
#endif
            // hide menu
            ToggleMenu(false);

            // MaybeEnableNreal
            TryEnableNreal();
            ListAudioDeviceNames();

            bgMusicPlayer = GameObject.Find("BGMusic").GetComponent<AudioSource>();
            sfxPlayer = GameObject.Find("SFXPlayer").GetComponent<AudioSource>();

            
            //Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToLandscapeLeft = false;

            if (DebugOutput.Instance == null)
            {
                Debug.LogWarning("GameManager [Start] DebugOutput.Instance is still null.");
            }
            MyInit();
            if (dataStore.LoadData())
            {
                // apply the decoded gamestate
                game.LoadState(dataStore.userData);
            }
            else
            {
                // deal a fresh game
                game.Deal();
            }

            if (dataStore.userData.bgMusicEnabled)
            {
                bgMusicPlayer.Play();
                //bgMusicPlayer.volume = dataStore.bgMusicVolume;
            }
            else
            {
                bgMusicPlayer.Stop();
            }

            didLoad = true;
        }

        public void OnFOVSliderChange(Slider fovSlider)
        {
            Camera camera = GameObject.Find("RenderCamera")?.GetComponent<Camera>();
            if (camera != null)
            {
                camera.fieldOfView = fovSlider.value;
            }
            else
            {
                Debug.LogWarning("fieldOfView slider: render camera is null");
            }
        }

        public void ToggleMenu()
        {
            if(menuGroup != null)
            {
                menuGroup.SetActive(!menuGroup.activeSelf);
                if (menuGroup.activeSelf)
                {
                    //menuGroup.transform.position = Vector3.zero;

                    // update difficulty slider & text
                    difficultyAssistant.UpdateDifficultyText();
                }
            }
        }
        public void ToggleMenu(bool force)
        {
            if (menuGroup != null)
            {
                menuGroup.SetActive(force);
                if (menuGroup.activeSelf)
                {
                    //menuGroup.transform.position = Vector3.zero;

                    // update difficulty slider & text
                    difficultyAssistant.UpdateDifficultyText();
                }
            }
        }

        public void ToggleDebugColors()
        {
            game.ToggleDebugCardColors();
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

        public void TogglePlayfield()
        {
            game.TogglePlayfield();
        }

        public void ToggleBGMusic()
        {
            dataStore.SetBGMusicEnabled(!dataStore.userData.bgMusicEnabled);
            if (dataStore.userData.bgMusicEnabled)
            {
                bgMusicPlayer.Play();
            }
            else
            {
                bgMusicPlayer.Stop();
            }
        }
        
        public string[] ListAudioDeviceNames()
        {
            // if we're in the editor, use the Unity API to get the list of output audio devices
            //if (Application.isEditor)
            //{
                var config = UnityEngine.AudioSettings.GetConfiguration();
                // list the available fields and properties and getters on the config
                // Print the audio configuration to the console
                Debug.Log("Audio configuration:");
                Debug.Log("  Sample rate: " + config.sampleRate);
                Debug.Log("  Speaker mode: " + config.speakerMode);
                Debug.Log("  DSP buffer size: " + config.dspBufferSize);
                //Debug.Log("  DSP buffer count: " + config.dspBufferCount);
                //Debug.Log("  Output device names: " + string.Join(", ", config.outputDeviceNames));


                string[] deviceNames = new string[0];
                return deviceNames;
            //}
            /*else
            {
                // otherwise, use the Android API to get the list of audio devices
                // get the context from the UnityPlayer class


                AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject audioManager = context.Call<AndroidJavaObject>("getSystemService", new AndroidJavaObject("java.lang.String", "audio"));

                // load the value for the AudioManager const from the AudioManager class
                int GET_DEVICES_OUTPUTS = new AndroidJavaClass("android.media.AudioManager").GetStatic<int>("GET_DEVICES_OUTPUTS");

                AndroidJavaObject[] devices = audioManager.Call<AndroidJavaObject[]>("getDevices", new AndroidJavaObject("java.lang.Integer", GET_DEVICES_OUTPUTS));
                string[] deviceNames = new string[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    string deviceName = devices[i].Call<string>("getProductName");
                    Debug.Log("Device Name: " + deviceName);
                    deviceNames[i] = deviceName;
                }

                return deviceNames;
            }*/
        }

        public void TryEnableNreal()
        {
            // if the platform is android,
            // and ( there's an external display connected OR we're in the simulator )
            // then enable nreal mode
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Display.displays.Length > 1 || Application.isEditor)
                {
                    SetNrealMode(true);
                    return;
                }
            }
            SetNrealMode(false);
        }

        public void SetNrealMode(bool value)
        {
            // if platform is not android, skip
            if (value == true && Application.platform != RuntimePlatform.Android && !Application.isEditor)
            {
                Debug.LogWarning("SetNrealMode, skipping, not android platform");
                return;
            }

            // if there's no external displays connected, force to disabled
            if (Display.displays.Length < 2 && !Application.isEditor)
            {
                Debug.LogWarning("SetNrealMode, skipping, only 1 display detected");
                value = false;
            }

            var findEventSystem = Resources.FindObjectsOfTypeAll<EventSystem>();
            if(findEventSystem.Length > 0)
            {
                foreach(var es in findEventSystem)
                {
                    if(es.gameObject.name == "MainEventSystem")
                    {
                        // Non-NR Event System
                        es.gameObject.SetActive(!value);
                    }
                    else if(es.gameObject.name == "[EventSystem]")
                    {
                        // NR's Event System
                        es.gameObject.SetActive(value);
                    }
                }
            }

            var mainCanvas = GameObject.Find("MainCanvas");
            var mainCanvasGraphicRaycaster = mainCanvas?.GetComponent<GraphicRaycaster>();
            if (mainCanvasGraphicRaycaster != null)
            {
                mainCanvasGraphicRaycaster.enabled = !value;
            }

            Screen.orientation = value ? ScreenOrientation.Portrait : ScreenOrientation.LandscapeLeft;
            nrealModeEnabled = value;
            var finds = Resources.FindObjectsOfTypeAll<NRVirtualDisplayer>();
            if (finds.Count() > 0)
            {
                foreach(var nrVirtDisplay in finds)
                {
                    nrVirtDisplay.gameObject.SetActive(value);
                }
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

            var finds4 = Resources.FindObjectsOfTypeAll<GameObject>();
            if(finds4.Count() > 0)
            {
                bool foundCameraRig = false;
                int i = 0;
                while(!foundCameraRig && i < finds4.Count())
                {
                    GameObject obj = finds4[i];
                    if(obj.name == "NRCameraRig")
                    {
                        NRMultiDisplayManager mdm = obj.GetComponent<NRMultiDisplayManager>();
                        foundCameraRig = true;
                    }
                    i++;
                }
            }

            Camera nrealCam = null;

            Camera[] findsCameras = Resources.FindObjectsOfTypeAll<Camera>();
            foreach (var _camera in findsCameras)
            {
                if (_camera.gameObject.name == "MainCamera")
                {
                    mainCamera = _camera;
                }
                else if(_camera.gameObject.name == "CenterCamera")
                {
                    nrealCam = _camera;
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
            
            if(nrealCam == null)
            {
                Debug.LogWarning("nreal cam not found");
            }
            else
            {
                NRHMDPoseTracker[] hmdPoseTrackers = Resources.FindObjectsOfTypeAll<NRHMDPoseTracker>();
                if (hmdPoseTrackers.Count() > 0)
                {
                    nrealCam = hmdPoseTrackers[0].transform.Find("CenterCamera").GetComponent<Camera>();
                }
            }

            TargetWorldCam = nrealModeEnabled ? nrealCam : mainCamera;
            if (TargetWorldCam == null)
            {
                Debug.LogError("error finding " + (nrealModeEnabled ? "nreal cam" : "main cam"));
            }
            else
            {
                mainCanvasCanvas.worldCamera = TargetWorldCam;
                Debug.Log("main canvas world camera is now " + mainCanvasCanvas.worldCamera.gameObject.name);
            }
            
        }

        public void NewGameAndDeal()
        {
            MyInit(true);
        }

        public void MyInit(bool deal = false)
        {
            m_animateCardsRoutine = null;

            game.NewGame();

            if (deal)
            {
                game.Deal();
            }

            // hide menu
            ToggleMenu(false);

            RefreshAnimationCoroutine();
        }

        public void ToggleGoalAnimationSystem()
        {
            //GoalAnimationSystemEnabled = !GoalAnimationSystemEnabled;
            /*if (!GoalAnimationSystemEnabled)
            {
                JointManager.Instance.Enable();
                StopAnimationCoroutine();
            }
            else
            {*/
                // clean up joints
                //JointManager.Instance.Disable();
                RefreshAnimationCoroutine();
            //}
        }

        public void StopAnimationCoroutine()
        {
            if (m_animateCardsRoutine != null)
            {
                StopCoroutine(m_animateCardsRoutine);
                m_animateCardsRoutine = null;
            }
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

        public void DropdownValueChanged(Dropdown dropdown)
        {
            int i = dropdown.value;
            Debug.LogWarning($"assist mode selected: {i} {assistModeSelect.options[i]} {(DifficultyAssistant.AssistMode)i}");
                
            DifficultyAssistant.AssistMode mode = (DifficultyAssistant.AssistMode)dropdown.value;
            difficultyAssistant.SetAssistMode(mode);
        }
        public void TogglePerCardAssist()
        {
            difficultyAssistant.TogglePerCardAssist();
        }
        public void RefreshAnimationCoroutine()
        {
            //return; // testing physics engine instead
            if (!GoalAnimationSystemEnabled)
            {
                return;
            }
            StopAnimationCoroutine();
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
            Assert.IsTrue(game.deck.cards.Count == 52);
            
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
                // TODO: encapsulate this pattern and apply it to Animations > Effects > RippleEffect
                if (game.fxManager != null)
                {
                    game.fxManager.OnUpdate();
                }
                /*else
                {
                    Debug.LogWarning("no fxManager?");
                }*/

                float nowTime = Time.realtimeSinceStartup;
                // TODO: make it so we can skip over cards who have flagged that they met their goals to save on animation cycles ("paused/frozen/sleeping")
                for (int i = 0; i < game.deck.cards.Count; i++)
                {
                    SolitaireCard card = game.deck.cards[i];
                    GoalIdentity goalID = card.GetGoalIdentity();
                    GameObject cardGameObj = card.gameObject;
                    if(goalID == null)
                    {
                        Debug.LogWarning("goalID not found");
                        continue;
                    }
                    if(cardGameObj == null)
                    {
                        Debug.LogWarning("cant animate card, gameObject is null");
                        continue;
                    }
                    Transform cardTX = cardGameObj.transform;

                    // if it's an Instant goal, there's no lerping, 
                    // todo: maybe only do no-lerp if goalID.HasntBeenMet
                    /*if(goalID.IsInstant)
                    {
                        
                        Transform cardTx = cardGameObj.transform;
                        cardTx.position = goalID.position;
                        //cardTx.position = game.fxManager.ApplyEffectsToPoint(cardTx.position);
                        cardTx.localRotation = goalID.rotation;
                        cardTx.localScale = goalID.scale;
                        continue;
                    }*/

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
                    SolitaireCard card = game.deck.cards[i];
                    GameObject cardGO = card.gameObject;
                    if (cardGO != null && card.GetGoalIdentity() != null)
                    {
                        Transform cardTx = cardGO.transform;
                        cardTx.position = job.startPositions[i];
                        //cardTx.position = game.fxManager.ApplyEffectsToPoint(cardTx.position);
                        cardTx.localRotation = job.startRotations[i];
                        cardTx.localScale = job.startScales[i];
                    }
                    else
                    {
                        Debug.LogWarning($"card game object missing {i} {game.deck.cards[i].gameObjectTypeName}");
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

        public void RestartGame()
        {

            ToggleMenu(false); // hide menu
            game.RestartGame();
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
        
        public void PlaySFX(SFX clipID)
        {
            // TODO: if sfx disabled, bail
            if (sfxPlayer == null || soundDict == null || !soundDict.ContainsKey(clipID)){
                Debug.LogError("no clip for id " + clipID);
                return;
            }
            AudioClip clip = soundDict[clipID];
            sfxPlayer.clip = clip;
            sfxPlayer.PlayOneShot(clip);
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
        float autoplayLastFired = 0;

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

            if (game.fxManager != null)
            {
                game.fxManager.OnUpdate();
            }
            else
            {
                Debug.LogWarning("no fxManager?");
            }

            // Every .3s
            if (Time.time - autoplayLastFired > 0.3f)
            {
                autoplayLastFired = Time.time;
                if (game.autoplaying)
                {
                    game.AutoPlayNextMove();
                }
            }

            // Every 1s
            if (Time.time - lastFired > 1.0f)
            {
                lastFired = Time.time;
                

                if (didLoad)
                {
                    if(game.IsComplete && game.DidCalculateFinalScore)
                    {
                        // do nothing
                    }
                    else if (game.IsComplete && !game.DidCalculateFinalScore)
                    {
                        game.SetDidCalculateFinalScore(true);
                        Debug.Log("Game is complete! Calculating final Score");
                        game.scoreKeeper.CalculateFinalScore();

                        // start particle system
                        ps.Play();
                    }
                    else
                    {
                        // don't tick while dealing
                        if (!game.IsDealing)
                        {
                            game.scoreKeeper.Tick();
                        }
                    }
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

        internal void SetDifficulty(float value)
        {
            difficultyAssistant.SetDifficulty((int)value);
        }

        public void DecreaseDifficulty()
        {
            if (difficultyAssistant.difficulty == 0)
            {
                return;
            }
            int next = difficultyAssistant.difficulty - 1;
            difficultyAssistant.SetDifficulty(next);
            difficultyAssistant.UpdateDifficultyText();
        }

        public void IncreaseDifficulty()
        {
            if (difficultyAssistant.difficulty == 10)
            {
                return;
            }
            int next = difficultyAssistant.difficulty + 1;
            difficultyAssistant.SetDifficulty(next);
            difficultyAssistant.UpdateDifficultyText();
        }

        public void OnSliderChanged(Slider slider)
        {
        
            //var lc = GameObject.Find("MainCamera").GetComponent<LeiaCamera>();
            //var ld = GameObject.FindObjectOfType<LeiaDisplay>();
            switch (slider.gameObject.name)
            {
                /*case "BaselineSlider":
                    
                    lc.BaselineScaling = slider.value;
                    break;

                case "PlaxSlider":

                    lc.CameraShiftScaling = slider.value;
                    break;
                    
                case "FocusSlider":

                    lc.ConvergenceDistance = slider.value;
                    break;*/

                case "ZoomSlider":
                    TargetWorldCam.fieldOfView = 100.0f - slider.value;
                    break;

                /*case "OffsetSlider":
                    game.Z_SPACING = slider.value;
                    break;*/
            }
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