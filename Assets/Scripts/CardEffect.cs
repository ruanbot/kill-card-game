using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public void DealDamage(BattleEntities target, int damage, DamageType damageType)
    {
        float multiplier = target.GetBuffedDamageMultiplier(damageType); // Check if this returns the correct value
        int initialDamage = Mathf.FloorToInt(damage * multiplier);

        // Call TakeDamage and let it calculate the reduced damage after resistances
        int actualDamage = target.TakeDamage(initialDamage, damageType);

        // target.TakeDamage(initialDamage, damageType);
        target.BattleVisuals?.ShowPopup(actualDamage, true);
        Debug.Log($"Dealt {actualDamage} {damageType} damage to {target.Name} (Multiplier: {multiplier})");
    }


    public void Heal(BattleEntities target, int amount)
    {
        int previousHealth = target.CurrentHealth;
        target.CurrentHealth += amount;
        target.CurrentHealth = Mathf.Min(target.CurrentHealth, target.MaxHealth);
        int healedAmount = target.CurrentHealth - previousHealth;

        target.BattleVisuals?.ShowPopup(healedAmount, false);
        target.BattleVisuals.SyncWithEntity(target);
    }

    public void DrawCards(int count, PlayerDeck playerDeck)
    {
        for (int i = 0; i < count; i++)
        {
            playerDeck.DrawCard();
        }
    }
}
