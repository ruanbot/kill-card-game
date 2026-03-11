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

    private BattleEntities selfEntity; // Reference to this enemy's BattleEntities object
    private bool isResting = false;    // Flag to track resting state

    private CombatActionQueue actionQueue;

    [SerializeField] private TextMeshProUGUI timerText;

    public void Initialize(BattleEntities entity)
    {
        selfEntity = entity; // Assign the reference
    }

    void Start()
    {
        var battleSystem = FindFirstObjectByType<BattleSystem>();
        if (battleSystem == null || battleSystem.playerBattlers.Count == 0)
        {
            Debug.LogError("BattleSystem not found or playerBattlers is empty!");
            return;
        }
        target = battleSystem.playerBattlers[UnityEngine.Random.Range(0, battleSystem.playerBattlers.Count)];
        actionQueue = battleSystem.ActionQueue;

        if (attacks == null || attacks.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: No attacks assigned to EnemyBehavior!");
            return;
        }

        attackTimer = attacks[currentAttackIndex].cooldown;

        var enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.RegisterEnemy(this);
        }
    }

    void Update()
    {
        // Ensure enemies don't attack before hand is initialized
        if (!CardManager.Instance.IsHandInitialized())
            return;

        // Pause timer while queue is processing
        if (actionQueue != null && actionQueue.IsProcessing)
            return;

        if (target == null || isResting || attacks.Length == 0) return;

        // Countdown the attack timer
        attackTimer -= Time.deltaTime;

        // Update the timer text display
        if (timerText != null)
        {
            timerText.text = attackTimer > 0
                ? Mathf.CeilToInt(attackTimer).ToString()
                : $"{attacks[currentAttackIndex].attackName}";
            timerText.gameObject.SetActive(true);
        }

        // If timer reaches zero, enqueue the attack
        if (attackTimer <= 0)
        {
            TriggerAttack();
        }
    }

    private void TriggerAttack()
    {
        if (isResting) return;

        isResting = true;
        attackTimer = float.MaxValue;

        var attack = attacks[currentAttackIndex];
        int adjustedDamage = attack.CalculateDamage(selfEntity);

        actionQueue.Enqueue(new CombatAction
        {
            description = $"{selfEntity.Name} uses {attack.attackName}",
            enqueuedTime = Time.time,
            isPlayerAction = false,
            execute = (done) =>
            {
                // Play attack animation
                selfEntity.BattleVisuals?.PlaySpecificAttackAnimation(attack.animationTrigger, () =>
                {
                    // Apply damage and react
                    ApplyDamageAndReact(attack, adjustedDamage, done);
                });
            }
        });
    }

    private void ApplyDamageAndReact(EnemyAttack attack, int adjustedDamage, System.Action done)
    {
        if (target == null)
        {
            done();
            return;
        }

        int actualDamage = target.TakeDamage(adjustedDamage, attack.damageType);
        target.BattleVisuals?.ShowPopup(actualDamage, true);
        attack.ApplySpecialEffects(target);

        Debug.Log($"{attack.attackName}: Dealt {actualDamage} {attack.damageType} damage to {target.Name}");

        if (!target.IsAlive)
        {
            target.BattleVisuals?.PlayDeathAnimation(() =>
            {
                var battleSystem = FindFirstObjectByType<BattleSystem>();
                battleSystem?.RemoveDeadEntity(target);
                if (target.BattleVisuals != null)
                    Destroy(target.BattleVisuals.gameObject, 1f);
                StartRestPhase();
                done();
            });
        }
        else
        {
            target.BattleVisuals?.PlayHitAnimation(() =>
            {
                StartRestPhase();
                done();
            });
        }
    }

    // Kept as no-op for animation event compatibility
    public void ApplyDamage() { }

    public void StartRestPhase()
    {
        StartCoroutine(ApplyRestPhase());
    }

    private IEnumerator ApplyRestPhase()
    {
        if (timerText != null)
        {
            timerText.text = "<color=#ADD8E6>Resting...</color>";
        }

        yield return new WaitForSeconds(restTime);

        // Reset attack timer and state
        attackTimer = attacks[currentAttackIndex].cooldown;
        currentAttackIndex = (currentAttackIndex + 1) % attacks.Length;
        isResting = false;
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
