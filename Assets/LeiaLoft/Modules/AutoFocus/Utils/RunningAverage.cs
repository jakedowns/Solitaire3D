using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    public class RunningFloatAverage
    {
        private float _average;
        public float Average
        {
            get
            {
                return _average;
            }
        }
        private int _maxSamplesCount;
        public int maxSamplesCount
        {
            get
            {
                return _maxSamplesCount;
            }
            private set
            {
                _maxSamplesCount = Mathf.Max(value, 1);
            }
        }

        private readonly IndexedQueue<float> sampleValues;

        public int Count
        {
            get
            {
                return sampleValues.Count;
            }
        }

        public RunningFloatAverage(int maxSamplesCount)
        {
            this.maxSamplesCount = maxSamplesCount;
            sampleValues = new IndexedQueue<float>(maxSamplesCount);
        }

        public void AddSample(float value)
        {
            sampleValues.Enqueue(value);

            if (sampleValues.Count > maxSamplesCount)
            {
                sampleValues.Dequeue();
            }

            _average = ComputeAverage();
        }

        private float ComputeAverage()
        {
            float count = sampleValues.Count;
            float sum = 0;

            for (int i = 0; i < count; i++)
            {
                sum += sampleValues[i];
            }

            
            float average = sum / count;

            return average;
        }

        public void AddOffset(float offset)
        {
            float count = sampleValues.Count;

            for (int i = 0; i < count; i++)
            {
                sampleValues[i] += offset;
            }
        }

        public void Reset()
        {
            sampleValues.Reset();
        }
    }
}