using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]
public class TextMeshProDeleter : MonoBehaviour
{
    #if UNITY_EDITOR
    // Start is called before the first frame update
    [MenuItem("Debug/Delete 2021 Directories")]
    static void DeleteTextMeshPro()
    {
        string directory = Application.dataPath;

        //Replace "Assets" with "Library"
        directory = directory.Replace("Assets", "Library/PackageCache");

        Debug.Log("directory = " + directory);

        string textMeshProDir = directory + "/com.unity.textmeshpro@3.0.6";
        string visualScriptingDir = directory + "/com.unity.visualscripting@1.7.6";
        string collabDir = directory + "/com.unity.collab-proxy@1.15.16";

        List<string> directoriesToDelete = new List<string>();
        directoriesToDelete.Add(textMeshProDir);
        directoriesToDelete.Add(visualScriptingDir);
        directoriesToDelete.Add(collabDir);

        for (int i = 0; i < directoriesToDelete.Count; i++)
        {
            if (Directory.Exists(directoriesToDelete[i]))
            {
                Debug.Log("Directory " + directoriesToDelete[i] + " exists! Deleting it.");
                Directory.Delete(directoriesToDelete[i],true);
            }
            else
            {
                Debug.Log("Directory " + directoriesToDelete[i] + " not found.");
            }
        }

    }
    #endif
}
