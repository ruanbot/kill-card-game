using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public void DealDamage(BattleEntities target, int damage, DamageType damageType)
    {
        float multiplier = target.GetBuffedDamageMultiplier(damageType); // Check if this returns the correct value
        int finalDamage = Mathf.FloorToInt(damage * multiplier);

        target.TakeDamage(finalDamage, damageType);
        Debug.Log($"Dealt {finalDamage} {damageType} damage to {target.Name} (Multiplier: {multiplier})");
    }


    public void Heal(BattleEntities target, int amount)
    {
        target.CurrentHealth += amount;
        target.CurrentHealth = Mathf.Min(target.CurrentHealth, target.MaxHealth);
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
