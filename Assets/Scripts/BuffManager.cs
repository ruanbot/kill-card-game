using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuffManager
{
    private List<CombatEffect> activeEffects = new List<CombatEffect>();

    // Get all active CombatEffects (Buffs/Debuffs)
    public List<CombatEffect> GetActiveEffects() => new List<CombatEffect>(activeEffects);

    // Add Buff or Debuff 
    public void AddEffect(CombatEffect effect, BattleEntities target)
    {
        CombatEffect existingEffect = activeEffects.FirstOrDefault(e => 
            e.EffectName == effect.EffectName);

        if (existingEffect != null && !effect.Stackable)
        {
            // Renew charges if not stackable
            existingEffect.ConsumeCharge = effect.ConsumeCharge;
            Debug.Log($"{effect.EffectName} refreshed with new charges: {effect.ConsumeCharge}");
        }
        else if (existingEffect != null && effect.Stackable)
        {
            // Stack charges for all effects
            existingEffect.ConsumeCharge += effect.ConsumeCharge;
            
            // Stack percentages only for buffs (like Sharpen)
            if (existingEffect is Buff existingBuff && effect is Buff newBuff)
            {
                existingBuff.AddPercentage(newBuff.Percentage);
                Debug.Log($"{effect.EffectName} stacked. New percentage: {existingBuff.Percentage}%, Charges: {existingEffect.ConsumeCharge}");
            }
            else
            {
                Debug.Log($"{effect.EffectName} stacked. Charges: {existingEffect.ConsumeCharge}");
            }
        }
        else
        {
            // Add new effect if it doesn't exist
            activeEffects.Add(effect);
            // Removed effect.Apply(target) as it might cause recursive application
            target.BattleVisuals?.AddBuffIcon(effect.BuffIconSprite, effect is Buff, effect.ConsumeCharge);
            Debug.Log($"Applied new {effect.EffectName} with {effect.ConsumeCharge} charges.");
        }

        target.BattleVisuals?.UpdateBuffIcons(GetActiveEffects());
    }


    // Trigger All Active Effects
    public void TriggerEffects(BattleEntities target, EffectTriggerType triggerType)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            if (!effect.TriggerEffect(target, triggerType))
            {
                Debug.Log($"{effect.EffectName} expired.");
                activeEffects.RemoveAt(i);
            }
        }
        target.BattleVisuals?.UpdateBuffIcons(GetActiveEffects());
    }



    // Calculate Buffed Damage Multiplier
    public float GetBuffedDamageMultiplier(DamageType damageType, EntityType entityType)
    {
        float totalBuffMultiplier = 1.0f;

        foreach (var effect in activeEffects)
        {
            if (effect is Buff buff)
            {
                // If SourceType is Self or matches the entity's type
                if (buff.DamageType == damageType && 
                    (buff.SourceType == EntityType.Self || buff.SourceType == entityType))
                {
                    totalBuffMultiplier += buff.Percentage;
                }
            }
        }
        return totalBuffMultiplier;
    }

    // New method for debuff multipliers
    public float GetDebuffMultiplier(DamageType damageType, EntityType entityType)
    {
        float totalDebuffMultiplier = 1.0f;

        foreach (var effect in activeEffects)
        {
            if (effect is Debuff debuff)
            {
                float debuffMultiplier = debuff.GetDamageMultiplier(damageType, entityType);
                totalDebuffMultiplier *= debuffMultiplier;
            }
        }

        return totalDebuffMultiplier;
    }

    // Consume Buff/Debuff
    public void ConsumeBuff(DamageType damageType, EntityType entityType)
    {
        Debug.Log($"[ConsumeBuff] Starting buff consumption for {damageType}");
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            
            // For regular buffs
            if (effect is Buff buff && 
                buff.DamageType == damageType &&
                (buff.SourceType == EntityType.Self || buff.SourceType == entityType))
            {
                effect.ReduceCharge();
                Debug.Log($"Consuming buff {buff.EffectName}, remaining: {effect.ConsumeCharge}");
                
                if (effect.IsExpired())
                {
                    activeEffects.RemoveAt(i);
                    Debug.Log($"{effect.EffectName} expired.");
                }
            }
            // For bleed debuff - don't consume here, let TriggerEffect handle it
            else if (effect is Debuff && effect is { } dotDebuff && 
                     ((Debuff)effect).Type == DebuffType.DamageOverTime)
            {
                Debug.Log($"Found bleed effect with {effect.ConsumeCharge} charges");
            }
        }
    }

    // Clear All Effects
    public void ClearAllEffects()
    {
        activeEffects.Clear();
        Debug.Log("All effects cleared.");
    }
}
