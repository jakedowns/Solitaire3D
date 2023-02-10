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

        if(GUILayout.Button("Fan Deck"))
        {
            myScript.FanCardsOut();
        }
        if(GUILayout.Button("Collect Deck"))
        {
            myScript.CollectCardsToDeck();
        }
        if (GUILayout.Button("ReInitialize"))
        {
            myScript.MyInit();
        }
        if (GUILayout.Button("Randomize"))
        {
            myScript.UIRandomize();
        }

        if (GUILayout.Button("ToggleNrealMode"))
        {
            myScript.ToggleNrealMode();
        }

    }
}
#endif