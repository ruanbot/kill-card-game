
using UnityEngine;

public class EnemyAttackSystem : MonoBehaviour
{
    public void ExecuteAttack(BattleEntities attacker, BattleEntities target, EnemyAttack attack)
    {
        if (attacker == null || target == null || attack == null)
        {
            Debug.LogError("Invalid attack execution parameters.");
            return;
        }

        // Debug.Log($"{attack.attackName}: Executing attack from {attacker.Name} on {target.Name}");
        attack.Execute(attacker, target);
    }
}
