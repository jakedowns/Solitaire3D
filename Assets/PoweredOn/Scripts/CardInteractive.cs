﻿/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;
using PoweredOn.Managers;
using System.Collections;

namespace PoweredOn.Objects
{
    public class CardInteractive : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Card card;
        
        /// <summary> The mesh render. </summary>
        private MeshRenderer m_MeshRender;

        // reference to our DeckManager script on our DeckOfCards object
        private DeckManager m_DeckManager;

        private float? lastClickTime = null;

        #nullable enable
        private IEnumerator? waitForDoubleClickTimeout;

        const float DOUBLE_CLICK_THRESHOLD = 0.4f;

        /// <summary> Awakes this object. </summary>
        void Awake()
        {
            m_MeshRender = transform.GetComponent<MeshRenderer>();
            m_DeckManager = GameObject.Find("DeckOfCards").GetComponent<DeckManager>();
        }

        void Start()
        {
            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.APP, () =>
            {
                Debug.Log("ResetWorldMatrix");
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
            m_DeckManager.OnSingleClickCard(this.card);
            //}

            lastClickTime = null;

            yield return null;
        }

        // set card
        public void SetCard(Card card)
        {
            this.card = card;
        }

        // get card
        public Card GetCard()
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
            if((now - lastClickTime) < DOUBLE_CLICK_THRESHOLD)
            {
                if(waitForDoubleClickTimeout != null)
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
            if(!this.card.IsFaceUp)
            {
                Debug.LogWarning("Ignoring double click on face-down card");
                return;
            }
            DeckManager.PlayfieldSpot? next_spot = m_DeckManager.GetNextValidPlayfieldSpotForSuitRank(this.card.GetSuitRank());
            Debug.LogWarning($"double-click {this.card.GetSuitRank()} -> {next_spot}");
            if (next_spot != null)
            {
                m_DeckManager.SetCardGoalIDToPlayfieldSpot(card, (DeckManager.PlayfieldSpot) next_spot, true); /* faceUp = true */
            }
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
