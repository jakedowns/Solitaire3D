using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PoweredOn.Managers;

public class DebugOutput : MonoBehaviour
{
    private float timer = 0f;

    DeckManager m_DeckManager;
    Text m_TextComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        m_DeckManager = GameObject.Find("DeckOfCards").GetComponent<DeckManager>();
        m_TextComponent = transform.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            // code to be executed once per second
            timer = 0f;
            m_TextComponent.text = m_DeckManager.GetDebugText();
        }
    }
}
