using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SlashData")]
public class SlashData : Card
{
    public DamageType damageType = DamageType.Slash;
    public int Damage = 2;

    public SlashData()
    {
        targetType = TargetType.Enemy;
        damageType = DamageType.Slash;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>();

        // Ensure base damage is consistent
        int baseDamage = Damage;

        // Play the attack animation
        caster.BattleVisuals?.PlayAttackAnimation();

        // Apply the adjusted damage to the target
        cardEffect.DealDamage(caster, target, baseDamage, damageType);
        Debug.Log($"{cardName} used: Base Damage = {baseDamage} on {target.Name}");
    }



    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
