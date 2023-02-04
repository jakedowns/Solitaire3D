using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire.Piles
{
    public class WasteCardPile : PlayingCardPile
    {

        public WasteCardPile() { }

        public WasteCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }
        
        public new WasteCardPile Clone()
        {
            return new WasteCardPile(cardList.Clone());
        }
    }
}
