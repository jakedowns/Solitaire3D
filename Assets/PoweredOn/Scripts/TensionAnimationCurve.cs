using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoweredOn.Animations
{
    using System;
    using System.Linq;
    using UnityEngine;

    public class BezierAnimationCurve : AnimationCurve
    {
        public struct Key
        {
            public float time;
            public float value;
            public Vector2 inHandle;
            public Vector2 outHandle;
            public float tension;

            public Key(float time, float value, Vector2 inHandle, Vector2 outHandle, float tension)
            {
                this.time = time;
                this.value = value;
                this.inHandle = inHandle;
                this.outHandle = outHandle;
                this.tension = tension;
            }
        }

        private new Key[] keys;

        public BezierAnimationCurve()
        {
            keys = new Key[0];
        }

        public BezierAnimationCurve(Keyframe[] keys)
        {
            this.keys = new Key[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                this.keys[i] = new Key(keys[i].time, keys[i].value, Vector2.zero, Vector2.zero, 0);
            }
        }

        /**
         * Adds a new key to the keys array, sorted by time.
         * If there is an existing key at the requested "time," it will overwrite the existing key and log a warning.
         *
         * @param time The time at which the key should be added
         * @param value The value of the key
         * @param inHandle The in-handle of the bezier curve
         * @param outHandle The out-handle of the bezier curve
         * @param tension The tension of the bezier curve
         **/
        public void AddBezierKey(float time, float value, Vector2 inHandle, Vector2 outHandle, float tension)
        {
            Key newKey = new Key(time, value, inHandle, outHandle, tension);
            int index = GetKeyIndex(time);

            // Check if there is an existing key at the requested "time"
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].time == time)
                {
                    Debug.LogWarning($"Overwriting key at time {time} {keys[i].value} with {value}");
                    keys[i] = newKey;
                    return;
                }
            }

            // If no existing key was found, insert the new key in the correct position
            int insertionIndex = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].time < time)
                {
                    insertionIndex = i + 1;
                }
            }

            Array.Resize(ref keys, keys.Length + 1);
            Array.Copy(keys, insertionIndex, keys, insertionIndex + 1, keys.Length - insertionIndex - 1);
            keys[insertionIndex] = newKey;
        }

        public float Evaluate(float time, float tension)
        {
            // Find the keyframes surrounding the current time
            int prevKeyIndex = GetPreviousKeyframeIndex(time);
            int nextKeyIndex = prevKeyIndex + 1;
            if(nextKeyIndex > keys.Length - 1)
            {
                nextKeyIndex = 0;
            }

            // Get the keyframe values and handles
            float prevTime = keys[prevKeyIndex].time;
            float nextTime = keys[nextKeyIndex].time;
            float prevValue = keys[prevKeyIndex].value;
            float nextValue = keys[nextKeyIndex].value;
            Vector2 prevInHandle = keys[prevKeyIndex].inHandle;
            Vector2 prevOutHandle = keys[prevKeyIndex].outHandle;
            Vector2 nextInHandle = keys[nextKeyIndex].inHandle;
            Vector2 nextOutHandle = keys[nextKeyIndex].outHandle;

            // Interpolate between the keyframe values and handles
            float t = (time - prevTime) / (nextTime - prevTime);
            float tt = t * t;
            float ttt = tt * t;
            float s = 1 - t;
            float ss = s * s;
            float sss = ss * s;
            Vector2 value = 
                ((sss * prevValue) * Vector2.one) + 
                (3 * ss * t * prevOutHandle) + 
                (3 * s * tt * nextInHandle) + 
                ((ttt * nextValue) * Vector2.one);

            // Use the interpolated values to calculate the final point on the Bezier curve
            return value.x + value.y;
        }

        private int GetKeyIndex(float time)
        {
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (time >= keys[i].time && time <= keys[i + 1].time)
                {
                    return i;
                }
            }
            return 0;
        }

        private int GetPreviousKeyframeIndex(float time)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].time > time)
                {
                    if (i > 0)
                        return i - 1;
                    else
                        return i;
                }
            }
            return keys.Length - 1;
        }
    }
}