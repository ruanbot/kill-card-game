using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject outlineObject; // Reference to the outline child object

    private void Start()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false); // Ensure outline starts hidden
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(true); // Show outline on hover
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false); // Hide outline on hover exit
        }
    }
}
