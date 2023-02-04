using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PoweredOn.CardBox.PlayingCards.PlayingCardIDList;

namespace PoweredOn.CardBox.PlayingCards
{
    public class PlayingCardIDListGroup
    {
        List<PlayingCardIDList> list;

        public PlayingCardIDListGroup(int capacity)
        {
            this.list = new List<PlayingCardIDList>(capacity);
        }

        public PlayingCardIDListGroup()
        {
            this.list = new List<PlayingCardIDList>();
        }

        public PlayingCardIDListGroup(List<PlayingCardIDList> list)
        {
            this.list = list;
        }

        public PlayingCardIDListGroup(List<List<SuitRank>> inList)
        {
            this.list = new List<PlayingCardIDList>();
            foreach (var l in inList)
                this.list.Add(new PlayingCardIDList(l));
        }

        public PlayingCardIDListGroup(PlayingCardIDListGroup list)
        {
            this.list = list.GetUnderlyingList();
        }

        public PlayingCardIDListGroup Clone()
        {
            PlayingCardIDListGroup outList = new PlayingCardIDListGroup();
            foreach (var l in this.list)
                outList.Add(new PlayingCardIDList(l));
            return outList;
        }

        public List<PlayingCardIDList> GetUnderlyingList()
        {
            return new List<PlayingCardIDList>(this.list);
        }

        public PlayingCardIDList this[int index]
        {
            get => this.list[index];
            set => this.list[index] = value;
        }

        public int Count => this.list.Count;

        public int Capacity => this.list.Capacity;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(PlayingCardIDList item)
        {
            this.list.Add(item);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(PlayingCardIDList item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(PlayingCardIDList[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            this.list = null;
        }

        public IEnumerator<PlayingCardIDList> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(PlayingCardIDList item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, PlayingCardIDList item)
        {
            this.list.Insert(index, item);
        }

        public bool Remove(PlayingCardIDList item)
        {
            return this.list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        /*IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }*/
    }
}
