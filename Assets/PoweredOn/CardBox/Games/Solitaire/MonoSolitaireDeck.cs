using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class MonoSolitaireDeck
    {
        public GameObject gameObject;
        public MonoSolitaireDeck()
        {
            gameObject = GameObject.Find("DeckOfCards");
        }
        
    }
}
