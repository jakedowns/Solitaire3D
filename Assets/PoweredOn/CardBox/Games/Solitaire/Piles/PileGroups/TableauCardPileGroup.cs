using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;

namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class TableauCardPileGroup : PlayingCardPileGroup
    {
        TableauCardPile[] tableauCardPiles;
        public TableauCardPileGroup(TableauCardPile[] tableauCardPiles)
        {
            this.tableauCardPiles = tableauCardPiles;
        }
    }
}
