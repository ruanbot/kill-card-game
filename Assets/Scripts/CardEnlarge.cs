using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CardEnlarge : MonoBehaviour, IPointerClickHandler
{
    [Header("Enlarged Settings")]
    public Vector2 enlargedScale = new Vector2(2f, 2f); // Scale factor when enlarged
    public float animationSpeed = 5f; // Speed of the enlargement animation

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector2 enlargedPosition;
    private bool isEnlarged = false;
    private HorizontalLayoutGroup handLayoutGroup;

    private void Start()
    {
        // Store the RectTransform and original settings
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // Set the center position of the Canvas as the enlarged target position
        enlargedPosition = Vector2.zero;

        // Find the Horizontal Layout Group on the Hand panel
        handLayoutGroup = originalParent.GetComponent<HorizontalLayoutGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEnlarged)
        {
            ShrinkCard();
        }
        else
        {
            EnlargeCard();
        }
    }

    private void Update()
    {
        // If the card is enlarged, detect any click to shrink it back
        if (isEnlarged && Input.GetMouseButtonDown(0))
        {
            ShrinkCard();
        }
    }

    private void EnlargeCard()
    {
        isEnlarged = true;

        // Disable layout group to allow the card to move freely
        if (handLayoutGroup != null) handLayoutGroup.enabled = false;

        // Move the card to the root Canvas
        transform.SetParent(GameObject.Find("Canvas").transform, true);

        // Animate to the center position and enlarged scale
        StartCoroutine(AnimateCardPosition(enlargedPosition, enlargedScale));
    }

    private void ShrinkCard()
    {
        isEnlarged = false;

        // Move the card back to its original parent in the Hand panel
        transform.SetParent(originalParent, true);

        // Animate back to the original position and scale
        StartCoroutine(AnimateCardPosition(originalPosition, originalScale));

        // Re-enable the layout group after shrinking
        if (handLayoutGroup != null) handLayoutGroup.enabled = true;
    }

    private IEnumerator AnimateCardPosition(Vector2 targetPosition, Vector3 targetScale)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.01f ||
               Vector3.Distance(rectTransform.localScale, targetScale) > 0.01f)
        {
            // Smoothly interpolate position and scale in Canvas space
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, animationSpeed * Time.deltaTime);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, animationSpeed * Time.deltaTime);

            yield return null;
        }

        // Ensure the final position and scale are exact
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localScale = targetScale;
    }
}
