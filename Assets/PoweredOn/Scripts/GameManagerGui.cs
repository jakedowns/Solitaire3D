using UnityEngine;
using System.Collections;
using UnityEditor;

using PoweredOn.Managers;

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GameManagerGui : Editor
{
    private bool autoplaying = false;
    public override void OnInspectorGUI()
    {
        GameManager myScript = (GameManager)target;

        if (GUILayout.Button("Run Tests"))
        {
            myScript.RunTests();
        }

        if (GUILayout.Button("Auto Play Next Move"))
        {
            myScript.AutoPlayNextMove();
        }

        if (!autoplaying)
        {   
            if (GUILayout.Button("Auto Play"))
            {
                myScript.StartAutoPlay();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Auto Play"))
            {
                myScript.StopAutoPlay();
            }
        }

    }
}
#endif