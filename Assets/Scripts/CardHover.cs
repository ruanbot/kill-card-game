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

    private void Start()
    {
        // Store the original local position of the HoverContainer
        originalLocalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return;
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;
        isHovered = false;
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
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * hoverSpeed);
    }

    public void InitializeHover()
    {
        isInitialized = true;
    }

}
