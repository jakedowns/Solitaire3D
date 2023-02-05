using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire
{
    public class StockCardPile : SolitaireCardPile
    {

        public new const SolitaireGameObject gameObjectType = SolitaireGameObject.Stock_Base;
        public StockCardPile() { }
        public StockCardPile(PlayingCardIDList idList) :base(idList) 
        { 
        }

        public new static StockCardPile EMPTY
        {
            get { return new StockCardPile(); }
        }

        public new StockCardPile Clone()
        {
            return new StockCardPile(cardList.Clone());
        }
    }
}
