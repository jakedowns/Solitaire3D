using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
using Unity.VisualScripting;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class TableauCardPileGroup : SolitaireCardPileGroup
    {
        List<TableauCardPile> piles;

        public TableauCardPileGroup()
        {
            this.piles = new List<TableauCardPile>();
            for(int i = 0; i < 7; i++)
            {
                this.piles.Add(new TableauCardPile(new PlayingCardIDList(), i));
            }
        }
        public TableauCardPileGroup(TableauCardPile[] tableauCardPiles)
        {
            this.piles = new List<TableauCardPile>(tableauCardPiles);
        }

        public TableauCardPileGroup(PlayingCardIDListGroup group)
        {
            piles = new List<TableauCardPile>(group.Count);
            for (int i = 0; i < group.Count; i++)
            {
                piles[i] = new TableauCardPile(group[i], i);
            }
        }

        public TableauCardPileGroup(PlayingCardIDList[] tableauCardPileIDLists)
        {
            piles = new List<TableauCardPile>(tableauCardPileIDLists.Count());
            for (int i = 0; i < piles.Count; i++)
            {
                piles[i] = new TableauCardPile(tableauCardPileIDLists[i], i);
            }
        }

        public TableauCardPileGroup Clone()
        {
            TableauCardPile[] newPiles = new TableauCardPile[7];
            for (int i = 0; i < 7; i++)
            {
                newPiles[i] = piles[i].Clone();
            }

            return new TableauCardPileGroup(newPiles);
        }

        public new TableauCardPile this[int index]
        {
            get => this.piles[index];
            set => this.piles[index] = value;
        }

        public new IEnumerator<TableauCardPile> GetEnumerator()
        {
            return this.piles.GetEnumerator();
        }

        public static TableauCardPileGroup EMPTY
        {
            get { return new TableauCardPileGroup(); }
        }

        public new int Count
        {
            get { return this.piles.Count; }
        }

        public override string ToString()
        {
            var outstring = "";
            var i = 0;
            foreach (var pile in this.piles)
            {
                outstring += "pile id: " + i + " " + pile.ToString() + "\n";
                i++;
            }
            return outstring;
        }

        internal List<SuitRank> GetFaceDownCards()
        {
            var faceDownCards = new List<SuitRank>();
            foreach (var pile in piles)
            {
                foreach (var cardID in pile)
                {
                    var card = GameManager.Instance.game.deck.GetCardBySuitRank(cardID);
                    if(card.IsFaceUp == false)
                    {
                        faceDownCards.Add(cardID);
                    }
                }
            }
            return faceDownCards;
        }
    }
}
