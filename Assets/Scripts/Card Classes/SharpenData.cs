using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SharpenData")]
public class SharpenData : Card
{
    public DamageType BuffedDamageType = DamageType.Slash;
    public float BuffPercentage;

    public SharpenData()
    {
        targetType = TargetType.Friendly;
    }
    
    public override void Use(BattleEntities caster, BattleEntities target)
    {
        // Apply the buff to the target
        var newBuff = new Buff
        {
            DamageType = BuffedDamageType,
            Percentage = BuffPercentage,
            RemainingUses = 3
        };

        target.ApplyBuff(newBuff);
        Debug.Log($"{cardName} used: {target.Name} now has {BuffPercentage}% increased {BuffedDamageType} damage.");

        // Log all active buffs
        foreach (var activeBuff in target.ActiveBuffs)
        {
            // Debug.Log($"Active Buff - DamageType: {activeBuff.DamageType}, Percentage: {activeBuff.Percentage}%");
        }
    }


    public override void Upgrade()
    {
        BuffPercentage += 5; // Example: Increase the buff percentage
        Debug.Log($"{cardName} upgraded: BuffPercentage = {BuffPercentage}%");
    }
}
