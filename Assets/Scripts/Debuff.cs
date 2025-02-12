using UnityEngine;

public class Debuff
{
    public string Name { get; private set; }
    public int DamagePerTrigger { get; private set; }
    public int RemainingTriggers { get; private set; }

    public Debuff(string name, int damage, int triggers)
    {
        Name = name;
        DamagePerTrigger = damage;
        RemainingTriggers = triggers;
    }

    public void Stack(int extraDamage, int extraTriggers)
    {
        DamagePerTrigger += extraDamage; // Increase Bleed damage if stacking
        RemainingTriggers += extraTriggers; // Increase duration
    }

    public bool Consume(BattleEntities target)
    {
        target.TakeDamage(DamagePerTrigger, DamageType.Bleed);
        Debug.Log($"[{target.Name}] suffered {DamagePerTrigger} damage from {Name}. Remaining Triggers: {RemainingTriggers - 1}");

        RemainingTriggers--;
        return RemainingTriggers > 0;
    }
}
