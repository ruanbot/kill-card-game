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

            // Instantiate the visual prefab at the designated spawn point and assign to BattleVisuals
            if (currentEnemies[i].EnemyVisualPrefab != null)
            {
                BattleVisuals tempBattleVisuals = Instantiate(
                    currentEnemies[i].EnemyVisualPrefab,
                    enemySpawnPoints[i].position,
                    Quaternion.identity
                ).GetComponent<BattleVisuals>();

                // Initialize visual with enemy data
                tempBattleVisuals.SetStartingValues(
                    currentEnemies[i].CurrentHealth,
                    currentEnemies[i].MaxHealth,
                    currentEnemies[i].Level
                );

                // Link the visual back to the BattleEntities object
                tempEntity.BattleVisuals = tempBattleVisuals;
            }
            else
            {
                Debug.LogWarning($"EnemyVisualPrefab is not assigned for {currentEnemies[i].EnemyName} in EnemyInfo.");
            }

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);

            Debug.Log($"Enemy created: {tempEntity.Name}, EntityType: {tempEntity.EntityType}");

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

    public void TakeDamage(int damage, DamageType damageType)
    {
        // Check if the entity is immune to this type of damage
        if (Resistances.IsImmune(damageType))
        {
            Debug.Log($"{Name} is immune to {damageType} damage. No damage taken.");
            return; // Negate damage if immune
        }
        float resistance = Resistances.GetResistance(damageType);
        int reducedDamage = Mathf.FloorToInt(damage * (1 - resistance));

        CurrentHealth -= reducedDamage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0); // Clamp health to 0

        Debug.Log($"{Name} took {reducedDamage} {damageType} damage (Resistance: {resistance * 100}%)");

        if (CurrentHealth == 0)
        {
            HandleDeath();
        }

        // Update Visuals
        BattleVisuals?.SyncWithEntity(this);
        BattleVisuals?.PlayHitAnimation();
    }

    private void HandleDeath()
    {
        Debug.Log($"{Name} has died!");
        BattleVisuals?.PlayDeathAnimation();
    }
}


