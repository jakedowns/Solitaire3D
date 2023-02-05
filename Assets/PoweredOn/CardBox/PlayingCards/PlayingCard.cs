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

    public class PlayingCard: Card
    {
        /// <summary> The mesh render. </summary>
        private MeshRenderer m_MeshRender;

        // suit
        protected Suit suit;

        // rank
        protected Rank rank;

        protected int deckOrder;

        private MonoPlayingCard monoPlayingCard;

        private SuitRank suitRank;

        protected GameObject selfGameObject;

        //public CardAnimation animation;

        /*private SolitaireGame game;*/

        public bool IsFaceUp;

        public PlayfieldSpot playfieldSpot = new PlayfieldSpot(PlayfieldArea.DECK, -1, -1);
        public PlayfieldSpot previousPlayfieldSpot = new PlayfieldSpot(PlayfieldArea.DECK, -1, -1);

        // TODO: maybe move this to an interface or a trait we can inherit called Animatable or something
        private GoalIdentity goalIdentity;

        // constructor that takes suit and rank
        public PlayingCard(Suit suit, Rank rank, int deckOrder)
        {
            this.suit = suit;
            this.rank = rank;
            this.deckOrder = deckOrder;
            this.suitRank = new SuitRank(suit, rank);
        }

        /*public bool CanBeMovedToFoundation()
        {
            return Managers.GameManager.Instance.game.GetFoundation().CanReceiveCard(this.GetSuitRank());
        }*/

        /*public void SetGame(SolitaireGame game)
        {
            this.game = game;
        }*/

        public SuitRank GetSuitRank()
        {
            return this.suitRank;
        }

        public void SetMonoCard(MonoPlayingCard monoPlayingCard)
        {
            this.monoPlayingCard = monoPlayingCard;
            this.selfGameObject = monoPlayingCard.GetGameObject();
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

        public GameObject GetGameObject()
        {
            if(this.selfGameObject == null)
            {
                DebugOutput.Instance?.LogWarning($"PlayingCard {this} has no GameObject Set");
            }
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

        internal void SetIsFaceUp(bool faceUp)
        {
            this.IsFaceUp = faceUp;
        }

        public void SetPlayfieldSpot(PlayfieldSpot spot)
        {
            this.playfieldSpot = spot;
        }

        public void SetPreviousPlayfieldSpot(PlayfieldSpot spot)
        {
            this.previousPlayfieldSpot = spot;
        }

        override public string ToString()
        {
            return $"{this.rank} of {this.suit} ({this.playfieldSpot}) > (prevSpot: {this.previousPlayfieldSpot})";
        }
    }
}
