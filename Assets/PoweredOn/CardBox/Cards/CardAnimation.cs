using PoweredOn.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn.CardBox.Animations
{
    public class CardAnimation
    {
        public GameObject gameObject;
        public double playhead;
        public bool playing;
        public float delayStart;
        public double delaySetAt;
        public double playbackStartedAt;
        public double playheadAtStart;
        public double lastTick;
        public int loopCount = 0;

        public float duration;

#nullable enable
        public BezierAnimationCurve? curvePosX;
        public BezierAnimationCurve? curvePosY;
        public BezierAnimationCurve? curvePosZ;

        public float samplePosX;
        public float samplePosY;
        public float samplePosZ;

        public float scaleAnimation = 1;

        /*public BezierAnimationCurve? curveRotX;
        public BezierAnimationCurve? curveRotY;
        public BezierAnimationCurve? curveRotZ;
        public BezierAnimationCurve? curveRotW;

        public BezierAnimationCurve? curveScaleX;
        public BezierAnimationCurve? curveScaleY;
        public BezierAnimationCurve? curveScaleZ;*/

        /*public bool IsStopped
        {
            get { return !playing && playhead == 0; }
        }*/

        /*public bool IsPaused
        {
            get { return !playing && playhead > 0; }
        }*/

        /*public bool IsPlaying
        {
            get { return playing; }
        }

        public CardAnimation(GameObject gameO)
        {
            gameObject = gameO;
            playhead = 0;
            duration = 5f; // 5 seconds by default
        }*/

        /*public void Play()
        {
            this.playing = true;
            this.playbackStartedAt = Time.realtimeSinceStartupAsDouble;
            this.playheadAtStart = playhead;
        }

        public void Pause()
        {
            this.playing = false;
        }

        public void Stop()
        {
            this.playing = false;
            this.playhead = 0;
        }*/

        public void Tick(double time)
        {

            double curve_time = (time - this.playbackStartedAt) / this.duration;
            // Debug.Log("playhead " + (time - this.playbackStartedAt) + " / " + this.duration + " = " + curve_time);
            if (curve_time > 1)
            {
                curve_time = curve_time % 1;
            }
            this.playhead = curve_time;
            this.lastTick = time;
        }

        public void SeekTo(double p)
        {
            this.playhead = p;
        }

        public GoalIdentity GetGoalIdentity()
        {
            if (curvePosX == null || curvePosY == null || curvePosZ == null)
                return new GoalIdentity(this.gameObject, Vector3.zero, Quaternion.identity, Vector3.one);

            // update cached sample values
            samplePosX = curvePosX.Evaluate((float)playhead);
            samplePosY = curvePosY.Evaluate((float)playhead);
            samplePosZ = curvePosZ.Evaluate((float)playhead);

            // the animation paths (curves) are like scalable vectors
            // we can scale them, and the objects will move along a bigger volume of space
            samplePosX *= scaleAnimation;
            samplePosY *= scaleAnimation;
            samplePosZ *= scaleAnimation;

            Vector3 goalPosition = new Vector3(
                samplePosX,
                samplePosY,
                samplePosZ
            );

            Quaternion goalRotation = Quaternion.identity;
            Vector3 goalScale = Vector3.one;
            return new GoalIdentity(
                this.gameObject,
                goalPosition,
                goalRotation,
                goalScale
            );
        }
    }
}
