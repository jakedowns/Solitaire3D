using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire
{
    public class WasteCardPile : SolitaireCardPile
    {

        public WasteCardPile() { }

        public WasteCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new static WasteCardPile EMPTY
        {
            get { return new WasteCardPile(); }
        }

        public new WasteCardPile Clone()
        {
            return new WasteCardPile(cardList.Clone());
        }
    }
}
