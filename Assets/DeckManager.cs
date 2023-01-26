using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PoweredOn.Objects;

public class DeckManager : MonoBehaviour
{

    public CardInteractive cardInteractive;
    // Start is called before the first frame update
    void Start()
    {
        
        for (int i = 0; i < transform.childCount; i++)
        {
            cardInteractive = new CardInteractive();
            Transform child = transform.GetChild(i);
            Debug.Log("child? " + child.gameObject.name);
            Debug.Log("adding " + cardInteractive.GetType());
            child.gameObject.AddComponent(cardInteractive.GetType());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
