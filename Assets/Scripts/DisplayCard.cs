using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DisplayCard : MonoBehaviour, IPointerClickHandler
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
        var cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.OnCardClicked(card);
        }
        else
        {
            Debug.LogError("CardManager not found in scene.");
        }
    }

    private void UpdateCardBack()
    {
        GameObject cardBackObject = transform.Find("CardBack")?.gameObject;
        if (cardBackObject != null)
        {
            cardBackObject.SetActive(cardBack);
        }
    }

}

