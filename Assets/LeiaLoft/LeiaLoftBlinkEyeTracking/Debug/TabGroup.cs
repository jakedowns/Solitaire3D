using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TabGroup : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private GameObject[] tabs;
    [SerializeField] private Sprite unselectedTab;
    [SerializeField] private Sprite selectedTab;
    [SerializeField] private int StartTab;

    private void Start()
    {
        if (Application.isPlaying)
        {
            SetTab(StartTab);
        }
    }

    public void SetTab(int tab)
    {
        int count = buttons.Length;

        for (int i = 0; i < count; i++)
        {
            if (i == tab)
            {
                tabs[i].SetActive(true);
                buttons[i].image.sprite = selectedTab;
            }
            else
            {
                tabs[i].SetActive(false);
                buttons[i].image.sprite = unselectedTab;
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (Selection.Contains(buttons[i].gameObject) && Selection.objects.Length == 1)
                {
                    SetTab(i);
                }
            }
        }
    }
#endif
}
