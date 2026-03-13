using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverHeight = 120f;
    public float hoverSpeed = 5f;

    private Vector3 originalLocalPosition;
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isInitialized = false;

    public Transform hoverTarget;

    private void Start()
    {
        // Store the original local position of the HoverContainer
        originalLocalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return;
        isHovered = true;

        // Forward to root CardOverlapHandler via explicit method call.
        // Do NOT use OnPointerEnter — CardOverlapHandler no longer implements the
        // interface to avoid double events (Unity sends enter to all ancestors).
        var overlap = GetComponentInParent<CardOverlapHandler>();
        if (overlap != null)
            overlap.HandlePointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;
        isHovered = false;

        var overlap = GetComponentInParent<CardOverlapHandler>();
        if (overlap != null)
            overlap.HandlePointerExit(eventData);
    }


    public void SetHover(bool hoverState)
    {
        if (!isInitialized) return;
        isSelected = hoverState;
    }

    private void Update()
    {
        if (!isInitialized) return;

        Vector3 targetPosition = originalLocalPosition;

        // If hovered, prioritize hover position; if selected, use selected position
        if (isHovered || isSelected)
        {
            targetPosition += Vector3.up * hoverHeight;
            // targetPosition += -Vector3.right * hoverHeight;
        }

        hoverTarget.localPosition = Vector3.Lerp(hoverTarget.localPosition, targetPosition, Time.deltaTime * hoverSpeed);

        // Snap to target when close enough to prevent cards getting stuck partially raised
        if (Vector3.Distance(hoverTarget.localPosition, targetPosition) < 0.5f)
        {
            hoverTarget.localPosition = targetPosition;
        }
    }

    public void ForceResetHover()
    {
        isHovered = false;
        isSelected = false;
    }

    public void InitializeHover()
    {
        isInitialized = true;
    }

}
