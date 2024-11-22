using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SlashData")]
public class SlashData : Card
{
    public int Damage = 2;

    public SlashData()
    {
        targetType = TargetType.Enemy; // Correct usage
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        if (target.EntityType == EntityType.Enemy)
        {
            target.TakeDamage(Damage);

            // Update visuals
            caster.BattleVisuals?.PlayAttackAnimation();

            Debug.Log($"{cardName} used: {Damage} damage to {target.Name}");
        }
        else
        {
            Debug.LogWarning($"{cardName} can only target enemies.");
        }
    }

    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
