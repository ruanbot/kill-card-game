using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] private GameObject outlineObject; // Reference to the outline child object
    [SerializeField] private Card card;

    private bool _isHolding;

    private void Start()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false); // Ensure outline starts hidden
        }
    }

    private void Update()
    {
        if (_isHolding)
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(true); // Show outline on hover
            }
        }
    }

    public void SetCard(Card cardData)
    {
        card = cardData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(true); // Show outline on hover
        }

        if (card != null && CardManager.Instance != null)
        {
            CardManager.Instance.SetCurrentlyHoveredCard(card);
            if (!CardManager.Instance.IsHighlightLocked)
            {
                CardManager.Instance.HighlightTargets(card);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false); // Hide outline on hover exit
        }

        if (CardManager.Instance != null)
        {
            CardManager.Instance.ClearCurrentlyHoveredCard();
            CardManager.Instance.ClearHighlights();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
         _isHolding = true;
         if (outlineObject != null)
         {
             outlineObject.SetActive(true); // Show outline on hover
         }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHolding = false;
        if (outlineObject != null)
        {
            outlineObject.SetActive(false); // Show outline on hover
        }
    }
}
