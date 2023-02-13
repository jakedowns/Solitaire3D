using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyEventTester : MonoBehaviour
{
    GameObject cube;
    // Start is called before the first frame update
    void Start()
    {
        cube = GameObject.Find("Cube");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EventTest(){
        Debug.Log("EventTest");
        var mr = cube.GetComponent<MeshRenderer>();
        mr.material.color = mr.material.color == Color.red ? Color.blue : Color.red;
    }
}
