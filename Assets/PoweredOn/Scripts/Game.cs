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
            Deck
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

            override public string ToString()
            {
                return $"PlayfieldSpot: {area} {index}:{subindex}";
            }
        }
    }
}