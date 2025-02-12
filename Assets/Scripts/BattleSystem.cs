using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] public List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] public List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] public List<BattleEntities> playerBattlers = new List<BattleEntities>();

    private PartyManager partyManager;
    [SerializeField] private EnemyManager enemyManager;

    public EnemyManager GetEnemyManager() => enemyManager;

    private Queue<System.Action> attackQueue = new Queue<System.Action>();
    private bool isProcessing = false;

    private bool canEnemiesAttack = false;

    public void StartEnemyAttacks()
    {
        canEnemiesAttack = true;
        Debug.Log("Hand initialized! Enemies can now attack.");
    }

    public void EnqueueAttack(System.Action attack)
    {
        if (!canEnemiesAttack)
        {
            Debug.Log("Enemies cannot attack yet! Waiting for hand initialization.");
            return;
        }

        attackQueue.Enqueue(attack);
        if (!isProcessing)
        {
            StartCoroutine(ProcessQueue());
        }
    }


    private IEnumerator ProcessQueue()
    {
        isProcessing = true;

        while (attackQueue.Count > 0)
        {
            var attack = attackQueue.Dequeue();
            attack.Invoke();
            yield return new WaitForSeconds(1f); // Delay between attacks
        }

        isProcessing = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
    }

private void CreateEnemyEntities()
{
    List<Enemy> currentEnemies = enemyManager.GetCurrentEnemies();

    for (int i = 0; i < currentEnemies.Count; i++)
    {
        BattleEntities tempEntity = new BattleEntities();

        tempEntity.SetEntityValues(
            currentEnemies[i].EnemyName,
            currentEnemies[i].CurrentHealth,
            currentEnemies[i].MaxHealth,
            currentEnemies[i].Initiative,
            currentEnemies[i].Strength,
            currentEnemies[i].Level,
            false,
            EntityType.Enemy,
            currentEnemies[i].Resistances
        );

        // Instantiate the visual prefab at the designated spawn point
        if (currentEnemies[i].EnemyVisualPrefab != null)
        {
            GameObject enemyObject = Instantiate(
                currentEnemies[i].EnemyVisualPrefab,
                enemySpawnPoints[i].position,
                Quaternion.identity
            );

            // Get the BattleVisuals component from the instantiated object
            BattleVisuals tempBattleVisuals = enemyObject.GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(
                currentEnemies[i].CurrentHealth,
                currentEnemies[i].MaxHealth,
                currentEnemies[i].Level
            );

            // Link the visual back to the BattleEntities object
            tempEntity.BattleVisuals = tempBattleVisuals;

            // Link the BattleEntities object to the BattleVisuals
            tempBattleVisuals.LinkToEntity(tempEntity);

            // Pass the BattleEntities reference to the EnemyBehavior script
            var enemyBehavior = enemyObject.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null)
            {
                enemyBehavior.Initialize(tempEntity);
            }
        }
        else
        {
            Debug.LogWarning($"EnemyVisualPrefab is not assigned for {currentEnemies[i].EnemyName} in EnemyInfo.");
        }

        allBattlers.Add(tempEntity);
        enemyBattlers.Add(tempEntity);
    }
}



    private void CreatePartyEntities()
    {
        List<PartyMember> currentParty = partyManager.GetCurrentParty();

        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(
                currentParty[i].MemberName,
                currentParty[i].CurrentHealth,
                currentParty[i].MaxHealth,
                currentParty[i].Initiative,
                currentParty[i].Strength,
                currentParty[i].Level,
                true,
                EntityType.Friendly,
                currentParty[i].Resistances
            );

            if (currentParty[i].MemberBattleVisualPrefab != null)
            {
                // Instantiate visual at designated spawn point and assign it to BattleVisuals
                BattleVisuals tempBattleVisuals = Instantiate(
                    currentParty[i].MemberBattleVisualPrefab,
                    partySpawnPoints[i].position,
                    Quaternion.identity
                ).GetComponent<BattleVisuals>();

                // Set initial values on BattleVisuals for consistency
                tempBattleVisuals.SetStartingValues(
                    currentParty[i].CurrentHealth,
                    currentParty[i].MaxHealth,
                    currentParty[i].Level
                );

                // Link BattleVisuals to both BattleEntities and PartyMember for real-time updates
                tempEntity.BattleVisuals = tempBattleVisuals;
                currentParty[i].BattleVisuals = tempBattleVisuals;

                // Link the BattleEntities object to the BattleVisuals
                tempBattleVisuals.LinkToEntity(tempEntity);

            }
            else
            {
                Debug.LogWarning($"MemberBattleVisualPrefab is not assigned for {currentParty[i].MemberName} in PartyMemberInfo.");
            }

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }

    public BattleEntities GetCurrentPlayer()
    {
        if (playerBattlers != null && playerBattlers.Count > 0)
        {
            return playerBattlers[0]; // Assume the first player is the current active player
        }

        Debug.LogWarning("No players available in playerBattlers.");
        return null;
    }

    public void EndBattle()
    {
        foreach (var entity in allBattlers)
        {
            entity.ClearBuffs();
        }

        Debug.Log("Battle ended, all buffs cleared.");
    }

}

[System.Serializable]
public class BattleEntities
{
    public string Name;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public int Level;
    public bool IsPlayer;
    public BattleVisuals BattleVisuals;
    public EntityType EntityType;
    public DamageResistances Resistances;

    private BuffManager buffManager = new BuffManager();

    // Resistances to different damage types (0-100%)
    public Dictionary<DamageType, float> DamageResistances = new Dictionary<DamageType, float>();

    public void SetEntityValues(
        string name,
        int currentHealth,
        int maxHealth,
        int initiative,
        int strength,
        int level,
        bool isPlayer,
        EntityType entityType,
        DamageResistances resistances = null)


    {
        Name = name;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Initiative = initiative;
        Strength = strength;
        Level = level;
        IsPlayer = isPlayer;
        EntityType = entityType;
        Resistances = resistances ?? new DamageResistances();

        Debug.Log($"Entity initialized: {name}, EntityType: {entityType}");
    }

    public int TakeDamage(int damage, DamageType damageType)
    {
        Debug.Log($"[TakeDamage] Receiving Damage: {damage}");

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0); // Prevent negative HP

        Debug.Log($"[{Name}] Took {damage} {damageType} damage. Remaining HP: {CurrentHealth}");

        if (CurrentHealth == 0)
        {
            HandleDeath();
        }

        BattleVisuals?.SyncWithEntity(this);
        BattleVisuals?.PlayHitAnimation();

        return damage;
    }

    public void ApplyDebuff(Debuff debuff)
    {
        buffManager.AddDebuff(debuff);
    }

    public void ApplyDebuffEffects()
    {
        buffManager.ApplyDebuffEffects(this);
    }

    private void HandleDeath()
    {
        Debug.Log($"{Name} has died!");
        BattleVisuals?.PlayDeathAnimation();
    }

    public void ApplyBuff(Buff buff)
    {
        // Ensure that only the intended entity type receives the buff
        if (EntityType != buff.SourceType)
        {
            return;
        }
        buffManager.AddBuff(buff);
    }

    public float GetBuffedDamageMultiplier(DamageType damageType)
    {
        return buffManager.GetBuffedDamageMultiplier(damageType, EntityType);
    }

    public void ConsumeBuff(DamageType damageType)
    {
        buffManager.ConsumeBuff(damageType, EntityType);
    }


    public void ClearBuffs()
    {
        buffManager.ClearAllBuffs();
    }
}




