using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BattleVisuals : MonoBehaviour
{
    public event Action<BattleVisuals> TargetSelected; // Public event

    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI levelText;

    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; }
    private int level;
    private Animator anim;

    // A reference to the highlight or outline component
    [SerializeField] private GameObject highlightEffect;

    private const string LEVEL_ABB = "Lvl: ";
    private const string IS_ATTACK_PARAM = "IsAttack";
    private const string IS_HIT_PARAM = "IsHit";
    private const string IS_DEAD_PARAM = "IsDead";

    private void Start()
    {
        anim = GetComponent<Animator>();

        // Ensure highlight is initially disabled
        if (highlightEffect != null) highlightEffect.SetActive(false);
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

    public void PlayAttackAnimation()
    {
        anim.SetTrigger(IS_ATTACK_PARAM);
    }

    public void PlayHitAnimation()
    {
        anim.SetTrigger(IS_HIT_PARAM);
    }

    public void PlayDeathAnimation()
    {
        anim.SetTrigger(IS_DEAD_PARAM);
    }

    public void EnableTargetHighlight()
    {
        if (highlightEffect != null) highlightEffect.SetActive(true);
    }

    public void DisableTargetHighlight()
    {
        if (highlightEffect != null) highlightEffect.SetActive(false);
    }

    private void OnMouseDown()
    {
        TargetSelected?.Invoke(this);
        Debug.Log("TargetSelected invoked for " + gameObject.name);
    }

    public void SubscribeToTargetSelected(Action<BattleVisuals> listener)
    {
        TargetSelected -= listener;
        TargetSelected += listener;
        Debug.Log($"Subscribed {listener.Method.Name} to TargetSelected event on {gameObject.name}");
    }

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
}
