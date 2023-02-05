using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireCardPile: PlayingCardPile
    {
        public new SolitaireGameObject gameObjectType = SolitaireGameObject.None;
        
        public void Add(SolitaireCard card)
        {
            this.cardList.Add(card.GetSuitRank());
        }

        public new SolitaireCard GetTopCard()
        {
            if (Managers.GameManager.Instance == null)
            {
                Debug.LogWarning("game manager instance is NULL?!");
            }
            else
            {
                Debug.LogWarning("game manager instance exists... it just doesn't have a game?!");
            }
            return Managers.GameManager.Instance.game.deck.GetCardBySuitRank(this.Last());
        }

        public new SuitRank First()
        {
            return this.cardList.DefaultIfEmpty(SuitRank.NONE).FirstOrDefault();
        }
        public new SolitaireCard FirstCard()
        {
            return Managers.GameManager.Instance.game.deck.GetCardBySuitRank(this.First());
        }

        public new GameObject gameObject
        {
            get
            {
                if(Managers.GameManager.Instance == null || Managers.GameManager.Instance.game == null){
                    return null;
                    //return new GameObject(); (this spams tree with objects :( )
                }
                return Managers.GameManager.Instance.game.GetGameObjectByType(gameObjectType);
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
    }
}
