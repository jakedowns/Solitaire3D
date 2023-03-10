
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SelectionRedirect : MonoBehaviour
{
    public GameObject redirectToSelf;

    // Update is called once per frame
    void Update()
    {
        if (redirectToSelf == null)
            return;

        #if UNITY_EDITOR
            if (Selection.Contains(redirectToSelf) && Selection.objects.Length == 1)
            {
                Selection.objects = new GameObject[] { gameObject };
            }
#endif
    }
}
