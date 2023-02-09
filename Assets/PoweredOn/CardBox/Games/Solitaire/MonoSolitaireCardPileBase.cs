using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoweredOn.CardBox.PlayingCards;
using UnityEngine.EventSystems;
using UnityEngine;
using PoweredOn.Managers;

namespace PoweredOn.CardBox.Games.Solitaire
{
    // SolitairePlayingCardPileBase ?
    public class MonoSolitaireCardPileBase: MonoPlayingCardPileBase, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private PlayingCardPile playingCardPile;

        /// <summary> The mesh render. </summary>
        private MeshRenderer m_MeshRender;

        public int index;

        public PlayfieldArea playfieldArea;

        public PlayfieldSpot spot;
        void Awake()
        {
            m_MeshRender = transform.GetComponent<MeshRenderer>();
        }

        // Start is called before the first frame update
        void Start()
        {
            spot = new PlayfieldSpot(playfieldArea, index);
            m_MeshRender.material.color = new Color(0.4f, 0.4f, 0.4f);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameManager.Instance.game.OnSingleClickCardPileBase(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_MeshRender.material.color = Color.green;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_MeshRender.material.color = new Color(0.4f, 0.4f, 0.4f);
        }
    }
}
