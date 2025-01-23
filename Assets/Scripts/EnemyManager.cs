using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyInfo[] allEnemies;
    [SerializeField] private List<Enemy> currentEnemies;
    private const float LEVEL_MODIFIER = 0.5f;

    private List<EnemyBehavior> activeEnemies = new List<EnemyBehavior>();

    public void RegisterEnemy(EnemyBehavior enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(EnemyBehavior enemy)
    {
        activeEnemies.Remove(enemy);
    }

    private void Awake()
    {
        GenerateEnemyByName("Skele Boi", 1);
    }

    private void GenerateEnemyByName(string enemyName, int level)
    {
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (enemyName == allEnemies[i].EnemyName)
            {

                Enemy newEnemy = new Enemy
                {
                    EnemyName = allEnemies[i].EnemyName,
                    Level = level,
                    MaxHealth = Mathf.RoundToInt(allEnemies[i].BaseHealth + (allEnemies[i].BaseHealth * (LEVEL_MODIFIER * level))),
                    CurrentHealth = Mathf.RoundToInt(allEnemies[i].BaseHealth + (allEnemies[i].BaseHealth * (LEVEL_MODIFIER * level))),
                    Strength = Mathf.RoundToInt(allEnemies[i].BaseStr + (allEnemies[i].BaseStr * (LEVEL_MODIFIER * level))),
                    Initiative = Mathf.RoundToInt(allEnemies[i].BaseInitiative + (allEnemies[i].BaseInitiative * (LEVEL_MODIFIER * level))),
                    Resistances = allEnemies[i].resistances, // Set resistances from scriptable object
                    EnemyVisualPrefab = allEnemies[i].EnemyVisualPrefab // Store the visual prefab for the enemy
                };

                currentEnemies.Add(newEnemy);

            }
        }
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
    }
}

[System.Serializable]
public class Enemy
{
    public string EnemyName;
    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public GameObject EnemyVisualPrefab;

    // Resistances to different damage types
    public DamageResistances Resistances;
}
