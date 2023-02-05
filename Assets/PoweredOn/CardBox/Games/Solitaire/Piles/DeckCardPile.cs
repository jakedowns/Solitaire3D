using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class DeckCardPile : SolitaireCardPile
    {
        public new const SolitaireGameObject gameObjectType = SolitaireGameObject.Deck_Base;
        public DeckCardPile() { }
        public DeckCardPile(DeckCardPile pile) {
            cardList = pile.cardList.Clone();
        }
        public DeckCardPile(PlayingCardIDList cardList) : base(cardList)
        {
        }

        public new static DeckCardPile EMPTY
        {
            get { return new DeckCardPile(); }
        }

        public new DeckCardPile Clone()
        {
            return new DeckCardPile(cardList.Clone());
        }
    }
}
