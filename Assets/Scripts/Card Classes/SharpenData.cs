using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SharpenData")]
public class SharpenData : Card
{
    public SharpenData()
    {
        targetType = TargetType.Friendly;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        if (specialEffect is SharpenEffect sharpenEffect)
        {
            CombatEffect effect = sharpenEffect.CreateEffect();
            target.ApplyEffect(effect);
            Debug.Log($"{cardName} applied: {sharpenEffect.buffPercentage}% increased {sharpenEffect.buffedDamageType} damage.");
        }
    }

    public override void Upgrade()
    {
        if (specialEffect is SharpenEffect sharpenEffect)
        {
            sharpenEffect.buffPercentage += 5f;
            Debug.Log($"{cardName} upgraded: Buff increased to {sharpenEffect.buffPercentage}%.");
        }
    }
}