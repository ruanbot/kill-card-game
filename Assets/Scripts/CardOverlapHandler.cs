using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardOverlapHandler : MonoBehaviour
{
    [Tooltip("How many pixels neighboring cards slide away from the hovered card")]
    public float spreadDistance = 80f;
    [Tooltip("How fast neighbors lerp to their spread position")]
    public float spreadSpeed = 10f;

    private int originalSiblingIndex;
    private int baseSortingOrder; // default sorting order based on hand position
    private Transform handTransform;
    private RectTransform rectTransform;
    private bool isInitialized = false;
    private bool isHovered = false;

    private Canvas overrideCanvas;
    private GraphicRaycaster overrideRaycaster;

    // Static reference so only one card is hovered at a time
    private static CardOverlapHandler currentlyHovered;

    // True layout positions — captured from anchoredPosition for consistency with UpdateHandSpacing
    private Vector2 restAnchoredPos;
    private bool hasRestPosition = false;

    // Set by CardManager during draw animation — disables all hover/spread/restPosition logic
    [HideInInspector] public bool isAnimating = false;

    // Live target position for the draw animation. Updated by UpdateHandSpacing so that
    // if the hand is repositioned while this card is still animating in, it lands in the
    // correct slot instead of the stale position captured when the animation started.
    [HideInInspector] public Vector2 animTargetPos;

    // Whether the hand is in "spread mode" (any card hovered)
    private static bool isSpreadActive = false;

    private void Start()
    {
        handTransform = transform.parent;
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Reset static state. Call when re-initializing the hand (e.g. new battle).
    /// </summary>
    public static void ResetStaticState()
    {
        currentlyHovered = null;
        isSpreadActive = false;
    }

    private void LateUpdate()
    {
        if (!isInitialized || isAnimating || rectTransform == null) return;

        // If nobody is hovered, continuously track our true layout position
        if (currentlyHovered == null)
        {
            restAnchoredPos = rectTransform.anchoredPosition;
            hasRestPosition = true;
            return;
        }

        if (!hasRestPosition) return;

        if (currentlyHovered == this)
        {
            // The hovered card stays at its rest position (CardHover handles the lift via HoverContainer child)
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, restAnchoredPos, Time.deltaTime * spreadSpeed);
        }
        else
        {
            // We are a neighbor — calculate spread offset from our TRUE rest position
            int hoveredIndex = currentlyHovered.originalSiblingIndex;
            int myIndex = originalSiblingIndex;
            int cardCount = handTransform.childCount;

            // Dynamic spread: small with few cards, larger with many cards
            // 5 cards → ~40px, 7 cards → ~60px, 10+ cards → 80px
            float dynamicSpread = Mathf.Lerp(spreadDistance * 0.5f, spreadDistance, Mathf.InverseLerp(5f, 10f, cardCount));

            Vector2 target = restAnchoredPos;
            if (myIndex < hoveredIndex)
            {
                // Left neighbors push further (1.4x) so they clear the hovered card
                target.x = restAnchoredPos.x - dynamicSpread * 1.4f;
            }
            else if (myIndex > hoveredIndex)
            {
                target.x = restAnchoredPos.x + dynamicSpread;
            }

            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, target, Time.deltaTime * spreadSpeed);
        }
    }

    /// <summary>
    /// Called by CardHover when pointer enters. Do NOT implement IPointerEnterHandler
    /// on this class — Unity sends enter events to all ancestors, which would cause
    /// double processing since CardHover already forwards the event.
    /// </summary>
    public void HandlePointerEnter(PointerEventData eventData)
    {
        if (!isInitialized || isAnimating) return;
        if (handTransform == null) return;

        // If switching from another hovered card, just hand off — don't re-snapshot
        if (currentlyHovered != null && currentlyHovered != this)
        {
            // Reset the old hovered card's canvas sorting back to its base
            if (currentlyHovered.overrideCanvas != null)
                currentlyHovered.overrideCanvas.sortingOrder = currentlyHovered.baseSortingOrder;
            currentlyHovered.isHovered = false;

            // Force reset the old card's CardHover so it drops back down
            var oldCardHover = currentlyHovered.GetComponentInChildren<CardHover>();
            if (oldCardHover != null)
                oldCardHover.ForceResetHover();
        }

        // First time entering hover (from unhovered state) — snapshot positions
        if (!isSpreadActive)
        {
            SnapshotAllRestPositions();
            isSpreadActive = true;
        }

        // Bring this card to front via canvas sorting (above all base orders)
        if (overrideCanvas != null)
            overrideCanvas.sortingOrder = 100;

        isHovered = true;
        currentlyHovered = this;
    }

    /// <summary>
    /// Called by CardHover when pointer exits.
    /// </summary>
    public void HandlePointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;
        if (handTransform == null) return;

        isHovered = false;

        // If another card already took over via HandlePointerEnter (which fires first),
        // it already cleaned us up — don't touch anything
        if (currentlyHovered != null && currentlyHovered != this)
            return;

        // Restore canvas sorting to base order
        if (overrideCanvas != null)
            overrideCanvas.sortingOrder = baseSortingOrder;

        currentlyHovered = null;
        Invoke(nameof(TryRestoreLayout), 0f);
    }

    /// <summary>
    /// Called at end of frame after OnPointerExit. If no new card was hovered,
    /// re-enable the layout group so cards return to their natural positions.
    /// </summary>
    private void TryRestoreLayout()
    {
        if (currentlyHovered != null) return;

        isSpreadActive = false;

        // Restore cards to their rest positions (set by UpdateHandSpacing)
        if (handTransform != null)
        {
            foreach (Transform child in handTransform)
            {
                var handler = child.GetComponent<CardOverlapHandler>();
                if (handler != null && handler.hasRestPosition && handler.rectTransform != null)
                {
                    handler.rectTransform.anchoredPosition = handler.restAnchoredPos;
                }
            }
        }
    }

    /// <summary>
    /// Snapshot the current layout positions of all cards. Only called once
    /// when entering hover from an unhovered state.
    /// Skips cards that are still animating (e.g. dealing from deck).
    /// </summary>
    private void SnapshotAllRestPositions()
    {
        if (handTransform == null) return;

        foreach (Transform child in handTransform)
        {
            var handler = child.GetComponent<CardOverlapHandler>();
            if (handler != null && !handler.isAnimating)
            {
                var rt = child.GetComponent<RectTransform>();
                if (rt != null)
                    handler.restAnchoredPos = rt.anchoredPosition;
                handler.hasRestPosition = true;
                handler.originalSiblingIndex = child.GetSiblingIndex();
            }
        }
    }

    /// <summary>
    /// Called by CardManager after UpdateHandSpacing to keep rest positions in sync.
    /// Always updates — even during hover — because the hand layout changed
    /// (card added/removed) and the old rest positions are now wrong.
    /// </summary>
    public static void SyncRestPositions(Transform handTransform)
    {
        if (handTransform == null) return;

        foreach (Transform child in handTransform)
        {
            var handler = child.GetComponent<CardOverlapHandler>();
            if (handler != null && !handler.isAnimating)
            {
                var rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    handler.restAnchoredPos = rt.anchoredPosition;
                    handler.hasRestPosition = true;
                }
                handler.originalSiblingIndex = child.GetSiblingIndex();
            }
        }
    }

    /// <summary>
    /// Assigns base sorting orders so left cards render on top of right cards
    /// (like a hand of cards fanned to the right). Call after cards are added to hand.
    /// </summary>
    public static void RefreshBaseSortingOrders(Transform handTransform)
    {
        int childCount = handTransform.childCount;
        foreach (Transform child in handTransform)
        {
            var handler = child.GetComponent<CardOverlapHandler>();
            if (handler != null && handler.overrideCanvas != null)
            {
                // Left cards get higher order so they render on top of right cards
                int index = child.GetSiblingIndex();
                handler.baseSortingOrder = childCount - index;
                handler.overrideCanvas.sortingOrder = handler.baseSortingOrder;
            }
        }
    }

    private void OnDestroy()
    {
        // If this card is destroyed while hovered (e.g. played from hand),
        // clean up the static state so remaining cards re-collapse
        if (currentlyHovered == this)
        {
            currentlyHovered = null;
            isSpreadActive = false;
        }
        // If this was the last card, ensure static state is clean
        if (handTransform != null && handTransform.childCount <= 1)
        {
            currentlyHovered = null;
            isSpreadActive = false;
        }
    }

    public void InitializeOverlap()
    {
        isInitialized = true;
        enabled = true; // Component starts disabled on the prefab; enable it now

        // Set up Canvas override for per-card sorting order control
        // Done here (not Start) so it's ready before RefreshBaseSortingOrders is called
        // Canvas may already exist if InstantiateCardVisual added it for early sorting
        overrideCanvas = gameObject.GetComponent<Canvas>();
        if (overrideCanvas == null)
        {
            overrideCanvas = gameObject.AddComponent<Canvas>();
            overrideCanvas.overrideSorting = true;
            overrideCanvas.sortingOrder = 0;

            // TMP's SDF shader needs these channels for glow, outline, and underlay.
            // A runtime-added Canvas doesn't have them enabled by default (Unity Case #1337742).
            overrideCanvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            overrideCanvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;
            overrideCanvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
        }

        overrideRaycaster = gameObject.GetComponent<GraphicRaycaster>();
        if (overrideRaycaster == null)
        {
            overrideRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        // Force TMP to rebuild meshes with the correct shader channel data
        foreach (var tmp in GetComponentsInChildren<TMPro.TMP_Text>())
        {
            tmp.ForceMeshUpdate(true);
        }
    }
}
