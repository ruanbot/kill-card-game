using UnityEngine;

[CreateAssetMenu(menuName = "Cards/SharpenData")]
public class SharpenData : Card
{
    public DamageType BuffedDamageType = DamageType.Slash;
    public float BuffPercentage;
    public int BuffUses;

    public SharpenData()
    {
        targetType = TargetType.Friendly;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        // Only apply the buff to player-controlled characters
        if (target.IsPlayer)
        {
            var newBuff = new Buff(BuffedDamageType, BuffPercentage, BuffUses, caster.EntityType);
            target.ApplyBuff(newBuff);
            Debug.Log($"{cardName} used: {target.Name} now has {BuffPercentage}% increased {BuffedDamageType} damage.");
        }
        else
        {
            Debug.LogWarning($"{cardName} was attempted on {target.Name}, but buffs only apply to players.");
        }


        Debug.Log($"{cardName} applied to {target.Name} ({target.EntityType}). Buff: {BuffPercentage}% increased {BuffedDamageType} for {BuffUses} uses.");
    }



    public override void Upgrade()
    {
        BuffPercentage += 5; // Example: Increase the buff percentage
        Debug.Log($"{cardName} upgraded: BuffPercentage = {BuffPercentage}%");
    }
}
