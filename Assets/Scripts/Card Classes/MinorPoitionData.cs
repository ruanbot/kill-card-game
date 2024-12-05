using UnityEngine;

[CreateAssetMenu(menuName = "Cards/MinorPotionData")]
public class MinorPotionData : Card
{
    public MinorPotionData()
    {
        targetType = TargetType.Friendly;
    }

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>(); // reference the card effect script

        cardEffect.Heal(caster, 2);

        Debug.Log($"{cardName}: Caster heals for 2 HP.");

    }

    public override void Upgrade()
    {

    }
}
