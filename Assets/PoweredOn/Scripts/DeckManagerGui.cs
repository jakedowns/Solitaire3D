using UnityEngine;
using System.Collections;
using UnityEditor;

using PoweredOn.Managers;

[CustomEditor(typeof(DeckManager))]
public class DeckManagerGui : Editor
{
    public override void OnInspectorGUI()
    {
        DeckManager myScript = (DeckManager)target;

        if (GUILayout.Button("Run Tests"))
        {
            myScript.RunTests();
        }
    }
}