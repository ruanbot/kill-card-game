using UnityEngine;

public class Buff
{
    public DamageType DamageType { get; private set; }
    public float Percentage { get; private set; }
    public int RemainingUses { get; private set; }
    public EntityType SourceType { get; private set; }

    public Buff(DamageType type, float percentage, int uses, EntityType source)
    {
        DamageType = type;
        Percentage = percentage;
        RemainingUses = uses;
        SourceType = source;
    }

    public void Consume()
    {
        RemainingUses--;
        if (RemainingUses <= 0)
        {
            Debug.Log($"Buff {DamageType} has expired.");
        }
    }
}
