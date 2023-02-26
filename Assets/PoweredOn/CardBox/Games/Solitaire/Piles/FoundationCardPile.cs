using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public List<SuitRank> GetNextValidCards()
        {
            Suit myPileSuit = (Suit)Enum.Parse(typeof(Suit), pile_index + "");
            var valid = new List<SuitRank>();
            if (this.Count == 0)
            {
                valid.Add(new SuitRank(myPileSuit, Rank.ACE));
                Debug.LogWarning($"foundation is empty, only the ACE matching myPileSuit is a valid next card {pile_index} {myPileSuit} {this}");
                return valid;
            }

            SolitaireCard topCard = this.GetTopCard();
            if (topCard.GetRank() == Rank.KING)
            {
                Debug.LogWarning($"top card is king, no valid next cards for foundation {this}");
                return valid; // empty list
            }

            int nextValidRank = (int)topCard.GetRank() + 1;
            valid.Add(new SuitRank(myPileSuit, (Rank)Enum.Parse(typeof(Rank), nextValidRank + "")));

            Debug.LogWarning($"next valid card for foundation {this}: {valid} \n TopCard: {topCard}");


            return valid;
        }
    }
}
