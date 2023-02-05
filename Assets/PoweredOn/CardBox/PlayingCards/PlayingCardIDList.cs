using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardIDList : IList<SuitRank>, IDisposable
    {
        List<SuitRank> list;

        public PlayingCardIDList(int capacity)
        {
            this.list = new List<SuitRank>(capacity);
        }

        public PlayingCardIDList()
        {
            this.list = new List<SuitRank>();
        }

        public PlayingCardIDList(List<SuitRank> list)
        {
            this.list = list;
        }

        public PlayingCardIDList(PlayingCardIDList list)
        {
            this.list = list.GetUnderlyingList();
        }

        public List<SuitRank> GetUnderlyingList()
        {
            return new List<SuitRank>(this.list);
        }

        public PlayingCardIDList Clone()
        {
            return new PlayingCardIDList(this.list);
        }

        public SuitRank this[int index]
        {
            get => this.list[index];
            set => this.list[index] = value;
        }

        public int Count => this.list.Count;

        public int Capacity => this.list.Capacity;

        public bool IsReadOnly => throw new NotImplementedException();

        public static PlayingCardIDList EMPTY { get { return new PlayingCardIDList(); } }

        public void Add(SuitRank item)
        {
            this.list.Add(item);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(SuitRank item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(SuitRank[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            this.list = null;
        }

        public IEnumerator<SuitRank> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(SuitRank item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, SuitRank item)
        {
            this.list.Insert(index, item);
        }

        public bool Remove(SuitRank item)
        {
            return this.list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        public SuitRank LastOrDefault()
        {
            return list.DefaultIfEmpty(SuitRank.NONE).LastOrDefault();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }

}
