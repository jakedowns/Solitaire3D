using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class FoundationCardPile : SolitaireCardPile
    {
        //private int pile_index;

        public FoundationCardPile(int index) : base()
        {
            this.pile_index = index;
            SetSelfGameObjectType();
        }

        public FoundationCardPile(PlayingCardIDList cardList, int pile_index) : base(cardList, pile_index)
        {
            SetSelfGameObjectType();
        }
        void SetSelfGameObjectType()
        {
            string typeName = $"Foundation{pile_index + 1}_Base";
            SolitaireGameObject theType = (SolitaireGameObject)Enum.Parse(typeof(SolitaireGameObject), typeName);
            this.gameObjectType = theType;
        }

        /*public new int Count
        {
            get { return this.cardList.Count; }
        }*/

        public new FoundationCardPile Clone()
        {
            return new FoundationCardPile(cardList.Clone(), this.pile_index);
        }
        public bool CanReceiveCard(SolitaireCard card)
        {
            //UnityEngine.Debug.LogWarning($"[Debugging FoundationCardPile.CanReceiveCard] cardList.Count {cardList.Count} (pile index) {pile_index} | Card: {card}");
            if (Count == 0)
            {
                if (card.GetRank() == Rank.ACE)
                {
                    return true;
                }
            }
            else if (Last().suit == card.GetSuit())
            {
                if ((int)Last().rank == (int)card.GetRank() - 1)
                {
                    return true;
                }
            }

            return false;
        }
        
        public PlayfieldSpot GetPlayfieldSpot()
        {
           return new PlayfieldSpot(PlayfieldArea.FOUNDATION, pile_index, Count);
        }

        internal IEnumerable<SuitRank> GetCardIDs()
        {
            var cards = new List<SuitRank>();
            foreach (var cardID in cardList)
            {
                cards.Add(cardID);
            }
            return cards;
        }
    }
}
