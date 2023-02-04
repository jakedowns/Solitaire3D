using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class StockCardPile : PlayingCardPile
    {

        public StockCardPile(PlayingCardIDList idList) :base(idList) 
        { 
        }

        public new StockCardPile Clone()
        {
            return new StockCardPile(cardList.Clone());
        }
    }
}
