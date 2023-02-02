using System;
using System.Collections;
using System.Collections.Generic;

namespace PoweredOn.PlayingCards
{
    public class CardListList : IList<CardList>, IDisposable
    {
        List<CardList> list;

        public CardListList(int capacity)
        {
            this.list = new List<CardList>(capacity);
        }

        public CardListList()
        {
            this.list = new List<CardList>();
        }

        public CardListList(List<CardList> list)
        {
            this.list = list;
        }

        public CardListList(List<List<SuitRank>> inList)
        {
            this.list = new List<CardList>();
            foreach (var l in inList)
                this.list.Add(new CardList(l));
        }

        public CardListList(CardListList list)
        {
            this.list = list.GetUnderlyingList();
        }

        public CardListList Clone()
        {
            CardListList outList = new CardListList();
            foreach (var l in this.list)
                outList.Add(new CardList(l));
            return outList;
        }

        public List<CardList> GetUnderlyingList()
        {
            return new List<CardList>(this.list);
        }

        public CardList this[int index]
        {
            get => this.list[index];
            set => this.list[index] = value;
        }

        public int Count => this.list.Count;

        public int Capacity => this.list.Capacity;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(CardList item)
        {
            this.list.Add(item);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(CardList item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(CardList[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            this.list = null;
        }

        public IEnumerator<CardList> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(CardList item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, CardList item)
        {
            this.list.Insert(index, item);
        }

        public bool Remove(CardList item)
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
