using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
//using Unity.VisualScripting;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireCardPile: PlayingCardPile
    {
        public virtual SolitaireGameObject gameObjectType { get; set; } = SolitaireGameObject.None;

        /*protected GameManager gmi
        {
            get
            {
                return GameManager.Instance ?? GameObject.FindObjectOfType<GameManager>();
            }
        }*/
        public void Add(SolitaireCard card)
        {
            this.cardList.Add(card.GetSuitRank());
        }

        public new SolitaireCard GetTopCard()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("game manager instance is NULL?!");
            }
            return GameManager.Instance.game.deck.GetCardBySuitRank(this.Last());
        }

        public new SuitRank First()
        {
            return this.cardList.DefaultIfEmpty(SuitRank.NONE).FirstOrDefault();
        }
        public new SolitaireCard FirstCard()
        {
            return GameManager.Instance.game.deck.GetCardBySuitRank(this.First());
        }

        public new GameObject gameObject
        {
            get
            {
                if(this.gameObjectType == SolitaireGameObject.None)
                {
                    throw new Exception($"{this.GetType().Name} class does not have a proper gameObjectType defined");
                }
                return GameManager.Instance.game.GetGameObjectByType(gameObjectType);
            }
        }

        public SolitaireCardPile(PlayingCardIDList list) : base(list, -1)
        {
        }

        public SolitaireCardPile(PlayingCardIDList list, int list_index) : base(list, list_index)
        {
        }

        public SolitaireCardPile() : base()
        {
            this.pile_index = -1;
            this.cardList = PlayingCardIDList.EMPTY;
        }

        public new static SolitaireCardPile EMPTY
        {
            get
            {
                return new SolitaireCardPile();
            }
        }

        public IEnumerable<SuitRank> DefaultIfEmpty(SuitRank replacement)
        {
            return this.cardList.DefaultIfEmpty(replacement);
        }

        public SuitRank LastOrDefault()
        {
            return this.cardList.LastOrDefault();
        }

        internal PlayingCardIDList GetFaceUpCards()
        {
            PlayingCardIDList faceUpCardIDs = new();
            foreach(var cardID in cardList)
            {
                var card = GameManager.Instance.game.deck.GetCardBySuitRank(cardID);
                if (card.IsFaceUp)
                {
                    faceUpCardIDs.Add(cardID);
                }
            }
            return faceUpCardIDs;
        }
    }
}
