using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SlashData")]
public class SlashData : Card
{
    public int Damage = 2;

    public SlashData()
    {
        targetType = TargetType.Enemy;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>();

        caster.BattleVisuals?.PlayAttackAnimation();
        cardEffect.DealDamage(target, Damage);
        Debug.Log($"{cardName} used: {Damage} damage to {target.Name}");

    }

    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
