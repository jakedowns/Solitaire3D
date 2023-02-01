namespace PoweredOn
{
    public class Game
    {
        public enum PlayfieldArea
        {
            Foundation,
            Tableau,
            Stock,
            Waste,
            Hand,
            Deck,
            Invalid
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

            public static PlayfieldSpot Invalid
            {
                get { return new PlayfieldSpot(PlayfieldArea.Invalid, -1, -1); }
            }

            override public string ToString()
            {
                return $"PlayfieldSpot: {area} {index}:{subindex}";
            }

            public PlayfieldSpot Clone()
            {
                return new PlayfieldSpot(this.area, this.index, this.subindex);
            }
        }
    }
}