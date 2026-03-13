using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] private GameObject outlineObject; // Reference to the outline child object
    [SerializeField] private Card card;

    #pragma warning disable CS0414
    private bool _isHolding;
    #pragma warning restore CS0414
    private EnergyManager energyManager;

    private void Start()
    {
        energyManager = FindFirstObjectByType<EnergyManager>();
        if (outlineObject != null)
        {
            outlineObject.SetActive(false); // Ensure outline starts hidden
        }
    }

    private void Update()
    {
        // TODO: Re-enable outline when new art is ready
        // Outline disabled for now
        // if (outlineObject != null && card != null && energyManager != null)
        // {
        //     bool canAfford = energyManager.HasEnoughEnergy(card.manaCost);
        //     outlineObject.SetActive(canAfford);
        // }
    }

    public void SetCard(Card cardData)
    {
        card = cardData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Outline is now managed by Update() based on energy, not hover state

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
        // Outline is now managed by Update() based on energy, not hover state

        if (CardManager.Instance != null)
        {
            CardManager.Instance.ClearCurrentlyHoveredCard();
            CardManager.Instance.ClearHighlights();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
         _isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHolding = false;
    }
}
