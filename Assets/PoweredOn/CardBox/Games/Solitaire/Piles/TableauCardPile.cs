using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class TableauCardPile : SolitaireCardPile
    {

        //public TableauCardPile():base() { }

        private new SolitaireGameObject gameObjectType = SolitaireGameObject.None;

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

        internal bool CanReceiveCard(SolitaireCard card)
        {
            //Debug.LogWarning("[Debugging TableauCardPile.CanReceiveCard] cardList.Count = (pile index)" + pile_index + " " + cardList.Count);
            //Debug.LogWarning($"{card}");
            if (Count == 0)
            {
                if (card.GetRank() == Rank.KING)
                {
                    return true;
                }
            }
            else if (SolitaireDeck.SuitColorsAreOpposite(Last().suit, card.GetSuit()))
            {
                if ((int)Last().rank == (int)card.GetRank() + 1)
                {
                    return true;
                }
            }

            return false;

        }

        // why do i need to extend this to get the gameObjectType reference to point to my extended classes' value and not the base value???
        public new GameObject gameObject
        {
            get
            {
                if (gameObjectType == SolitaireGameObject.None)
                {
                    throw new Exception($"{this.GetType().Name} class does not have a proper gameObjectType defined");
                }
                return GameManager.Instance.game.GetGameObjectByType(gameObjectType);
            }
        }
        public PlayfieldSpot GetPlayfieldSpot()
        {
            return new PlayfieldSpot(PlayfieldArea.TABLEAU, pile_index);
        }
    }
}
