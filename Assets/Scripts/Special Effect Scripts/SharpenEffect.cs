using UnityEngine;

[CreateAssetMenu(menuName = "Special Effect/SharpenEffect")]
public class SharpenEffect : CombatSpecialEffects
{
    public DamageType buffedDamageType = DamageType.Slash;
    public float buffPercentage = 50f;
    public EntityType sourceType;

    public override CombatEffect CreateEffect()
    {
        return new Buff(
            "Sharpen",
            buffedDamageType,
            buffPercentage,
            consumeCharge,
            sourceType,
            BuffIconSprite,
            stackable
        )
        {
            TriggerType = EffectTriggerType.OnAttack
        };
    }
}
