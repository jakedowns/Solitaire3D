using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using Unity.VisualScripting;

namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class TableauCardPileGroup : PlayingCardPileGroup
    {
        List<TableauCardPile> piles;
        public TableauCardPileGroup(TableauCardPile[] tableauCardPiles)
        {
            this.piles = new List<TableauCardPile>(tableauCardPiles);
        }

        public TableauCardPileGroup(PlayingCardIDListGroup group)
        {
            piles = new List<TableauCardPile>(group.Count);
            for (int i = 0; i < group.Count; i++)
            {
                piles[i] = new TableauCardPile(group[i]);
            }
        }

        public TableauCardPileGroup(PlayingCardIDList[] tableauCardPileIDLists)
        {
            piles = new List<TableauCardPile>(tableauCardPileIDLists.Count());
            for (int i = 0; i < piles.Count; i++)
            {
                piles[i] = new TableauCardPile(tableauCardPileIDLists[i]);
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


        public int Count
        {
            get
            {
                return this.piles.Count();
            }
        }

        public TableauCardPile this[int index]
        {
            get => this.piles[index];
            set => this.piles[index] = value;
        }

        public IEnumerator<TableauCardPile> GetEnumerator()
        {
            return this.piles.GetEnumerator();
        }
    }
}
