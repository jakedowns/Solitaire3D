using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public enum PlayfieldArea
    {
        FOUNDATION,
        TABLEAU,
        STOCK,
        WASTE,
        HAND,
        DECK,
        INVALID
    }

    public struct PlayfieldSpot
    {
        public PlayfieldArea area;
        public int index;
        public int subindex;

        public PlayfieldSpot(PlayfieldArea area, int index)
        {
            this.area = area;
            this.index = index;
            this.subindex = -1;
        }

        public PlayfieldSpot(PlayfieldArea area, int index, int subindex)
        {
            this.area = area;
            this.index = index;
            this.subindex = subindex;
        }

        public static PlayfieldSpot INVALID
        {
            get { return new PlayfieldSpot(PlayfieldArea.INVALID, -1, -1); }
        }

        public static PlayfieldSpot DECK
        {
            // TODO: support multiple GameManager instances simultaneously?
            // or use IsTesting to determine which GameManager or GameManagerTesting instance to return.

            // note we return the top index of the pile, not always 0
            // could be -1 by default to define unset
            // be we'd just be doing the lookup elsewhere anyway 

            get { 
                int nextTopIndex = Managers.GameManager.Instance.game.GetDeckCardPile().Count;
                return new PlayfieldSpot(PlayfieldArea.DECK, nextTopIndex); 
            }
        }

        public static PlayfieldSpot STOCK
        {
            get { 
                int nextTopIndex = Managers.GameManager.Instance.game.GetStockCardPile().Count;
                return new PlayfieldSpot(PlayfieldArea.STOCK, nextTopIndex); 
            }
        }

        public static PlayfieldSpot WASTE
        {
            get { 
                int nextTopIndex = Managers.GameManager.Instance.game.GetWasteCardPile().Count;
                return new PlayfieldSpot(PlayfieldArea.WASTE, nextTopIndex); 
            }
        }

        public static PlayfieldSpot HAND
        {
            get { 
                int nextTopIndex = Managers.GameManager.Instance.game.GetHandCardPile().Count;
                return new PlayfieldSpot(PlayfieldArea.HAND, nextTopIndex); 
            }
        }



        override public string ToString()
        {
            return $"PlayfieldSpot: {area} {index}:{subindex}";
        }

        public PlayfieldSpot Clone()
        {
            return new PlayfieldSpot(this.area, this.index, this.subindex);
        }

        public static bool IsInvalidSpot(PlayfieldSpot spot)
        {
            return spot.area == PlayfieldArea.INVALID;
        }

        public bool IsInvalid()
        {
            return IsInvalidSpot(this);
        }

        public static PlayfieldSpot Hand
        {
            get
            {
                // TODO: could make this smart enough to set the index to the top card in the hand
                // (for when we add multiple cards to hand)
                // setting to -1 for now as sort of an uninitialized value
                return new PlayfieldSpot(PlayfieldArea.HAND, -1);
            }
        }

        public static PlayfieldSpot Waste
        {
            get
            {
                // TODO: could make this smart enough to set the index to the top card in the waste pile
                // setting to -1 for now as sort of an uninitialized value
                return new PlayfieldSpot(PlayfieldArea.WASTE, -1);
            }
        }
    }
}
