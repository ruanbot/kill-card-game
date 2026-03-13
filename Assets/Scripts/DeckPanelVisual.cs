using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual deck panel display. Shows/hides card visuals
/// based on the current deck count, and centers remaining cards.
/// Attach to the DeckPanel GameObject that contains the card children.
/// Disables HorizontalLayoutGroup so we have full control over positioning.
/// </summary>
public class DeckPanelVisual : MonoBehaviour
{
    private PlayerDeck playerDeck;
    private int lastKnownCount = -1;

    private Transform[] cardVisuals;
    private Vector2[] originalPositions;
    private RectTransform panelRect;

    void Start()
    {
        playerDeck = FindFirstObjectByType<PlayerDeck>();
        panelRect = GetComponent<RectTransform>();

        // Disable the HorizontalLayoutGroup — we position cards manually
        var layoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            // Let it do one layout pass first, then disable
            Canvas.ForceUpdateCanvases();
            layoutGroup.enabled = false;
        }

        // Cache child card references and their original positions
        int childCount = transform.childCount;
        cardVisuals = new Transform[childCount];
        originalPositions = new Vector2[childCount];
        for (int i = 0; i < childCount; i++)
        {
            cardVisuals[i] = transform.GetChild(i);
            originalPositions[i] = cardVisuals[i].GetComponent<RectTransform>().anchoredPosition;

            // Disable raycasts on all deck card visuals so clicks pass through
            // to the Draw button underneath. These are display-only, not interactive.
            foreach (var img in cardVisuals[i].GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.raycastTarget = false;
            }
        }
    }

    void Update()
    {
        if (playerDeck == null) return;

        int deckCount = playerDeck.deck.Count;
        if (deckCount == lastKnownCount) return;

        lastKnownCount = deckCount;
        UpdateVisuals(deckCount);
    }

    private void UpdateVisuals(int deckCount)
    {
        int maxCards = cardVisuals.Length;

        // deck 4+ → show 4, deck 3 → show 3, etc.
        int cardsToShow = Mathf.Clamp(deckCount, 0, maxCards);

        // Hide from the top (last children first = highest index)
        for (int i = 0; i < maxCards; i++)
        {
            cardVisuals[i].gameObject.SetActive(i < cardsToShow);
        }

        RepositionCards(cardsToShow);
    }

    private void RepositionCards(int visibleCount)
    {
        if (visibleCount == 0) return;

        // Use original prefab positions as the base
        // Calculate the center of all original visible card positions
        float originalCenterX = 0f;
        float originalCenterY = 0f;
        for (int i = 0; i < visibleCount; i++)
        {
            originalCenterX += originalPositions[i].x;
            originalCenterY += originalPositions[i].y;
        }
        originalCenterX /= visibleCount;
        originalCenterY /= visibleCount;

        // Calculate the center of ALL cards' original positions (the "full" center)
        float fullCenterX = 0f;
        for (int i = 0; i < cardVisuals.Length; i++)
        {
            fullCenterX += originalPositions[i].x;
        }
        fullCenterX /= cardVisuals.Length;

        // Shift visible cards so their center matches the full stack center
        float shiftX = fullCenterX - originalCenterX;

        for (int i = 0; i < visibleCount; i++)
        {
            RectTransform cardRect = cardVisuals[i].GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(
                originalPositions[i].x + shiftX,
                originalPositions[i].y
            );
        }
    }
}
