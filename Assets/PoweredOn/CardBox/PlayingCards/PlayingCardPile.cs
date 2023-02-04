using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardPile
    {
        protected PlayingCardIDList cardList;

        public static PlayingCardPile EMPTY
        {
            get
            {
                return new PlayingCardPile();
            }
        }

        public PlayingCardPile()
        {
            this.cardList = new PlayingCardIDList();
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

        public int Count
        {
            get { return cardList.Count; }
        }

        public SuitRank Last()
        {
            return this.cardList.DefaultIfEmpty(SuitRank.NONE).LastOrDefault();
        }
    }
}
