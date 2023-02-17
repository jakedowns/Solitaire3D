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

        // remove rigidbody component too
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
            DestroyImmediate(rigidbody);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
