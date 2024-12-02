using UnityEngine;

[CreateAssetMenu(menuName = "Cards/FrenzyData")]
public class FrenzyData : Card
{
    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>();
        var playerDeck = FindFirstObjectByType<PlayerDeck>();

        // Deal Blunt damage to caster
        cardEffect.DealDamage(caster, 3, DamageType.Blunt);

        // Draw 2 cards
        cardEffect.DrawCards(2, playerDeck);

        Debug.Log($"{cardName}: Caster took 3 damage and drew 2 cards");
    }

    public override void Upgrade()
    {
        // Upgrade logic here, if needed
    }
}
