using System.Collections.Generic;
using UnityEngine;

public class BuffManager
{
    private List<Buff> activeBuffs = new List<Buff>();
    private List<Debuff> activeDebuffs = new List<Debuff>();

    public void AddBuff(Buff buff)
    {
        activeBuffs.Add(buff);
        Debug.Log($"Applied buff: {buff.DamageType}, {buff.Percentage}% for {buff.RemainingUses} uses.");
    }

    public void AddDebuff(Debuff debuff)
    {
        // Check if the debuff already exists
        bool found = false;
        foreach (var existingDebuff in activeDebuffs)
        {
            if (existingDebuff.Name == debuff.Name)
            {
                // If it's the same type of debuff, increase the stack count
                existingDebuff.Stack(debuff.DamagePerTrigger, debuff.RemainingTriggers);
                found = true;
                Debug.Log($"Stacking {debuff.Name}: Now does {existingDebuff.DamagePerTrigger} damage for {existingDebuff.RemainingTriggers} triggers.");
                break;
            }
        }

        if (!found)
        {
            activeDebuffs.Add(debuff);
            Debug.Log($"Applied debuff: {debuff.Name}, {debuff.DamagePerTrigger} damage for {debuff.RemainingTriggers} triggers.");
        }
    }

    public float GetBuffedDamageMultiplier(DamageType damageType, EntityType entityType)
    {
        float totalBuffMultiplier = 1.0f; // Base 1.0 = 100% of normal damage

        foreach (var buff in activeBuffs)
        {
            if (buff.DamageType == damageType && buff.SourceType == entityType)
            {
                totalBuffMultiplier += (buff.Percentage / 100f); // Proper additive stacking
            }
        }

        return totalBuffMultiplier;
    }




    public void ConsumeBuff(DamageType damageType, EntityType entityType)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = activeBuffs[i];

            if (buff.DamageType == damageType && buff.SourceType == entityType)
            {
                Debug.Log($"Consuming buff {buff.DamageType} from {entityType}, remaining uses: {buff.RemainingUses}");

                buff.Consume();

                if (buff.RemainingUses <= 0)
                {
                    Debug.Log($"Buff {buff.DamageType} expired for {entityType}");
                    activeBuffs.RemoveAt(i);
                }
                else
                {
                    activeBuffs[i] = buff; // Ensure the updated struct is reassigned
                }
            }
        }

    }

    public void ApplyDebuffEffects(BattleEntities target)
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            if (!activeDebuffs[i].Consume(target))
            {
                activeDebuffs.RemoveAt(i); // Remove expired debuffs
            }
        }
    }



    public void ClearAllBuffs()
    {
        activeBuffs.Clear();
        Debug.Log("All buffs cleared.");
    }
}
