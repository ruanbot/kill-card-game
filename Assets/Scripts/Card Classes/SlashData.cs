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

        // Calculate the damage multiplier from the caster's buffs
        float multiplier = caster.GetBuffedDamageMultiplier(damageType);
        // Debug.Log($"Caster {caster.Name} has a multiplier for {damageType}: {multiplier}");

        // Adjust damage based on the multiplier
        int adjustedDamage = Mathf.FloorToInt(Damage * multiplier);

        // Play the attack animation
        caster.BattleVisuals?.PlayAttackAnimation();

        // Apply the adjusted damage to the target
        cardEffect.DealDamage(target, adjustedDamage, damageType);
        Debug.Log($"{cardName} used: {Damage} base damage (adjusted to {adjustedDamage}) on {target.Name}");
    }



    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
