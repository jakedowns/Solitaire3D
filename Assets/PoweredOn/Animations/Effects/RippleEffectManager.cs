using PoweredOn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PoweredOn.GPT.Helpers;

namespace PoweredOn.Animations.Effects
{
    public class Ripple
    {
        public Vector3 origin;
        public float radius;
        
        public RippleEffect parent;
        //public Quaternion rotation;
        public int index;
        public float amplitude;  // amplitude of the sine wave
        public float frequency; // frequency of the sine wave
        float time = 0;        // current time (in seconds)
        public float value = 0.0f; // current value of the sine wave
        public bool alive = true;
        float aliveAt;
        float myDelay;

        public Ripple(Vector3 origin, int index, RippleEffect parent)
        {
            this.origin = origin;
            this.index = index;
            this.parent = parent;
            radius = 0.0f;
            amplitude = parent.options.amplitude - (index * parent.options.decay);
            frequency = parent.options.frequency;
            aliveAt = Time.time;
            myDelay = (parent.options.delayBetween * index);
            //Debug.LogWarning($"new ripple o:{origin} i:{index} delay:{myDelay} amp:{amplitude} freq:{frequency}");
        }

        public void OnUpdate()
        {
            //Debug.LogWarning("Ripple OnUpdate " + index);
            if (!alive)
            {
                return;
            }

            // max lifespan
            //if(Time.time - aliveAt > parent.options.duration)
            // max size
            if(radius > 2.0f)// || amplitude <= 0f)
            {
                alive = false;
                parent.PruneRipple(this.index);
                return;
            }

            //Debug.Log($"{index} : delta " + (Time.time - aliveAt) + " / delay " + myDelay);

            // expands continually after delay
            if (myDelay > 0 && Time.time - aliveAt < myDelay)
            {
                value = 0;
                return;
            }
            myDelay = 0;

            // begin expansion after delay
            radius += parent.options.speed * parent.options.decay;

            // update amplitude (z-height that changes sinosodially)
            value = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * time);

            // adjust the amplitude so that it is from -1 to 1 => -amplitude to +amplitude
            value = value - (amplitude*0.5f);

            value = Mathf.Clamp(value, -parent.options.amplitude, parent.options.amplitude);

            // draw a debug line representing the value
            if (DebugOutput.Instance.ripple_draw_debug_gizmos)
                Debug.DrawLine(origin, origin + new Vector3(0, value*DebugOutput.Instance.ripple_debug_factor, 0), Color.blue, DebugOutput.Instance.ripple_debug_duration);

            if (DebugOutput.Instance.ripple_log)
            {
                Debug.Log($"ripple {index} r:{radius} v:{value} a:{amplitude} f:{frequency} t:{time}");
            }

            // decay the amplitude
            amplitude = Mathf.Clamp01(amplitude * parent.options.decay);
            if (Mathf.Abs(amplitude) < 0.0001)
            {
                amplitude = 0;
            }

            // draw a wire arc gizmo to visualize the ripple radius
            if (DebugOutput.Instance.ripple_draw_debug_gizmos)
                DrawWireCircle.New(origin, radius, Color.blue, false, DebugOutput.Instance.ripple_debug_duration);


            time += Time.deltaTime;
        }

