using UnityEngine;

[CreateAssetMenu(menuName = "Cards/MinorPotionData")]
public class MinorPotionData : Card
{
    public MinorPotionData()
    {
        targetType = TargetType.Friendly;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        // Check if the target already has full health
        if (target.CurrentHealth >= target.MaxHealth)
        {
            Debug.Log($"{cardName} cannot be used: {target.Name} already has full HP.");
            return; // Prevent further execution
        }

        var cardEffect = FindFirstObjectByType<CardEffect>(); // Reference the card effect script

        // Heal the target
        cardEffect.Heal(target, 2);

        Debug.Log($"{cardName}: {target.Name} heals for 2 HP.");
    }

    public override void Upgrade()
    {
        // Implement upgrade logic if needed
    }
}
