
using UnityEngine;

namespace LeiaLoft
{
    public class IndexedQueue<T>
    {
        private int currentPosition = 0;
        private int count;
        readonly private T[] values;

        public int Count
        {
            get
            {
                return count;
            }
        }

        public IndexedQueue(int startCount)
        {
            values = new T[startCount];
        }

        public void Enqueue(T value)
        {
            values[currentPosition] = value;
            currentPosition++;
            if (currentPosition == values.Length)
            {
                currentPosition = 0;
            }
            count = Mathf.Min(values.Length, count + 1);
        }

        public T Dequeue()
        {
            if (count > 0)
            {
                T dequeuedValue = values[currentPosition];
                currentPosition--;
                if (currentPosition == -1)
                {
                    currentPosition = values.Length - 1;
                }
                count--;
            return dequeuedValue;
            }
            return default(T);
        }

        public void Reset()
        {
            for(int i = 0; i < values.Length; i++)
            {
                values[i] = default(T);
            }
            currentPosition = 0;
        }
        int BoundReadPosition(int position)
        {
            if (position >= values.Length)
                position -= values.Length;

            return position;
        }

        public T this[int position]
        {
            get
            {
                return values[BoundReadPosition(position)];
            }
            set
            {
                values[BoundReadPosition(position)] = value;
            }
        }
    }
}