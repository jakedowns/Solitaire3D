using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.Animations;
using PoweredOn.CardBox.Cards;
using PoweredOn.CardBox.Games.Solitaire;
using UnityEngine;

namespace PoweredOn.CardBox.PlayingCards
{

    public class PlayingCard : Card
    {
        // suit
        protected Suit suit;

        // rank
        protected Rank rank;

        protected int deckOrder;

        private MonoPlayingCard monoPlayingCard;

        private SuitRank suitRank;

        protected GameObject selfGameObject;

        public Vector3 prevPosition { get; internal set; }
        public Quaternion prevRotation { get; internal set; }
        public Vector3 prevScale { get; internal set; }

        public float goalSetTimestamp { get; internal set; }

        //public CardAnimation animation;

        /*private SolitaireGame game;*/

        public bool IsFaceUp { get; internal set; }

        public PlayfieldSpot playfieldSpot = new PlayfieldSpot(PlayfieldArea.DECK, -1, -1);
        public PlayfieldSpot previousPlayfieldSpot = new PlayfieldSpot(PlayfieldArea.DECK, -1, -1);

        // TODO: maybe move this to an interface or a trait we can inherit called Animatable or something
        protected GoalIdentity goalIdentity;

        // constructor that takes suit and rank
        public PlayingCard(Suit suit, Rank rank, int deckOrder)
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

        public void SetMonoCard(MonoPlayingCard monoPlayingCard)
        {
            this.monoPlayingCard = monoPlayingCard;
            this.selfGameObject = monoPlayingCard.gameObject;
        }

        public MonoPlayingCard GetCardInteractive()
        {
            return monoPlayingCard;
        }

        public GoalIdentity GetGoalIdentity()
        {
            /*if (this.animation != null)
            {
                return this.animation.GetGoalIdentity();
            }*/
            return this.goalIdentity;
        }

        public string GetGameObjectName()
        {
            return GetRank().ToString().ToLower() + "_of_" + GetSuit().ToString().ToLower();
        }

        public GameObject gameObject {
            get
            {
                if (this.selfGameObject == null)
                {
                    DebugOutput.Instance?.LogWarning($"PlayingCard has no GameObject Set {this}");
                }
                return this.selfGameObject;
            }
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

        public virtual void SetGoalIdentity(GoalIdentity goalIdentity)
        {
            CacheIDWhenGoalSet();
            //Debug.LogWarning($"new goal identity {this} {goalIdentity}");
            this.goalIdentity = goalIdentity;
        }

        internal void CacheIDWhenGoalSet()
        {
            if(this.gameObject == null)
            {
                return; 
            }
            // track the values that we're leaving so that GameManager can lerp from Start<->End, not just Current<->End
            prevPosition = this.gameObject.transform.position;
            prevRotation = this.gameObject.transform.localRotation;
            prevScale = this.gameObject.transform.localScale;
            goalSetTimestamp = Time.time;
        }

        /*public void SetAnimation(CardAnimation cardAnimation)
        {
            this.animation = cardAnimation;
        }

        public void StopAnimation()
        {
            if (this.animation != null)
            {
                this.animation.Stop();
            }
        }*/

        internal virtual void SetIsFaceUp(bool faceUp)
        {
            this.IsFaceUp = faceUp;
        }

        public virtual void SetPlayfieldSpot(PlayfieldSpot spot)
        {
            //Debug.Log($"card set playfield spot: {this} {spot}");
            this.playfieldSpot = spot;
        }

        public void SetPreviousPlayfieldSpot(PlayfieldSpot spot)
        {
            this.previousPlayfieldSpot = spot;
        }

        override public string ToString()
        {
            return $"{this.rank} of {this.suit} | { (this.IsFaceUp ? "faceUp" : "faceDown") } @ [{this.playfieldSpot}] | prevFrom:[{this.previousPlayfieldSpot}])";
        }
    }
}
