﻿using UnityEngine;

namespace LeiaLoft.Examples
{
    public class AutoFocusMethodChooser : MonoBehaviour
    {
        enum Method {DepthCamera = 0, Raycast = 1, Target = 2 };

#pragma warning disable 649
        [SerializeField] private LeiaDepthFocus depthFocus;
        [SerializeField] private LeiaRaycastFocus raycastFocus;
        [SerializeField] private LeiaTargetFocus targetFocus;
#pragma warning restore 649

        public void OnValueChange(int chosenMethod)
        {
            Method method = (Method) chosenMethod;

            Debug.AssertFormat(depthFocus != null, "Variable {0} on component {1} on gameObject {2} was not set", "depthFocus", this.GetType(), gameObject);
            Debug.AssertFormat(raycastFocus != null, "Variable {0} on component {1} on gameObject {2} was not set", "raycastFocus", this.GetType(), gameObject);
            Debug.AssertFormat(targetFocus != null, "Variable {0} on component {1} on gameObject {2} was not set", "targetFocus", this.GetType(), gameObject);

            depthFocus.enabled = (method == Method.DepthCamera);
            raycastFocus.enabled = (method == Method.Raycast);
            targetFocus.enabled = (method == Method.Target);
        }
    }
}
