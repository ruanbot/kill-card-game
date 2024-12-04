using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DisplayCard : MonoBehaviour, IPointerClickHandler,IPointerUpHandler,IPointerDownHandler
{
    public Card card;
    public bool isSelected = false;
    private PlayerDeck playerDeck;


    public int id;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI descriptionText;
    public Image artworkImage;
    public TextMeshProUGUI manaText;
    public bool cardBack;

    // public static bool staticCardBack;

    public GameObject Hand;
    public int numberOfCardsInDeck;

    private bool _isHolding;


    // Start is called before the first frame update
    void Awake()
    {
        UpdateCardInfo(); // Initial setup when script awakes
    }

    private void Start()
    {
        playerDeck = Object.FindFirstObjectByType<PlayerDeck>();
    }

    void Update()
    {
        UpdateCardBack();

        if (_isHolding)
        {
            var cardManager = FindFirstObjectByType<CardManager>();
            cardManager.OnCardHold(this);
            
        }
    }

    public void UpdateCardInfo()
    {
        if (card != null)
        {
            cardName.text = card.cardName;
            descriptionText.text = card.cardDescription;
            artworkImage.sprite = card.artwork;
            manaText.text = card.manaCost.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Moved here to down
    }

    private void UpdateCardBack()
    {
        GameObject cardBackObject = transform.Find("CardBack")?.gameObject;
        if (cardBackObject != null)
        {
            cardBackObject.SetActive(cardBack);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.OnCardClicked(card);
        }
        else
        {
            Debug.LogError("CardManager not found in scene.");
        }
        
        _isHolding = true;
        GetComponentInChildren<ArcDotControllerUI>().enabled = true;
        Debug.Log("DOWN");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHolding = false;
        GetComponentInChildren<ArcDotControllerUI>().enabled = false;
        Debug.Log("UP");
        
        var cardManager = FindFirstObjectByType<CardManager>();
        cardManager.DeselectCard();
        
    }
}

