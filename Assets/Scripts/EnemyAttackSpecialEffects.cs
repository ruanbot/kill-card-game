
using UnityEngine;

[System.Serializable]
public abstract class EnemyAttackSpecialEffects : ScriptableObject
{
    public abstract void ApplyEffect(BattleEntities target);
}

// Example: Disable cards for X seconds
[CreateAssetMenu(menuName = "Special Effect/DisableCards")]
public class DisableCardsEffect : EnemyAttackSpecialEffects
{
    public float disableDuration;

    public override void ApplyEffect(BattleEntities target)
    {
        Debug.Log($"Disabling cards for {target.Name} for {disableDuration} seconds.");
        // Implement logic to disable player cards
    }
}
