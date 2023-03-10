using UnityEngine;

public class ExpandingSettingBox : MonoBehaviour
{
    bool expanded = false;
    RectTransform rect;
    Vector2 initialSize;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        initialSize = rect.sizeDelta;
    }

    void Update()
    {
        if (expanded)
        {
            rect.sizeDelta = Vector2.Lerp(
                rect.sizeDelta,
                new Vector2(initialSize.x, initialSize.y * 2f),
                Mathf.Clamp(Time.deltaTime * 5f, 0f, 1f)
            ); ;
        }
        else
        {
            rect.sizeDelta = Vector2.Lerp(
                rect.sizeDelta,
                new Vector2(initialSize.x, initialSize.y),
                Mathf.Clamp(Time.deltaTime * 5f, 0f, 1f)
            );
        }
    }

    public void ToggleExpand()
    {
        expanded = !expanded;
    }

    public void SetExpanded(bool expanded)
    {
        this.expanded = expanded;
    }
}
