using PoweredOn.CardBox.Games;
using PoweredOn.CardBox.Games.Solitaire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardPile
    {
        protected PlayingCardIDList cardList;

        //public virtual GameGameObject gameObjectType { get; set; } = GameGameObject.None;

        protected int pile_index = -1;

        public static PlayingCardPile EMPTY
        {
            get
            {
                return new PlayingCardPile();
            }
        }

        public PlayingCard GetTopCard()
        {
            return Managers.GameManager.Instance.game.deck.GetCardBySuitRank(this.Last());
        }

        public SuitRank First(){
            return this.cardList.DefaultIfEmpty(SuitRank.NONE).FirstOrDefault();
        }
        public PlayingCard FirstCard()
        {
            return Managers.GameManager.Instance.game.deck.GetCardBySuitRank(this.First());
        }

        public const GameGameObject gameObjecType = GameGameObject.None;

        public GameObject gameObject
        {
            get {
                throw new NotImplementedException("did you mean to use SolitairePlayingCardPile?");
            }
        }

        public PlayingCardPile()
        {
            this.pile_index = -1;
            this.cardList = new PlayingCardIDList();
        }

        public PlayingCardPile(PlayingCardIDList cardList, int pile_index = -1)
        {
            this.cardList = new PlayingCardIDList(cardList);
            this.pile_index = pile_index;
        }

        public PlayingCardPile(PlayingCardIDList cardList)
        {
            this.cardList = new PlayingCardIDList(cardList);
        }

        public PlayingCardIDList GetCardListImmutable()
        {
            return cardList.Clone();
        }

        public PlayingCardIDList Clone()
        {
            return cardList.Clone();
        }

        //public void Add(PlayingCardID id)
        public void Add(SuitRank id)
        {
            this.cardList.Add(id);
        }

        public void Add(PlayingCard card)
        {
            // TODO: change to card.GetCardID
            this.cardList.Add(card.GetSuitRank());
        }

        public int Count
        {
            get { return cardList.Count; }
        }

        public SuitRank Last()
        {
            Debug.LogWarning("Last: "+this.cardList);
            return this.cardList.DefaultIfEmpty(SuitRank.NONE).LastOrDefault();
        }

        public IEnumerator<SuitRank> GetEnumerator()
        {
            return this.cardList.GetEnumerator();
        }

        public SuitRank this[int index]
        {
            get => this.cardList[index];
            set => this.cardList[index] = value;
        }

        public int IndexOf(SuitRank item)
        {
            return this.cardList.IndexOf(item);
        }

        // RemoveAt
        public void RemoveAt(int index)
        {
            this.cardList.RemoveAt(index);
        }
    }
}
