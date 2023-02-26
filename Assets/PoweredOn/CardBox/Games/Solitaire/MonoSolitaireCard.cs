using NRKernal;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class MonoSolitaireCard: MonoPlayingCard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private SolitaireCard card;

        private Rigidbody rigidBody;

        /// <summary> The mesh render. </summary>
        private MeshRenderer m_MeshRenderer;

        private float? lastClickTime = null;

        private double? pointerDownAt = null;

        private ConfigurableJoint joint;
        public float spring = 10f;
        public float damper = 0.002f;
        private float distance = 0f;
        public float winchSpeed = 0.1f;
        public float maxDistance = 0.5f;
        public float minDistance = 0.001f;
        private GameObject myAnchorGameObject;
        private AudioSource drawSFX;

        [Unity.Collections.ReadOnly]
        public string currentSpot = "";

        [Unity.Collections.ReadOnly]
        public string currentArea = "";

        public Color currentColor { get; private set; }

        public const float LONG_PRESS_DURATION = 0.5f;

#nullable enable
        private IEnumerator? waitForDoubleClickTimeout;

        const float DOUBLE_CLICK_THRESHOLD = 0.4f;

        /// <summary> Awakes this object. </summary>
        void Awake()
        {
            m_MeshRenderer = transform.GetComponent<MeshRenderer>();
        }

        void Start()
        {
            /*rigidBody = GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = false;*/
            //rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // delete any existing AudioSource components
            foreach (AudioSource audioSource in GetComponents<AudioSource>())
            {
                DestroyImmediate(audioSource);
            }

            // attach an audiosource component and hold a reference to it
            drawSFX = gameObject.AddComponent<AudioSource>();
            drawSFX.playOnAwake = false;
            drawSFX.loop = false;
            drawSFX.volume = 0.5f;
            drawSFX.clip = Resources.Load<AudioClip>("Audio/Sfx/Draw");

            DestroySprings();
        }

        public void DestroySprings(){
            // remove any and all existing SpringJoint components in a loop
            foreach(SpringJoint springJoint in GetComponents<SpringJoint>()){
                DestroyImmediate(springJoint);
            }

            foreach(ConfigurableJoint configurableJoint in GetComponents<ConfigurableJoint>()){
                DestroyImmediate(configurableJoint);
            }
        }

        public void SetColor(Color color){
            currentColor = color;
            if(m_MeshRenderer!=null)
                m_MeshRenderer.material.color = color;
        }

        public void SetDebugSpotName(string name)
        {
            this.currentSpot = name;
        }

        public void SetDebugAreaName(string name)
        {
            this.currentArea = name;
        }

        /**
        * NewSpringJoint
        * @param GameObject objA - the object that will be moved
        * @param Vector3 targetPosition - the position that objA will be moved to
        **/
        // void NewSpringJoint(GameObject objA, Vector3 targetPosition){
        //     // create a new SpringJoint component

        //     SpringJoint springJoint = objA.AddComponent<SpringJoint>();


        //     // set the connected body and anchor position
        //     //springJoint.connectedBody = rigidBody;
        //     springJoint.anchor = Vector3.zero;
        //     springJoint.autoConfigureConnectedAnchor = false;
        //     springJoint.connectedAnchor = transform.InverseTransformPoint(targetPosition); //Vector3.zero;

        //     // set up the spring and damper for the joint
        //     springJoint.spring = spring;
        //     springJoint.damper = damper;

        //     // set the target position for the joint
        //     //springJoint.targetPosition = targetPosition;
        // }


        // void NewConfigurableJoint(GameObject objA, GameObject objB){
        //     // create a new ConfigurableJoint component
        //     // remove any existing ConfigurableJoint component
        //     Destroy(objA.GetComponent<ConfigurableJoint>());

        //     joint = objA.AddComponent<ConfigurableJoint>();
        //     rigidBody = GetComponent<Rigidbody>();

        //     // set the connected body and anchor position
        //     joint.connectedBody = objB.GetComponent<Rigidbody>();

        //     // set up the spring and damper for the linear limit spring
        //     SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        //     linearLimitSpring.spring = spring;
        //     linearLimitSpring.damper = damper;
        //     joint.linearLimitSpring = linearLimitSpring;

        //     // set up the spring and damper for the angular drive
        //     JointDrive angularXDrive = new JointDrive();
        //     angularXDrive.maximumForce = spring;
        //     angularXDrive.positionSpring = damper;
        //     joint.angularXDrive = angularXDrive;

        //     JointDrive angularYZDrive = new JointDrive();
        //     angularYZDrive.maximumForce = spring;
        //     angularYZDrive.positionSpring = damper;
        //     joint.angularYZDrive = angularYZDrive;

        //     SoftJointLimitSpring angularXLimitSpring = new SoftJointLimitSpring();
        //     angularXLimitSpring.spring = spring;
        //     angularXLimitSpring.damper = damper;
        //     joint.angularXLimitSpring = angularXLimitSpring;
        // }

        /**
         * 
         * @param position world space, auto converted to local space
         */
        // public void UpdateJointTargetPosition(Vector3 position)
        // {
        //     // set the anchor position
        //     if(joint == null){
        //         return;
        //     }

        //     // calculate the target position based on the distance and winch speed
        //     // float distance = Vector3.Distance(transform.position, position);
        //     // distance = Mathf.Max(minDistance, distance - winchSpeed);
        //     // distance = Mathf.Min(distance, maxDistance);

        //     float distance = minDistance;

        //     // set the target position of the joint's linear limit
        //     joint.linearLimit = new SoftJointLimit { limit = distance };

        //     // start coroutine to loop, reducing the limit until it reaches 0
        //     //StartCoroutine(ReduceJointLimit(distance));
        // }

        // loop to reduce the joint limit
        // IEnumerator ReduceJointLimit(float distance)
        // {
        //     // while the limit is greater than the target position (give or take 0.001f)
        //     while (joint.linearLimit.limit > distance + 0.001f)
        //     {
        //         // reduce the limit by the winch speed
        //         joint.linearLimit = new SoftJointLimit { limit = joint.linearLimit.limit - winchSpeed };

        //         // wait for the next fixedUpdate
        //         yield return new WaitForFixedUpdate();
        //     }
        // }

        void OnMouseDown()
        {
            isClicked = true;
        }

        private bool isClicked = false;

        void FixedUpdate()
        {
        }

        public IEnumerator WaitForDoubleClickTimeout()
        {
            yield return new WaitForSeconds(DOUBLE_CLICK_THRESHOLD);
            //float now = Time.realtimeSinceStartup;
            //if(now - lastClickTime > DOUBLE_CLICK_THRESHOLD)
            //{
            // if this coroutine has a chance to finish, it should register a single click
            // think of this as our single-click event binding
            // it will get cancelled if the user double-clicks
            GameManager.Instance.OnSingleClickCard(this.card);
            //}

            lastClickTime = null;

            yield return null;
        }

        // set card
        public void SetCard(SolitaireCard card)
        {
            this.card = card;

            if(Application.isEditor && !Application.isPlaying){
                return;
            }

            // create an Anchor gameObject to represent my anchor, but make it a child of a different parent
            // so that it doesn't move with the card
            // GameObject prefab = GameObject.Find("CardAnchorPoint");
            // myAnchorGameObject = Instantiate(prefab, GameObject.Find("CardAnchorPoints").transform);
            // myAnchorGameObject.name = "CardAnchorPoint_"+this.card.GetSuitRank();
            // Rigidbody myAnchorGameObjectRigidBody = myAnchorGameObject.AddComponent<Rigidbody>();
            // // disable gravity
            // myAnchorGameObjectRigidBody.isKinematic = true;
            // myAnchorGameObjectRigidBody.useGravity = false;
        }

        // get card
        public SolitaireCard GetCard()
        {
            return card;
        }

        /// <summary> Updates this object. </summary>
        void Update()
        {
            //get controller rotation, and set the value to the cube transform
            //transform.rotation = NRInput.GetRotation();

            if (pointerDownAt != null)
            {
                double delta = Time.realtimeSinceStartupAsDouble - (double)pointerDownAt;
                if (delta > LONG_PRESS_DURATION)
                {
                    pointerDownAt = null; // unflag
                    GameManager.Instance.game.OnLongPressCard(this.card);
                }
            }
        }

        public new void OnPointerDown(PointerEventData eventData)
        {
            pointerDownAt = Time.realtimeSinceStartupAsDouble;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            double delta = Time.realtimeSinceStartupAsDouble - (double)(pointerDownAt ?? Time.realtimeSinceStartupAsDouble);
            if (pointerDownAt != null && delta > 0 && delta < LONG_PRESS_DURATION)
            {
                GameManager.Instance.OnSingleClickCard(this.card);
            }
            pointerDownAt = null;
        }

        /// <summary> when pointer click, set the cube color to random color. </summary>
        /// <param name="eventData"> Current event data.</param>
        public new void OnPointerClick(PointerEventData eventData)
        {
            //m_MeshRenderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            float now = Time.realtimeSinceStartup;
            /*

            if (waitForDoubleClickTimeout == null && lastClickTime == null)
            {
                waitForDoubleClickTimeout = WaitForDoubleClickTimeout();
                if (waitForDoubleClickTimeout != null)
                {
                    StartCoroutine(waitForDoubleClickTimeout);
                }
                lastClickTime = now;
                return;
            }
            if ((now - lastClickTime) < DOUBLE_CLICK_THRESHOLD)
            {
                if (waitForDoubleClickTimeout != null)
                {
                    StopCoroutine(waitForDoubleClickTimeout);
                    waitForDoubleClickTimeout = null;
                }
                OnDoubleClick(eventData);
                lastClickTime = null;
            }*/
            lastClickTime = now;
        }

        /*public void OnDoubleClick(PointerEventData eventData)
        {
            GameManager.Instance.game.OnDoubleClickCard(this.card);
        }*/

        /// <summary> when pointer hover, set the cube color to green. </summary>
        /// <param name="eventData"> Current event data.</param>
        public new void OnPointerEnter(PointerEventData eventData)
        {
            //m_MeshRenderer.material.color = Color.green;
            this.card.UpdateGoalIDScale(Vector3.one * 1.1f);
        }

        /// <summary> when pointer exit hover, set the cube color to white. </summary>
        /// <param name="eventData"> Current event data.</param>
        public new void OnPointerExit(PointerEventData eventData)
        {
            //m_MeshRenderer.material.color = Color.white;
            this.card.UpdateGoalIDScale(Vector3.one);
        }

        internal void PlayFlipSound()
        {
            // play the flip sound effect using the audio source component
            if (
                drawSFX == null
                || GameManager.Instance == null
                || GameManager.Instance.game.IsDealing 
                || GameManager.Instance.game.deck.IsCollectingCardsToDeck
                || GameManager.Instance.game.IsRecyclingWasteToStock
            )
            {
                return;
            }
            this.drawSFX?.Play();
        }
    }
}
