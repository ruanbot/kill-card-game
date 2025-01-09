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
    private BattleVisuals currentHoverTarget;

    // Offset for tooltip placement (adjustable)
    [Header("Tooltip Position Offset")]
    public Vector2 tooltipOffset = new Vector2(-50f, -50f); // Offset from the top-right corner

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvas = FindObjectOfType<Canvas>();
        tooltipPrefab.SetActive(false);
    }

    public void RegisterHoverEvents(BattleVisuals visuals)
    {
        visuals.HoveringOnTarget += OnHoverStart;
        visuals.HoveringOnTargetEnded += OnHoverEnd;
    }

    public void UnregisterHoverEvents(BattleVisuals visuals)
    {
        visuals.HoveringOnTarget -= OnHoverStart;
        visuals.HoveringOnTargetEnded -= OnHoverEnd;
    }

    private void OnHoverStart(BattleVisuals visuals)
    {
        currentHoverTarget = visuals;

        if (visuals.enemyData != null || visuals.memberData != null)
        {
            ShowTooltip(visuals);
        }
    }

    private void OnHoverEnd(BattleVisuals visuals)
    {
        if (currentHoverTarget == visuals)
        {
            currentHoverTarget = null;
            HideTooltip();
        }
    }

    public void ShowTooltip(BattleVisuals visuals)
    {
        // Clear existing tooltip data
        entityNameText.text = string.Empty;
        portraitImage.sprite = null;

        // Populate tooltip using ScriptableObject references
        if (visuals.enemyData != null)
        {
            var enemy = visuals.enemyData;
            entityNameText.text = enemy.EnemyName;
            portraitImage.sprite = enemy.entityPortrait;

            // Populate resistances
            slashResistanceText.text = $"{enemy.resistances.Slash * 100f}%";
            bluntResistanceText.text = $"{enemy.resistances.Blunt * 100f}%";
            rangeResistanceText.text = $"{enemy.resistances.Ranged * 100f}%";
            fireResistanceText.text = $"{enemy.resistances.Fire * 100f}%";
            lightningResistanceText.text = $"{enemy.resistances.Lightning * 100f}%";
            frostResistanceText.text = $"{enemy.resistances.Frost * 100f}%";
            darknessResistanceText.text = $"{enemy.resistances.Dark * 100f}%";
            holyResistanceText.text = $"{enemy.resistances.Holy * 100f}%";
        }
        else if (visuals.memberData != null)
        {
            var member = visuals.memberData;
            entityNameText.text = member.MemberName;
            portraitImage.sprite = member.entityPortrait;

            // Populate resistances
            slashResistanceText.text = $"{member.resistances.Slash * 100f}%";
            bluntResistanceText.text = $"{member.resistances.Blunt * 100f}%";
            rangeResistanceText.text = $"{member.resistances.Ranged * 100f}%";
            fireResistanceText.text = $"{member.resistances.Fire * 100f}%";
            lightningResistanceText.text = $"{member.resistances.Lightning * 100f}%";
            frostResistanceText.text = $"{member.resistances.Frost * 100f}%";
            darknessResistanceText.text = $"{member.resistances.Dark * 100f}%";
            holyResistanceText.text = $"{member.resistances.Holy * 100f}%";
        }

        if (portraitImage.sprite != null)
        {
            portraitImage.preserveAspect = true; // Ensures the image respects aspect ratio
            portraitImage.SetNativeSize();      // Resets the size to the sprite's native size

            // Center the image within its parent
            RectTransform portraitRect = portraitImage.GetComponent<RectTransform>();
            portraitRect.anchoredPosition = Vector2.zero; // Center the image
            portraitRect.localPosition = Vector3.zero;   // Ensure it's at the center in local space

            // Flip the image horizontally
            portraitRect.localScale = new Vector3(-Mathf.Abs(portraitRect.localScale.x), portraitRect.localScale.y, portraitRect.localScale.z);
        }


        // Position the tooltip in the top-right corner
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 topRightPosition = new Vector2(canvasSize.x / 2, canvasSize.y / 2); // Top-right corner in canvas space

        tooltipTransform.anchoredPosition = topRightPosition + tooltipOffset; // Apply offset for spacing
        tooltipPrefab.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPrefab.SetActive(false);
    }
}
