using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Tooltip UI References")]
    public GameObject tooltipPrefab;            // Tooltip prefab
    public RectTransform tooltipTransform;      // Tooltip transform for positioning

    public TMP_Text entityNameText;             // Entity name (Enemy or Party Member)
    public Image portraitImage;                 // Entity portrait

    // Resistance TextMeshPro references
    [Header("Resistance UI References")]
    public TMP_Text slashResistanceText;
    public TMP_Text bluntResistanceText;
    public TMP_Text rangeResistanceText;
    public TMP_Text fireResistanceText;
    public TMP_Text lightningResistanceText;
    public TMP_Text frostResistanceText;
    public TMP_Text darknessResistanceText;
    public TMP_Text holyResistanceText;

    private Canvas canvas;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvas = FindObjectOfType<Canvas>();
        tooltipPrefab.SetActive(false);
    }

    /// <summary>
    /// Show tooltip for a BattleEntities instance.
    /// </summary>
    public void ShowTooltip(BattleEntities entity, Vector3 worldPosition)
    {
        if (entity == null) return;

        PopulateTooltip(
            entity.Name,
            entity.BattleVisuals?.portraitImage?.sprite,
            entity.Resistances,
            worldPosition
        );
    }


    /// <summary>
    /// Populates the tooltip UI with shared data.
    /// </summary>
    private void PopulateTooltip(string entityName, Sprite portrait, DamageResistances resistances, Vector3 worldPosition)
    {
        // Set Entity Name and Portrait
        entityNameText.text = entityName;
        portraitImage.sprite = portrait;

        // Set Resistances
        slashResistanceText.text = $"{resistances.Slash * 100f}%";
        bluntResistanceText.text = $"{resistances.Blunt * 100f}%";
        rangeResistanceText.text = $"{resistances.Ranged * 100f}%";
        fireResistanceText.text = $"{resistances.Fire * 100f}%";
        lightningResistanceText.text = $"{resistances.Lightning * 100f}%";
        frostResistanceText.text = $"{resistances.Frost * 100f}%";
        darknessResistanceText.text = $"{resistances.Dark * 100f}%";
        holyResistanceText.text = $"{resistances.Holy * 100f}%";

        // Show Tooltip
        tooltipPrefab.SetActive(true);

        // Convert world position to screen position
        Vector2 mousePosition = Input.mousePosition;

        // Convert screen position to local position relative to the canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, // TooltipCanvas RectTransform
            mousePosition,   // Screen position
            canvas.worldCamera, // UI Camera
            out Vector2 localPoint
        );

        // Set the tooltip position
        tooltipTransform.localPosition = localPoint + new Vector2(10f, -10f); // Add an offset to avoid overlapping the mouse
    }


    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void HideTooltip()
    {
        tooltipPrefab.SetActive(false);
    }
}
