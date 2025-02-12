using UnityEngine;

[CreateAssetMenu(menuName = "Special Effect/BleedEffect")]
public class BleedEffect : EnemyAttackSpecialEffects
{
    public int damagePerTrigger = 1;
    public int maxTriggers = 2;

    public override void ApplyEffect(BattleEntities target)
    {
        var bleed = new Debuff("Bleed", damagePerTrigger, maxTriggers);
        target.ApplyDebuff(bleed);
        Debug.Log($"[{target.Name}] is now Bleeding! Takes {damagePerTrigger} damage for {maxTriggers} attacks.");
    }
}
