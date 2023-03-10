using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string nextScene;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void LoadNextScene()
    {
        if (nextScene == "")
        {
            nextScene = GetNextSceneName();
        }
        SceneManager.LoadScene(nextScene);
    }

    public void LoadNextSceneAsync()
    {
        if (nextScene == "")
        {
            nextScene = GetNextSceneName();
        }
        SceneManager.LoadSceneAsync(nextScene);
    }

    string GetNextSceneName()
    {
        Scene scene = SceneManager.GetActiveScene();
        int nextSceneNumber = int.Parse(scene.name.Replace("Scene ", ""));
        string nextSceneName = "Scene " + (nextSceneNumber + 1);
        return nextSceneName;
    }
}
