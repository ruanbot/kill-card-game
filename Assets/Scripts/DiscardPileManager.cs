using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscardPileManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform discardPileUI; // Reference to the discard pile UI panel
    public GameObject cardPrefab; // Prefab for card visuals
    public TMP_Text discardCountText; // Reference to DiscardPanelText

    [Header("Card Display")]
    [Tooltip("Scale of the card in the discard pile (adjust to fit DiscardPlacement)")]
    public float cardScale = 0.3f;

    [Header("Logic")]
    private Stack<GameObject> discardPileStack = new Stack<GameObject>(); // Stack to hold discarded cards

    // Method to add a card to the discard pile
    public void AddToDiscardPile(Card card)
    {
        GameObject cardVisual = Instantiate(cardPrefab, discardPileUI);

        // Position and scale card to fit DiscardPlacement
        RectTransform rt = cardVisual.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * cardScale;

        // Set the card data and show face-up
        DisplayCard displayCard = cardVisual.GetComponent<DisplayCard>();
        if (displayCard != null)
        {
            displayCard.card = card;
            displayCard.cardBack = false; // face-up
            displayCard.UpdateCardInfo();
            displayCard.enabled = false; // Disable Update loop (hold logic, mana color)
        }

        // Disable all interaction scripts — discard pile cards can't be played
        var hover = cardVisual.GetComponentInChildren<CardHover>();
        if (hover != null) hover.enabled = false;
        var hoverHighlight = cardVisual.GetComponent<CardHoverHighlight>();
        if (hoverHighlight != null) hoverHighlight.enabled = false;
        var overlap = cardVisual.GetComponent<CardOverlapHandler>();
        if (overlap != null) overlap.enabled = false;
        var arcDot = cardVisual.GetComponentInChildren<ArcDotControllerUI>();
        if (arcDot != null) arcDot.enabled = false;

        // Push card into the discard pile stack
        discardPileStack.Push(cardVisual);

        // Update visuals
        UpdateTopCardVisual();
        UpdateDiscardCountUI();
    }

    // Update the top card visual in the discard pile
    private void UpdateTopCardVisual()
    {
        // Hide all cards in the stack (not all children — preserves boundary sprite etc.)
        foreach (GameObject cardObj in discardPileStack)
        {
            cardObj.SetActive(false);
        }

        // Show only the top card
        if (discardPileStack.Count > 0)
        {
            discardPileStack.Peek().SetActive(true);
        }
    }

    // Method to retrieve the discard pile for other scripts if needed
    public Stack<GameObject> GetDiscardPile()
    {
        return discardPileStack;
    }

    public void ClearDiscardPileVisuals()
    {
        // Only destroy cards we instantiated, not the boundary sprite or other children
        foreach (GameObject cardObj in discardPileStack)
        {
            if (cardObj != null)
                Destroy(cardObj);
        }
        discardPileStack.Clear();
        UpdateDiscardCountUI();
    }

    private void UpdateDiscardCountUI()
    {
        if (discardCountText != null)
            discardCountText.text = discardPileStack.Count.ToString();
    }

}
