using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardOverlapHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int originalSiblingIndex;
    private Transform handTransform;
    private HorizontalLayoutGroup layoutGroup;
    private bool isInitialized = false;

    private void Start()
    {
        handTransform = transform.parent;
        layoutGroup = handTransform.GetComponent<HorizontalLayoutGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return;

        if (handTransform == null) return;

        originalSiblingIndex = transform.GetSiblingIndex();

        if (layoutGroup != null)
            layoutGroup.enabled = false;

        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;

        if (handTransform == null) return;

        transform.SetSiblingIndex(originalSiblingIndex);

        if (layoutGroup != null)
            layoutGroup.enabled = true;
    }


    public void InitializeOverlap()
    {
        isInitialized = true;
    }
}
