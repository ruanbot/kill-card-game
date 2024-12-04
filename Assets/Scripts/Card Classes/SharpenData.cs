using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SharpenData")]
public class SharpenData : Card
{
    public DamageType BuffedDamageType;
    public float BuffPercentage;

    public SharpenData()
    {
        targetType = TargetType.Friendly;
    }
    public override void Use(BattleEntities caster, BattleEntities target)
    {
        // Apply the buff to the target
        var buff = new Buff
        {
            DamageType = BuffedDamageType,
            Percentage = BuffPercentage
        };

        target.ApplyBuff(buff);
        Debug.Log($"{cardName} used: {target.Name} now has {BuffPercentage}% increased {BuffedDamageType} damage.");
    }

    public override void Upgrade()
    {
        BuffPercentage += 5; // Example: Increase the buff percentage
        Debug.Log($"{cardName} upgraded: BuffPercentage = {BuffPercentage}%");
    }
}
