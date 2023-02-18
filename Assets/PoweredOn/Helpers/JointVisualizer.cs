using PoweredOn.CardBox.Games.Solitaire;
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

            ConfigurableJoint[] cfJoints = GetComponents<ConfigurableJoint>();
            if(cfJoints.Count() > 1)
            {
                Debug.LogWarning("Obj has more than one CF joint! " + this.name);
            }

            // change color of line based on joint type
            Color current_color = GetColorForObject(gameObject);
            lineRenderer.startColor = current_color;
            lineRenderer.endColor = current_color;

            Rigidbody connectedBody = cfJoint.connectedBody;
            Vector3 jointPosition = transform.TransformPoint(GetComponent<ConfigurableJoint>().anchor);
            Vector3 connectedPosition = connectedBody.transform.TransformPoint(connectedBody.centerOfMass);

            lineRenderer.SetPosition(0, jointPosition);
            lineRenderer.SetPosition(1, connectedPosition);
        }

        Color GetColorForObject(GameObject obj)
        {
            var name = obj.name;
            MonoSolitaireCard card = obj.GetComponent<MonoSolitaireCard>();
            // if is card: yellow
            if (card != null)
            {
                return Color.yellow;
            }
            var monoBase = obj.GetComponent<MonoSolitaireCardPileBase>();
            if(monoBase == null)
            {
                // orange
                return new Color(1.0f, 1.0f, 0.0f);
            }
            
            switch (monoBase.playfieldArea)
            {
                case PlayfieldArea.TABLEAU: return Color.red;
                case PlayfieldArea.FOUNDATION: return Color.green;
                case PlayfieldArea.DECK: return Color.magenta;
                case PlayfieldArea.STOCK: return Color.blue;
                case PlayfieldArea.WASTE: return Color.yellow;
                case PlayfieldArea.HAND: return Color.white;
            }

            return Color.magenta;
        }
    }
}
