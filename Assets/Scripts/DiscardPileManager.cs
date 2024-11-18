using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscardPileManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform discardPileUI; // Reference to the discard pile UI panel
    public GameObject cardPrefab; // Prefab for card visuals

    [Header("Logic")]
    private Stack<GameObject> discardPileStack = new Stack<GameObject>(); // Stack to hold discarded cards

    // Method to add a card to the discard pile
    public void AddToDiscardPile(GameObject card)
    {
        // Reset card position and parent it to discard pile UI
        card.transform.SetParent(discardPileUI);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;

        // Push card into the discard pile stack
        discardPileStack.Push(card);

        // Update the top card visual
        UpdateTopCardVisual();
    }

    // Update the top card visual in the discard pile
    private void UpdateTopCardVisual()
    {
        // Hide all cards except the top one
        foreach (Transform card in discardPileUI)
        {
            card.gameObject.SetActive(false);
        }

        // Show the top card if the pile is not empty
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
        foreach (Transform card in discardPileUI)
        {
            Destroy(card.gameObject);
        }
    }

}
