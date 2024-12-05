using UnityEngine;

[CreateAssetMenu(menuName = "Cards/CleaveData")]
public class CleaveData : Card
{
    public DamageType damageType;
    public int Damage = 2;

    public CleaveData()
    {
        targetType = TargetType.AllEnemy;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>();

        // Calculate the damage multiplier from the caster's buffs
        float multiplier = caster.GetBuffedDamageMultiplier(damageType);

        // Adjust damage based on the multiplier
        int adjustedDamage = Mathf.FloorToInt(Damage * multiplier);

        // Loop through all enemy entities in the scene
        var battleSystem = FindFirstObjectByType<BattleSystem>();

        caster.BattleVisuals?.PlayAttackAnimation();

        foreach (var enemy in battleSystem.enemyBattlers)
        {
            cardEffect.DealDamage(enemy, adjustedDamage, damageType);
            Debug.Log($"{cardName} used: {Damage} damage to {target.Name}");
        }

    }

    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
