using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn.GPT.Helpers
{
    public static class DrawWireCircle
    {
        public static void New(Vector3 center, float radius, Color color, bool faceCamera = true, float duration = 0f)
        {
            Vector3 normal = faceCamera ? (center - Camera.main.transform.position).normalized : Vector3.up;
            float angle = 360f;
            int numSegments = 32;// Mathf.RoundToInt(radius * 20f);
            DrawWireArc(center, normal, Vector3.right * radius, angle, radius, numSegments, color, duration);
        }
        
        public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
        {
            DrawWireArc(center, normal, from, angle, radius, Vector3.zero, 32);
        }

        public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, int numSegments)
        {
            DrawWireArc(center, normal, from, angle, radius, Vector3.zero, numSegments);
        }

        public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Vector3 axis, int numSegments)
        {
            float angleRadians = angle * Mathf.Deg2Rad;
            float segmentAngle = angleRadians / numSegments;
            float currentAngle = 0.0f;
            Vector3 currentDirection = from.normalized * radius;
            Vector3 axisDirection = axis.normalized * radius;
            Quaternion rotation = Quaternion.AngleAxis(segmentAngle * Mathf.Rad2Deg, normal);

            for (int i = 0; i <= numSegments; ++i)
            {
                Vector3 nextDirection = rotation * currentDirection;
                Vector3 nextAxisDirection = rotation * axisDirection;
                Debug.DrawLine(center + currentDirection + axisDirection, center + nextDirection + nextAxisDirection, Color.white);
                currentDirection = nextDirection;
                axisDirection = nextAxisDirection;
                currentAngle += segmentAngle;
            }
        }

        private static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, int numSegments, Color color, float duration)
        {
            float angleStep = angle / (float)(numSegments - 1);
            Quaternion rotation = Quaternion.LookRotation(normal);
            Vector3 prevPoint = center + rotation * from * radius;
            for (int i = 1; i < numSegments; i++)
            {
                float a = angleStep * (float)i;
                Vector3 nextPoint = center + rotation * Quaternion.AngleAxis(a, normal) * from * radius;
                Debug.DrawLine(prevPoint, nextPoint, color, duration);
                prevPoint = nextPoint;
            }
        }
    }
}
