using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;
using static PoweredOn.Managers.DeckManager;
using PoweredOn.Managers;

public class CardStackHitArea : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary> The mesh render. </summary>
    private MeshRenderer m_MeshRender;
    
    // reference to our DeckManager script on our DeckOfCards object
    private DeckManager m_DeckManager;

    public int index;

    [SerializeField]
    public PlayfieldArea playfieldArea;

    public PlayfieldSpot spot;

    void Awake()
    {
        m_MeshRender = transform.GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_DeckManager = GameObject.Find("DeckOfCards").GetComponent<DeckManager>();
        spot = new PlayfieldSpot(playfieldArea, index);
        m_MeshRender.material.color = new Color(0.4f, 0.4f, 0.4f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_DeckManager.OnSingleClickEmptyStack(this.spot);
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