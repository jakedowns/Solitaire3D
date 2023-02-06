using NRKernal;
using PoweredOn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace PoweredOn.CardBox.PlayingCards
{
    public class MonoPlayingCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {

        public void OnPointerClick(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}
