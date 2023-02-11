using UnityEngine;
using System.Collections;
using UnityEditor;

using PoweredOn.Managers;

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GameManagerGui : Editor
{
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

        if (GUILayout.Button("Toggle Auto Play"))
        {
            myScript.game.ToggleAutoPlay();
        }

        if (GUILayout.Button("Fan Deck"))
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

        if (GUILayout.Button("Toggle Log"))
        {
            myScript.game.ToggleLog();
        }

    }
}
#endif