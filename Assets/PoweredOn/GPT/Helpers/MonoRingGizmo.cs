using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn.GPT.Helpers
{
    /* v0.02 Co-pilot */
    public class MonoRingGizmo : MonoBehaviour
    {
        public float radius = 1f; // the radius of the ring
        public float thickness = 0.1f; // the thickness of the ring
        public Color color = Color.white; // the color of the ring
        public int segments = 32; // the number of segments in the ring

        void OnDrawGizmos()
        {
            
        }
    }

    /* v0.01 GPT
    public class RingGizmo : MonoBehaviour
    {
        public float radius = 1f; // the radius of the ring
        public float thickness = 0.1f; // the thickness of the ring
        public Color color = Color.white; // the color of the ring
        public int segments = 32; // the number of segments in the ring

        void OnDrawGizmos()
        {
            // set the gizmo color
            Gizmos.color = color;

            // draw the outer ring
            Gizmos.DrawWireArc(transform.position, transform.up, transform.forward, 360f, radius);

            // draw the inner ring
            Gizmos.DrawWireArc(transform.position, transform.up, transform.forward, 360f, radius - thickness);

            // draw the segments of the ring
            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * 360f;
                Vector3 pointA = Quaternion.AngleAxis(angle, transform.forward) * transform.up * (radius - thickness / 2);
                Vector3 pointB = Quaternion.AngleAxis(angle, transform.forward) * transform.up * (radius + thickness / 2);
                Gizmos.DrawLine(transform.position + pointA, transform.position + pointB);
            }
        }
    }
    */
}
