using UnityEngine;
using UnityEngine.UI;

public class Buff : CombatEffect
{
    public DamageType DamageType { get; private set; }
    public float Percentage { get; private set; }
    public EntityType SourceType { get; private set; }

    public Buff(
        string effectName,
        DamageType damageType,
        float percentage,
        int charges,
        EntityType sourceType,
        Sprite iconSprite,
        bool stackable)
        : base(effectName, charges, iconSprite, stackable)
    {
        DamageType = damageType;
        Percentage = percentage;
        SourceType = sourceType;
    }

    public override bool TriggerEffect(BattleEntities target, EffectTriggerType triggerType)
    {
        if (TriggerType.HasFlag(triggerType))
        {
            // Debug.Log($"Consuming {EffectName} charge on {triggerType}. Charges before: {ConsumeCharge}");
            ReduceCharge();
            // Debug.Log($"Charges remaining: {ConsumeCharge}");
            return !IsExpired();
        }
        return true;
    }

    public override void Apply(BattleEntities target)
    {
        Debug.Log($"Applied Buff: {EffectName} - {Percentage}% increased {DamageType} damage.");
    }

    public void Consume()
    {
        ReduceCharge();
    }

    public void AddPercentage(float additionalPercentage)
    {
        Percentage += additionalPercentage;
    }
}
