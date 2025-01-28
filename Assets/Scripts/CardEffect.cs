using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public void DealDamage(BattleEntities attacker, BattleEntities target, int baseDamage, DamageType damageType)
    {
        // Keep base damage consistent (don't let buffs alter it)
        int originalBaseDamage = baseDamage;

        // **Apply Buff Multiplier First (before resistance)**
        float multiplier = attacker.GetBuffedDamageMultiplier(damageType);
        int boostedDamage = Mathf.FloorToInt(originalBaseDamage * multiplier);

        // **Apply Resistance After Buff Multiplier**
        float resistance = target.Resistances.GetResistance(damageType);
        int finalDamage = Mathf.CeilToInt(boostedDamage * (1 - resistance));

        // **Apply Final Damage**
        int actualDamage = target.TakeDamage(finalDamage, damageType);

        target.BattleVisuals?.ShowPopup(actualDamage, true);
        Debug.Log($"[{attacker.Name} -> {target.Name}] Base Damage: {originalBaseDamage}, Buff Multiplier: {multiplier}, Boosted Damage: {boostedDamage}, Resistance Applied: {resistance * 100}%, Final Damage: {actualDamage}");

        attacker.ConsumeBuff(damageType);
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
