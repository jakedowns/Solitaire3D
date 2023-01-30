using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace PoweredOn.Animations
{
    public class AnimationInfinity: CardAnimation
    {        
        public AnimationInfinity(GameObject gameO): base(gameO)
        {
            curvePosX = new BezierAnimationCurve();
            curvePosY = new BezierAnimationCurve();
            curvePosZ = new BezierAnimationCurve();

            //duration = 3f; // 3 seconds

            // Adding keyframes to the AnimationCurve
            // Starting point of infinity symbol
            curvePosX.AddBezierKey(0, 0, new Vector2(-0.2f, 0.5f), new Vector2(0.2f, 0.5f), 0.5f);

            // First half of upper loop
            curvePosX.AddBezierKey(0.25f, 1, new Vector2(0.2f, 0.5f), new Vector2(0.5f, 0.8f), 0.5f);

            // Second half of upper loop
            curvePosX.AddBezierKey(0.5f, 1, new Vector2(0.5f, 0.8f), new Vector2(-0.5f, 0.8f), 0.5f);

            // First half of lower loop
            curvePosX.AddBezierKey(0.75f, -1, new Vector2(-0.5f, 0.8f), new Vector2(-0.2f, 0.5f), 0.5f);

            // Second half of lower loop
            curvePosX.AddBezierKey(1, 0, new Vector2(-0.2f, 0.5f), new Vector2(-0.2f, -0.5f), 0.5f);
        }
    }

}