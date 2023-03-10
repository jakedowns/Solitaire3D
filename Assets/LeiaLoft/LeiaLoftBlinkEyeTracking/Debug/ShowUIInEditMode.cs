using UnityEngine;

[ExecuteInEditMode]
public class ShowUIInEditMode : MonoBehaviour
{
    private void Start()
    {
        if (Application.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }
    
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            gameObject.SetActive(true);
        }
    }
}
