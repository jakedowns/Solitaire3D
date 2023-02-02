/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

using PoweredOn.Animations;

namespace PoweredOn.PlayingCards
{
    /// <summary> A cube interactive test. </summary>
    public class Card
    {
        /// <summary> The mesh render. </summary>
        private MeshRenderer m_MeshRender;

        // suit
        private Suit suit;

        // rank
        private Rank rank;

        private int deckOrder;

        private CardInteractive cardInteractive;

        private SuitRank suitRank;

        private GameObject selfGameObject;

        public CardAnimation animation;

        public bool IsFaceUp;

        public Game.PlayfieldSpot playfieldSpot = new Game.PlayfieldSpot(Game.PlayfieldArea.Deck, -1, -1);
        public Game.PlayfieldSpot previousPlayfieldSpot = new Game.PlayfieldSpot(Game.PlayfieldArea.Deck, -1, -1);

        // TODO: maybe move this to an interface or a trait we can inherit called Animatable or something
        private GoalIdentity goalIdentity;

        // constructor that takes suit and rank
        public Card(Suit suit, Rank rank, int deckOrder)
        {
            this.suit = suit;
            this.rank = rank;
            this.deckOrder = deckOrder;
            this.suitRank = new SuitRank(suit, rank);
        }

        public SuitRank GetSuitRank()
        {
            return this.suitRank;
        }

        public void SetCardInteractive(CardInteractive cardInteractive)
        {
            this.cardInteractive = cardInteractive;
            this.selfGameObject = cardInteractive.GetGameObject();
        }

        public CardInteractive GetCardInteractive()
        {
            return cardInteractive;
        }

        public GoalIdentity GetGoalIdentity()
        {
            if (this.animation != null)
            {
                return this.animation.GetGoalIdentity();
            }
            return this.goalIdentity;
        }

        public string GetGameObjectName()
        {
            return GetRank().ToString() + "_of_" + GetSuit().ToString();
        }

        public GameObject GetGameObject()
        {
            return this.selfGameObject;
        }

        public Rank GetRank()
        {
            return this.rank;
        }

        // GetSuit
        public Suit GetSuit()
        {
            return this.suit;
        }

        // SetDeckOrder
        public void SetDeckOrder(int deckOrder)
        {
            this.deckOrder = deckOrder;
        }

        // GetDeckOrder

        public int GetDeckOrder()
        {
            return this.deckOrder;
        }

        public void SetGoalIdentity(GoalIdentity goalIdentity)
        {
            this.goalIdentity = goalIdentity;
        }

        public void SetAnimation(CardAnimation cardAnimation)
        {
            this.animation = cardAnimation;
        }

        public void StopAnimation()
        {
            if (this.animation != null)
            {
                this.animation.Stop();
            }
        }

        internal void SetIsFaceUp(bool faceUp)
        {
            this.IsFaceUp = faceUp;
        }

        public void SetPlayfieldSpot(Game.PlayfieldSpot spot)
        {
            this.playfieldSpot = spot;
        }

        public void SetPreviousPlayfieldSpot(Game.PlayfieldSpot spot)
        {
            this.previousPlayfieldSpot = spot;
        }

        override public string ToString()
        {
            return $"{this.rank} of {this.suit} ({this.playfieldSpot}) > (prevSpot: {this.previousPlayfieldSpot})";
        }
    }
}
