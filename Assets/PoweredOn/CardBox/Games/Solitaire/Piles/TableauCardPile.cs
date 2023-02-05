using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
namespace PoweredOn.CardBox.Games.Solitaire
{
    public class TableauCardPile : SolitaireCardPile
    {

        //public TableauCardPile():base() { }

        private new SolitaireGameObject gameObjectType;
        public SolitaireGameObject GetGameObjectType()
        {
            return this.gameObjectType;
        }

        public static new TableauCardPile EMPTY
        {
            get
            {
                return new TableauCardPile(PlayingCardIDList.EMPTY, -1);
            }
        }

        public TableauCardPile(PlayingCardIDList cardList, int pile_index) : base(cardList, pile_index)
        {
            
        }

        public new TableauCardPile Clone()
        {
            return new TableauCardPile(cardList.Clone(), pile_index);
        }
    }
}
