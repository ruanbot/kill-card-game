using UnityEngine;

[CreateAssetMenu(menuName = "Cards/FrenzyData")]
public class FrenzyData : Card
{

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        var cardEffect = FindFirstObjectByType<CardEffect>();
        var playerDeck = FindFirstObjectByType<PlayerDeck>();

        cardEffect.DealDamage(caster, 3);
        cardEffect.DrawCards(2, playerDeck);

        Debug.Log($"{cardName}: Caster took 3 damage and drew 2 cards");
        
    }

    public override void Upgrade()
    {

    }
}
