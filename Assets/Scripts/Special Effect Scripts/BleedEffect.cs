using UnityEngine;

[CreateAssetMenu(menuName = "Special Effect/BleedEffect")]
public class BleedEffect : CombatSpecialEffects
{
    public int damagePerTrigger = 1;

    public override CombatEffect CreateEffect()
    {
        return new Debuff(
            "Bleed",
            DebuffType.DamageOverTime,
            consumeCharge,
            BuffIconSprite,
            stackable,
            damagePerTrigger: damagePerTrigger
        )
        {
            TriggerType = EffectTriggerType.OnDamageReceived
        };
    }
}
