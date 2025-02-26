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
            Debug.Log($"{effect.EffectName} refreshed with new charges.");
        }
        else if (existingEffect != null && effect.Stackable)
        {
            // Stack both charges and percentage for buffs
            existingEffect.ConsumeCharge += effect.ConsumeCharge;
            if (existingEffect is Buff existingBuff && effect is Buff newBuff)
            {
                existingBuff.AddPercentage(newBuff.Percentage);
                Debug.Log($"{effect.EffectName} stacked. New percentage: {existingBuff.Percentage}%, Charges: {existingBuff.ConsumeCharge}");
            }
        }
        else
        {
            // Add new effect if it doesn't exist
            activeEffects.Add(effect);
            effect.Apply(target);
            target.BattleVisuals?.AddBuffIcon(effect.BuffIconSprite, effect is Buff, effect.ConsumeCharge);
            Debug.Log($"Applied new effect: {effect.EffectName}.");
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

    // Consume Buff
    public void ConsumeBuff(DamageType damageType, EntityType entityType)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i] is Buff buff &&
                buff.DamageType == damageType &&
                buff.SourceType == entityType)
            {
                buff.Consume();
                Debug.Log($"Consuming buff {buff.EffectName}, remaining: {buff.ConsumeCharge}");

                if (buff.ConsumeCharge <= 0)
                {
                    activeEffects.RemoveAt(i);
                    Debug.Log($"Buff {buff.EffectName} expired.");
                }
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
