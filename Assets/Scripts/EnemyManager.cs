using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyInfo[] allEnemies;
    [SerializeField] private List<Enemy> currentEnemies;
    private const float LEVEL_MODIFIER = 0.5f;

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
                Enemy newEnemy = new Enemy();

                newEnemy.EnemyName = allEnemies[i].EnemyName;
                newEnemy.Level = level;
                float levelModifier = (LEVEL_MODIFIER * newEnemy.Level);

                newEnemy.MaxHealth = Mathf.RoundToInt(allEnemies[i].BaseHealth + (allEnemies[i].BaseHealth * levelModifier));
                newEnemy.CurrentHealth = newEnemy.MaxHealth;
                newEnemy.Strength = Mathf.RoundToInt(allEnemies[i].BaseStr + (allEnemies[i].BaseStr * levelModifier));
                newEnemy.Initiative = Mathf.RoundToInt(allEnemies[i].BaseInitiative + (allEnemies[i].BaseInitiative * levelModifier));

                newEnemy.EnemyVisualPrefab = allEnemies[i].EnemyVisualPrefab;  // Store the instance as the visual prefab for the enemy
                

                currentEnemies.Add(newEnemy);
            }
        }
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
    }


    // Any method that needs to update enemy health can now reference BattleVisuals through EnemyVisualPrefab
}






