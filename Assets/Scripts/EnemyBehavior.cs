using System.Collections;
using UnityEngine;
using TMPro;

public class EnemyBehavior : MonoBehaviour
{
    public EnemyAttack[] attacks; // Array of possible attacks
    public float restTime = 2f;   // Time to rest after an attack
    private float attackTimer;
    private int currentAttackIndex = 0;
    private BattleEntities target;
    private EnemyAttackSystem attackSystem;

    private BattleEntities selfEntity; // Reference to this enemy's BattleEntities object
    private bool isResting = false;    // Flag to track resting state

    [SerializeField] private TextMeshProUGUI timerText;

    public void Initialize(BattleEntities entity)
    {
        selfEntity = entity; // Assign the reference
    }

    void Start()
    {
        attackSystem = FindFirstObjectByType<EnemyAttackSystem>();
        if (attackSystem == null)
        {
            Debug.LogError("EnemyAttackSystem not found in the scene!");
            return;
        }

        var battleSystem = FindFirstObjectByType<BattleSystem>();
        if (battleSystem == null || battleSystem.playerBattlers.Count == 0)
        {
            Debug.LogError("BattleSystem not found or playerBattlers is empty!");
            return;
        }
        target = battleSystem.playerBattlers[UnityEngine.Random.Range(0, battleSystem.playerBattlers.Count)];

        if (attacks == null || attacks.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: No attacks assigned to EnemyBehavior!");
            return;
        }

        attackTimer = attacks[currentAttackIndex].cooldown;

        var enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }
        enemyManager.RegisterEnemy(this);
    }

    void Update()
    {
        // ensure enemies dont attack before hand is initialized
        if (!CardManager.Instance.IsHandInitialized())
        {
            return;
        }
        if (target == null || isResting || attacks.Length == 0) return;

        // Countdown the attack timer
        attackTimer -= Time.deltaTime;

        // Update the timer text display
        if (timerText != null)
        {
            timerText.text = attackTimer > 0
                ? Mathf.CeilToInt(attackTimer).ToString()
                : $"{attacks[currentAttackIndex].attackName}"; // Indicate attack phase
            timerText.gameObject.SetActive(true);
        }

        // If timer reaches zero and not resting, trigger the attack
        if (attackTimer <= 0)
        {
            TriggerAttack();
        }
    }

    private void TriggerAttack()
    {
        if (isResting) return; // Prevent duplicate triggers

        isResting = true; // Enter resting state
        attackTimer = float.MaxValue; // Prevent multiple triggers

        // Play attack animation
        if (selfEntity != null)
        {
            selfEntity.BattleVisuals?.PlaySpecificAttackAnimation(attacks[currentAttackIndex].animationTrigger);
        }

        // Debug.Log($"[{gameObject.name}] Triggered attack: {attacks[currentAttackIndex].attackName}");
    }

    //  **This function is now called as an animation event in Unity!**
    public void ApplyDamage()
    {
        if (target != null && attackSystem != null && attacks.Length > 0)
        {
            attackSystem.ExecuteAttack(selfEntity, target, attacks[currentAttackIndex]);
            // Debug.Log($"[{gameObject.name}] Applied damage to {target.Name} using attack: {attacks[currentAttackIndex].attackName}");
        }
    }

    //  **This function is now called as an animation event at the end of the animation!**
    public void StartRestPhase()
    {
        // Debug.Log($"[{gameObject.name}] Animation event triggered! Entering rest phase.");
        StartCoroutine(ApplyRestPhase());
    }

    private IEnumerator ApplyRestPhase()
    {
        // Ensure the enemy is back in idle before resetting
        Animator anim = selfEntity.BattleVisuals.GetComponent<Animator>();
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("skele_idle"));

        if (timerText != null)
        {
            timerText.text = "<color=#ADD8E6>Resting...</color>";
        }

        yield return new WaitForSeconds(restTime);

        // Reset attack timer and state
        attackTimer = attacks[currentAttackIndex].cooldown;
        currentAttackIndex = (currentAttackIndex + 1) % attacks.Length; // Cycle to the next attack
        isResting = false; // Exit resting state
    }

    private void OnDestroy()
    {
        var enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.UnregisterEnemy(this);
        }
    }
}
