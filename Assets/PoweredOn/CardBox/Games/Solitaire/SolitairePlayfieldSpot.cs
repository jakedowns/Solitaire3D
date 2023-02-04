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
    }
}
