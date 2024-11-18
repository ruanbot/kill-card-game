using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverHeight = 120f;
    public float hoverSpeed = 5f;

    private Vector3 originalLocalPosition;
    private bool isHovered = false;
    private bool isSelected = false;

    private void Start()
    {
        // Store the original local position of the HoverContainer
        originalLocalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void SetHover(bool hoverState)
    {
        isSelected = hoverState;
    }

    private void Update()
    {
        Vector3 targetPosition = originalLocalPosition;

        // If hovered, prioritize hover position; if selected, use selected position
        if (isHovered || isSelected)
        {
            targetPosition += Vector3.up * hoverHeight;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * hoverSpeed);
    }
    // {
    //     if (isHovered || isSelected)
    //     {
    //         // Hover up by modifying the local position of HoverContainer
    //         Vector3 targetPosition = originalLocalPosition + Vector3.up * hoverHeight;
    //         transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * hoverSpeed);
    //     }
    //     else
    //     {
    //         // Move back to the original position
    //         transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * hoverSpeed);
    //     }
    // }
}
