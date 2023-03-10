using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft.Examples
{
    public class Rotation : MonoBehaviour
    {
        [SerializeField] private Vector3 rotation = Vector3.zero;

        bool rotationOn = true;

        // Update is called once per frame
        void Update()
        {
            if (rotationOn)
            {
                transform.Rotate(rotation * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rotationOn = !rotationOn;
            }
        }
    }
}
