using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RemoveOldSpringJoints : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // remove any and all existing spring joints that may be left over
        // from a previous game
        SpringJoint[] springJoints = GetComponents<SpringJoint>();
        foreach (SpringJoint springJoint in springJoints)
        {
            DestroyImmediate(springJoint);
        }

        foreach(ConfigurableJoint configurableJoint in GetComponents<ConfigurableJoint>()){
            DestroyImmediate(configurableJoint);
        }

        // remove any EXTRA rigidbody components
        Rigidbody[] rigidbodies = GetComponents<Rigidbody>();
        if (rigidbodies.Length > 1)
        {
            for (int i = 1; i < rigidbodies.Length; i++)
            {
                DestroyImmediate(rigidbodies[i]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
