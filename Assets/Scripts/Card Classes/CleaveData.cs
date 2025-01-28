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

        // Ensure base damage is consistent
        int baseDamage = Damage;

        // Loop through all enemy entities in the scene
        var battleSystem = FindFirstObjectByType<BattleSystem>();

        caster.BattleVisuals?.PlayAttackAnimation();

        foreach (var enemy in battleSystem.enemyBattlers)
        {
            cardEffect.DealDamage(caster, target, baseDamage, damageType);
            Debug.Log($"{cardName} used: {Damage} damage to {target.Name}");
        }

    }

    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