        float POINT_MARGIN = 0.2f;
        public Vector3 ApplyEffectsToPoint(Vector3 point)
        {
            float distance = Mathf.Abs(Vector2.Distance(point, origin));
            // Check if the game object is within the ripple radius
            //Debug.LogWarning($"ApplyEffectsToPoint {point}, distance:{distance}");
            /*if (
                (distance + POINT_MARGIN) > radius - parent.options.wavelength 
                && (distance - POINT_MARGIN) < radius + parent.options.wavelength
            ){*/
                // Calculate the weight for the current ripple
                float weight = Mathf.Clamp01((radius + parent.options.wavelength - distance) / parent.options.wavelength);
                //Debug.LogWarning($"ApplyEffectsToPoint weight:{weight}, zBefore = {point.z}");

                // Apply a Z offset to the game object based on the current ripple value and weight
                point.z += value * weight;

                //Debug.LogWarning($"ApplyEffectsToPoint zAfter:{point.z}");
            /*}*/
            return point;
        }
    }

    public class RippleOptions
    {
        //public float duration = 1f;
        public float speed = 0.1f;
        public int numRipples = 1; //3f;
        public float amplitude = 0.5f;
        public float frequency = 0.01f;
        public float decay = 0.01f;
        public float wavelength = 0.01f;
        public float delayBetween = 0.2f;
    }

    public class RippleEffect
    {
        private Vector3 origin;
        private bool playing = false;
        public RippleOptions options =  new();
        public Guid guid { get; internal set; }
        private List<Ripple> rippleList = new List<Ripple>();
        RippleEffectManager parent;
        //private int[] prunedRipples;
        private int prunedCount;
        public RippleEffect(Vector3 origin, RippleEffectManager parent)
        {
            guid = new Guid();
            //options = DebugOutput.Instance.rippleOptions;

            // gui-controlled values for dialing in
            options.speed = DebugOutput.Instance.ripple_speed;
            options.numRipples = DebugOutput.Instance.ripple_numRipples;
            options.amplitude = DebugOutput.Instance.ripple_amplitude;
            options.frequency = DebugOutput.Instance.ripple_frequency;
            options.decay = DebugOutput.Instance.ripple_decay;
            options.wavelength = DebugOutput.Instance.ripple_wavelength;
            options.delayBetween = DebugOutput.Instance.ripple_delayBetween;


            for (var i = 0; i<options.numRipples; i++)
            {
                rippleList.Add(new Ripple(origin, i, this));
            }
        }

        public void PruneRipple(int ripple_index)
        {
            // remove ripples once beyond max time/size
            prunedCount++;
            // if they've all been pruned, clean up the effect
            if (prunedCount == options.numRipples)
            {
                parent?.PruneRippleEffect(this.guid);
            }

        }

        // TODO: update to use burst-compiled coroutine
        public void OnUpdate()
        {
            //Debug.LogWarning("RippleEffect OnUpdate " + rippleList.Count);
            if (!playing)
            {
                return;
            }
            for(var i = 0; i<rippleList.Count; i++)
            {
                // update each ripple proportional to it's index
                rippleList[i].OnUpdate();
            }
        }

        public void Play()
        {
            this.playing = true;
        }

        public Vector3 ApplyEffectsToPoint(Vector3 point)
        {
            for (var i = 0; i < rippleList.Count; i++)
            {
                point = rippleList[i].ApplyEffectsToPoint(point);
            }
            return point;
        }
    }

    public class RippleEffectManager
    {
        private List<RippleEffect> effectsList;
        private Dictionary<Guid, int> effectsMap = new();
        private Guid[] prunedGuids;

        public RippleEffectManager()
        {
            effectsList = new();
        }

        public Guid NewRippleEffect(Vector3 origin)
        {
            return Guid.NewGuid(); // TODO: bring this back
            if (DebugOutput.Instance.ripple_clear_before)
            {
                effectsList.Clear();
                effectsMap.Clear();
            }

            var newFX = new RippleEffect(origin,this);
            effectsList.Add(newFX);
            effectsMap[newFX.guid] = effectsList.Count - 1;

            newFX.Play();
            return newFX.guid;
        }

        public RippleEffect GetRippleEffectForID(Guid guid)
        {
            return effectsList[effectsMap[guid]];
        }

        public void OnUpdate()
        {
            //Debug.LogWarning("RippleEffectManager OnUpdate " + effectsList.Count);
            // TODO: batch and parallelize
            for(var i = 0; i<effectsList.Count; i++)
            {
                effectsList[i].OnUpdate();
            }
        }

        public void PruneRippleEffect(Guid guid)
        {
            prunedGuids[prunedGuids.Count()-1] = guid;
            if(prunedGuids.Count() == effectsList.Count)
            {
                effectsList.Clear();
                effectsMap.Clear();
            }
            /*effectsList.RemoveAt(effectsMap[guid]);
            effectsMap.Remove(guid);*/
            // TODO: re-index the map
        }
        public Vector3 ApplyEffectsToPoint(Vector3 point)
        {
            for(var i = 0; i<effectsList.Count; i++)
            {
                point = effectsList[i].ApplyEffectsToPoint(point);
            }
            return point;
        }
    }
}
