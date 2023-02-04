using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class TableauCardPile : PlayingCardPile
    {
        public TableauCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new TableauCardPile Clone()
        {
            return new TableauCardPile(cardList.Clone());
        }
    }
}
