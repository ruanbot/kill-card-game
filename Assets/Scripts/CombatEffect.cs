using UnityEngine;
using UnityEngine.UI;

public enum EffectTriggerType
{
    OnPlayerAttack = 1,
    OnEnemyAttack = 2,
    OnDamageReceived = 4,
    OnCardUse = 8,
}

public abstract class CombatEffect
{
    public string EffectName { get; protected set; }
    public int ConsumeCharge { get; set; }
    public Sprite BuffIconSprite { get; protected set; }
    public bool Stackable { get; set; }
    public EffectTriggerType TriggerType { get; set; }

    public CombatEffect(string effectName, int charges, Sprite iconSprite, bool stackable)
    {
        EffectName = effectName;
        ConsumeCharge = charges;
        BuffIconSprite = iconSprite;
        Stackable = stackable;
    }

    public abstract void Apply(BattleEntities target);
    public abstract bool TriggerEffect(BattleEntities target, EffectTriggerType triggerType);

    public void ReduceCharge()
    {
        ConsumeCharge--;
    }

    public bool IsExpired()
    {
        return ConsumeCharge <= 0;
    }

    public virtual float GetDamageMultiplier(DamageType damageType, EntityType entityType)
    {
        return 1.0f;  // Default implementation returns no multiplier
    }
}
