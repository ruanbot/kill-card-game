using UnityEngine;
using UnityEngine.UI;

public enum DebuffType
{
    None,
    DamageOverTime,  // Damage over time
    Weakness,        // Reduces damage dealt
    Vulnerable,      // Takes increased damage
    Exhausted,       // Reduces energy
    // Add more debuff types as needed
}

public class Debuff : CombatEffect
{
    public DebuffType Type { get; private set; }
    public int DamagePerTrigger { get; private set; }
    public float EffectValue { get; private set; }  // Generic value (damage reduction %, energy reduction, etc.)

    public delegate bool TriggerHandler(Debuff debuff, BattleEntities target, EffectTriggerType triggerType);
    public TriggerHandler OnTrigger { get; set; }

    public Debuff(
        string name, 
        DebuffType type,
        int charges, 
        Sprite iconSprite, 
        bool stackable,
        float effectValue = 0,
        int damagePerTrigger = 0)
        : base(name, charges, iconSprite, stackable)
    {
        Type = type;
        EffectValue = effectValue;
        DamagePerTrigger = damagePerTrigger;
    }

    public override void Apply(BattleEntities target)
    {
        target.ApplyEffect(this);
    }

    public override bool TriggerEffect(BattleEntities target, EffectTriggerType triggerType)
    {
        if (OnTrigger != null)
        {
            return OnTrigger(this, target, triggerType);
        }
        
        if (TriggerType.HasFlag(triggerType))
        {
            switch (Type)
            {
                case DebuffType.Exhausted:
                    var energyManager = GameObject.FindFirstObjectByType<EnergyManager>();
                    if (energyManager != null)
                    {
                        energyManager.SpendEnergy((int)EffectValue);
                        Debug.Log($"Exhausted debuff reduced energy by {EffectValue}");
                    }
                    break;
            }
            ReduceCharge();
            return !IsExpired();
        }
        return true;
    }

    public override float GetDamageMultiplier(DamageType damageType, EntityType entityType)
    {
        return Type switch
        {
            DebuffType.Weakness => 1.0f - (EffectValue / 100f),
            DebuffType.Vulnerable => 1.0f + (EffectValue / 100f),
            _ => 1.0f
        };
    }
}
