using NRKernal;
using PoweredOn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace PoweredOn.CardBox.PlayingCards
{
    public class MonoPlayingCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private PlayingCard card;

        /// <summary> The mesh render. </summary>
        private MeshRenderer m_MeshRender;

        private float? lastClickTime = null;

        private double? pointerDownAt = null;

        public const float LONG_PRESS_DURATION = 0.5f;

#nullable enable
        private IEnumerator? waitForDoubleClickTimeout;

        const float DOUBLE_CLICK_THRESHOLD = 0.4f;

        /// <summary> Awakes this object. </summary>
        void Awake()
        {
            m_MeshRender = transform.GetComponent<MeshRenderer>();
        }

        void Start()
        {
            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.APP, () =>
            {
                DebugOutput.Instance.Log("ResetWorldMatrix");
                var poseTracker = NRSessionManager.Instance.NRHMDPoseTracker;
                poseTracker.ResetWorldMatrix();
            });
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
        public void SetCard(PlayingCard card)
        {
            this.card = card;
        }

        // get card
        public PlayingCard GetCard()
        {
            return card;
        }

        public GameObject GetGameObject()
        {
            return this.gameObject;
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
                    GameManager.Instance.game.OnLongPressCard(this.card);
                    pointerDownAt = null; // unflag
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownAt = Time.realtimeSinceStartupAsDouble;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerDownAt = null;
        }

        /// <summary> when pointer click, set the cube color to random color. </summary>
        /// <param name="eventData"> Current event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            //m_MeshRender.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            float now = Time.realtimeSinceStartup;

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
            }
            lastClickTime = now;
        }

        public void OnDoubleClick(PointerEventData eventData)
        {
            GameManager.Instance.game.OnDoubleClickCard(this.card);
        }

        /// <summary> when pointer hover, set the cube color to green. </summary>
        /// <param name="eventData"> Current event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            m_MeshRender.material.color = Color.green;
        }

        /// <summary> when pointer exit hover, set the cube color to white. </summary>
        /// <param name="eventData"> Current event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            m_MeshRender.material.color = Color.white;
        }
    }
}
