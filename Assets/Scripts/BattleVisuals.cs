using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Reflection;

public class BattleVisuals : MonoBehaviour
{
    public event Action<BattleVisuals> TargetSelected; // Public event
    public event Action<BattleVisuals> HoveringOnTarget; //Public Event

    public event Action<BattleVisuals> HoveringOnTargetEnded; //Public Event

    [SerializeField] public Slider healthBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject floatingText;
    [SerializeField] public Image portraitImage;

    public EnemyInfo enemyData;
    public PartyMemberInfo memberData;

    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; }
    private int level;
    private Animator anim;


    // A reference to the highlight or outline component
    [SerializeField] private GameObject highlightObject;

    private const string LEVEL_ABB = "Lvl: ";
    private const string IS_ATTACK_PARAM = "IsAttack";
    private const string IS_HIT_PARAM = "IsHit";
    private const string IS_DEAD_PARAM = "IsDead";

    private void Start()
    {
        anim = GetComponent<Animator>();

    }

    private void SetFromEnemyData()
    {
        levelText.text = LEVEL_ABB + enemyData.Level;
        portraitImage.sprite = enemyData.entityPortrait;

        SetHealthValues(enemyData.CurrentHealth, enemyData.MaxHealth);
    }

    private void SetFromPartyMemberData()
    {
        levelText.text = LEVEL_ABB + memberData.StartingLevel;
        portraitImage.sprite = memberData.entityPortrait;

        SetHealthValues(memberData.BaseHealth, memberData.BaseHealth);
    }

    public void SetStartingValues(int startHealth, int startMaxHealth, int startLevel)
    {
        currentHealth = startHealth;
        maxHealth = startMaxHealth;
        level = startLevel;
        levelText.text = LEVEL_ABB + level;
        UpdateHPDisplay();
    }

    public void SetHealthValues(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        UpdateHPDisplay();
    }


    public void ChangeHealth(int healthChange)
    {
        currentHealth += healthChange;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health stays within valid range
        if (currentHealth <= 0)
        {
            PlayDeathAnimation();
            Destroy(gameObject, 1f); // Delay to allow death animation
        }
        UpdateHPDisplay();
    }

    public void UpdateHPDisplay()
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHealth}/{maxHealth}";
            Debug.Log($"{gameObject.name} HP display updated: {hpText.text}");
        }
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void ShowPopup(int amount, bool isDamage)
    {
        // Offset the popup position slightly above the entity
        Vector3 popupPosition = transform.position + new Vector3(0, 0, -0.1f);

        GameObject popup = Instantiate(floatingText, popupPosition, Quaternion.identity);

        // Set the popup's parent to the battle entity but keep the world position
        popup.transform.SetParent(transform, worldPositionStays: true);

        popup.GetComponent<Renderer>().sortingLayerName = "UI"; // Adjust Sorting Layer Name
        popup.GetComponent<Renderer>().sortingOrder = 10; // Ensure a high sorting order

        var textComponent = popup.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            // Set the text and color based on whether it's damage or healing
            textComponent.text = amount.ToString();
            textComponent.color = isDamage ? Color.yellow : Color.green; // Yellow for damage, green for heal
        }


    }

    public void InitializeWithEnemy(EnemyInfo info)
    {
        enemyData = info;
    }

    public void InitializeWithPartyMember(PartyMemberInfo info)
    {
        memberData = info;
    }


    public void PlayAttackAnimation()
    {
        anim.SetTrigger(IS_ATTACK_PARAM);
    }

    public void PlaySpecificAttackAnimation(string animationTrigger)
    {
        if (!string.IsNullOrEmpty(animationTrigger))
        {
            anim.SetTrigger(animationTrigger);
            StartCoroutine(WaitForAnimationToEnd(animationTrigger));
        }
    }

    private IEnumerator WaitForAnimationToEnd(string animationTrigger)
    {
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
        anim.ResetTrigger(animationTrigger); // Not strictly necessary for triggers, but safe
    }


    public void PlayHitAnimation()
    {
        CardManager.Instance.LockHighlights(true);
        anim.SetTrigger(IS_HIT_PARAM);
        StartCoroutine(UnlockHighlightAfterAnimation());
    }

    private IEnumerator UnlockHighlightAfterAnimation()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        CardManager.Instance.LockHighlights(false);

        // Retrigger highlights if a card is currently hovered
        if (CardManager.Instance.currentlyHoveredCard != null)
        {
            CardManager.Instance.HighlightTargets(CardManager.Instance.currentlyHoveredCard);
        }
    }

    public void PlayDeathAnimation()
    {
        anim.SetTrigger(IS_DEAD_PARAM);
    }

    private void OnMouseEnter()
    {

        // Enable the Highlight child object
        if (highlightObject != null)
        {
            highlightObject.SetActive(true);
        }
        //// New: Show resistances from BattleEntities
        //if (linkedEntity != null)
        //{
        //    TooltipManager.Instance.ShowTooltip(linkedEntity, transform.position);
        //}

    }

    private void OnMouseExit()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false); // Ensure the outline is off initially
        }

        TooltipManager.Instance.HideTooltip();

        HoveringOnTargetEnded?.Invoke(this);
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SelectTarget();
        }

        OnTargetHover();

        // Debug.Log("MOUSEOVER");
    }

    public void SubscribeToTargetSelected(Action<BattleVisuals> listener)
    {
        TargetSelected -= listener;
        TargetSelected += listener;
        // Debug.Log($"Subscribed {listener.Method.Name} to TargetSelected event on {gameObject.name}");
    }

    public void SubscribeToTargetHovering(Action<BattleVisuals> listener)
    {
        HoveringOnTarget -= listener;
        HoveringOnTarget += listener;
        // Debug.Log($"Subscribed {listener.Method.Name} to HoveringOnTarget event on {gameObject.name}");
    }

    public void SubscribeToTargetHoveringEnded(Action<BattleVisuals> listener)
    {
        HoveringOnTargetEnded -= listener;
        HoveringOnTargetEnded += listener;
        // Debug.Log($"Subscribed {listener.Method.Name} to HoveringOnTargetEnded event on {gameObject.name}");
    }


    //Disket added this
    private void SelectTarget()
    {
        TargetSelected?.Invoke(this);
        Debug.Log("TargetSelected invoked for " + gameObject.name);
    }

    private void OnTargetHover()
    {
        HoveringOnTarget?.Invoke(this);
    }
    //

    public void DestroySelf()
    {
        Destroy(gameObject); // Destroys the GameObject this script is attached to
    }

    public void SyncWithEntity(BattleEntities entity)
    {
        currentHealth = entity.CurrentHealth;
        maxHealth = entity.MaxHealth;
        UpdateHPDisplay();
    }

    public void SetHighlight(bool isActive)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isActive);
        }
    }

    public BattleEntities LinkedEntity => linkedEntity;

    private BattleEntities linkedEntity;

    public void LinkToEntity(BattleEntities entity)
    {
        linkedEntity = entity;
        TooltipManager.Instance.RegisterHoverEvents(this);
    }

    private void OnDestroy()
    {
        TooltipManager.Instance.UnregisterHoverEvents(this);
    }




}
