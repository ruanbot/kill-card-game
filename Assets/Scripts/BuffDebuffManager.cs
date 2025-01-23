using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BuffDebuffManager
{
    private List<Buff> activeBuffs = new List<Buff>();
    private List<Debuff> activeDebuffs = new List<Debuff>();

    public IEnumerable<Buff> ActiveBuffs => activeBuffs;
    public IEnumerable<Debuff> ActiveDebuffs => activeDebuffs;

    /// <summary>
    /// Applies a buff to the entity.
    /// </summary>
    public void ApplyBuff(Buff buff)
    {
        activeBuffs.Add(buff);
        Debug.Log($"Applied Buff: {buff.BuffName} ({buff.DamageType}, {buff.Percentage}%)");
    }

    /// <summary>
    /// Applies a debuff to the entity.
    /// </summary>
    public void ApplyDebuff(Debuff debuff)
    {
        activeDebuffs.Add(debuff);
        Debug.Log($"Applied Debuff: {debuff.DebuffName} ({debuff.DamageType}, Remaining Attacks: {debuff.RemainingTriggers})");
    }

    /// <summary>
    /// Processes buffs when attacking.
    /// </summary>
    public float GetBuffedDamageMultiplier(DamageType damageType)
    {
        float multiplier = 1.0f;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];
            if (buff.DamageType == damageType)
            {
                multiplier += buff.Percentage / 100f;
                buff.RemainingUses--;

                if (buff.RemainingUses <= 0)
                {
                    activeBuffs.RemoveAt(i);
                    Debug.Log($"Buff {buff.BuffName} expired.");
                }
            }
        }
        return multiplier;
    }

    /// <summary>
    /// Processes debuffs when receiving damage.
    /// </summary>
    public void ProcessDebuffsOnHit(BattleEntities entity)
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            var debuff = activeDebuffs[i];

            // If it's a "Broken Armor" type (decreases on being attacked)
            if (debuff.TriggerType == DebuffTrigger.OnHit)
            {
                debuff.RemainingTriggers--;
                if (debuff.RemainingTriggers <= 0)
                {
                    activeDebuffs.RemoveAt(i);
                    Debug.Log($"Debuff {debuff.DebuffName} expired on {entity.Name}.");
                }
            }
        }
    }

    /// <summary>
    /// Processes debuffs when attacking.
    /// </summary>
    public void ProcessDebuffsOnAttack(BattleEntities entity)
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            var debuff = activeDebuffs[i];

            // If it's a "Bleed" type (decreases on attacking)
            if (debuff.TriggerType == DebuffTrigger.OnAttack)
            {
                entity.TakeDamage(debuff.DamageAmount, DamageType.True); // True Damage bypasses resistances
                debuff.RemainingTriggers--;
                if (debuff.RemainingTriggers <= 0)
                {
                    activeDebuffs.RemoveAt(i);
                    Debug.Log($"Debuff {debuff.DebuffName} expired on {entity.Name}.");
                }
            }
        }
    }

    public void ClearAllEffects()
    {
        activeBuffs.Clear();
        activeDebuffs.Clear();
    }
}
