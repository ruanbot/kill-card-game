using System.Collections.Generic;
using UnityEngine;

public class BuffManager
{
    private List<Buff> activeBuffs = new List<Buff>();

    public void AddBuff(Buff buff)
    {
        activeBuffs.Add(buff);
        Debug.Log($"Applied buff: {buff.DamageType}, {buff.Percentage}% for {buff.RemainingUses} uses.");
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



    public void ClearAllBuffs()
    {
        activeBuffs.Clear();
        Debug.Log("All buffs cleared.");
    }
}
