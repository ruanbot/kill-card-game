using UnityEngine;

[CreateAssetMenu(menuName = "Cards/CleaveData")]
public class CleaveData : Card
{
    public int Damage = 2;

    public CleaveData()
    {
        targetType = TargetType.AllEnemy;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>();

        // Loop through all enemy entities in the scene
        var battleSystem = FindFirstObjectByType<BattleSystem>();
        foreach (var enemy in battleSystem.enemyBattlers)
        {
            caster.BattleVisuals?.PlayAttackAnimation();
            cardEffect.DealDamage(enemy, Damage, DamageType.Slash);
            Debug.Log($"{cardName} used: {Damage} damage to {target.Name}");
        }

    }

    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
