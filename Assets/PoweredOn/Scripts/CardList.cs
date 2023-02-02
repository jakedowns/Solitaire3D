using System;
using System.Collections;
using System.Collections.Generic;

namespace PoweredOn.PlayingCards
{
    public class CardList : IList<SuitRank>, IDisposable
    {
        List<SuitRank> list;

        public CardList(int capacity)
        {
            this.list = new List<SuitRank>(capacity);
        }

        public CardList()
        {
            this.list = new List<SuitRank>();
        }

        public CardList(List<SuitRank> list)
        {
            this.list = list;
        }

        public CardList(CardList list)
        {
            this.list = list.GetUnderlyingList();
        }

        public List<SuitRank> GetUnderlyingList()
        {
            return new List<SuitRank>(this.list);
        }

        public CardList Clone()
        {
            return new CardList(this.list);
        }

        public SuitRank this[int index]
        {
            get => this.list[index];
            set => this.list[index] = value;
        }

        public int Count => this.list.Count;

        public int Capacity => this.list.Capacity;

        public bool IsReadOnly => throw new NotImplementedException();

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }
}
