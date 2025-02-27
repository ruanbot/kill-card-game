using UnityEngine;

[CreateAssetMenu(menuName = "Special Effect/BleedEffect")]
public class BleedEffect : CombatSpecialEffects
{
    public int damagePerTrigger = 1;

    public override CombatEffect CreateEffect()
    {
        var effect = this;  // Capture the BleedEffect instance
        return new Debuff(
            "Bleed",
            DebuffType.DamageOverTime,
            consumeCharge,
            BuffIconSprite,
            stackable,
            damagePerTrigger: damagePerTrigger
        )
        {
            TriggerType = EffectTriggerType.OnPlayerAttack,
            OnTrigger = (debuff, target, triggerType) =>
            {
                if (triggerType == EffectTriggerType.OnPlayerAttack)
                {
                    int stackedDamage = damagePerTrigger * debuff.ConsumeCharge;
                    target.TakeDamage(stackedDamage, DamageType.Bleed);
                    target.BattleVisuals?.ShowPopup(stackedDamage, true, true);
                    debuff.ReduceCharge();
                    return !debuff.IsExpired();
                }
                return true;
            }
        };
    }
}
