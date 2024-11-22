using UnityEngine;

[CreateAssetMenu(menuName = "Cards/FrenzyData")]
public class FrenzyData : Card
{
    public int Damage = 3;

    public override void Use(BattleEntities caster, BattleEntities target)
    {
        // Behavior for Frenzy card:

        // Update visuals
        if (target.EntityType == EntityType.Self)
        {
            Debug.Log($"Valid target for {cardName}: {target.Name}, applying {Damage} damage.");
            target.CurrentHealth -= Damage;
            target.BattleVisuals.UpdateHPDisplay();
            target.BattleVisuals?.PlayHitAnimation();
            
        }

        Debug.Log($"{cardName} used: {Damage} damage to {target.Name}");
    }

    public override void Upgrade()
    {
        Damage += 1;
        Debug.Log($"{cardName} upgraded: Damage = {Damage}");
    }
}
