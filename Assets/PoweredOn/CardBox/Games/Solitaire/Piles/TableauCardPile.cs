using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using Unity.VisualScripting;

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
            if(pile_index == -1)
            {
                UnityEngine.Debug.LogWarning("tab card pile with index of -1 created. are you sure?");
            }
            else
            {
                string typeName = $"Tableau{pile_index+1}_Base";
                SolitaireGameObject theType = (SolitaireGameObject)Enum.Parse(typeof(SolitaireGameObject), typeName);
                this.gameObjectType = theType;
            }
        }

        public new TableauCardPile Clone()
        {
            return new TableauCardPile(cardList.Clone(), pile_index);
        }
    }
}
