using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PoweredOn
{
    [RequireComponent(typeof(LineRenderer))]
    public class JointVisualizer: MonoBehaviour
    {
        private LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
        }

        void LateUpdate()
        {
            ConfigurableJoint cfJoint = GetComponent<ConfigurableJoint>();
            if (!cfJoint) return;
            Rigidbody connectedBody = cfJoint.connectedBody;
            Vector3 jointPosition = transform.TransformPoint(GetComponent<ConfigurableJoint>().anchor);
            Vector3 connectedPosition = connectedBody.transform.TransformPoint(connectedBody.centerOfMass);

            lineRenderer.SetPosition(0, jointPosition);
            lineRenderer.SetPosition(1, connectedPosition);
        }
    }
}
