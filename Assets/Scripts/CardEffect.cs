using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public void DealDamage(BattleEntities attacker, BattleEntities target, int baseDamage, DamageType damageType)
    {
        // Triggered at start of card use
        attacker.TriggerEffects(EffectTriggerType.OnCardUse);

        // Keep base damage consistent
        int originalBaseDamage = baseDamage;

        // Apply Buff Multiplier First
        float buffMultiplier = attacker.GetBuffedDamageMultiplier(damageType);
        int boostedDamage = Mathf.FloorToInt(originalBaseDamage * buffMultiplier);

        // Add debug to verify the call
        Debug.Log($"Checking buffs for {attacker.Name} with damage type {damageType} and entity type {attacker.EntityType}");

        // Apply Debuff Multiplier Second
        float debuffMultiplier = target.GetDebuffMultiplier(damageType);
        int debuffedDamage = Mathf.FloorToInt(boostedDamage * debuffMultiplier);

        // Apply Resistance Last
        float resistance = target.Resistances.GetResistance(damageType);
        int finalDamage = Mathf.CeilToInt(debuffedDamage * (1 - resistance));

        // More detailed logging
        Debug.Log($"Damage Calculation: Base({originalBaseDamage}) * Buff({buffMultiplier}) * Debuff({debuffMultiplier}) * (1 - Resistance({resistance})) = {finalDamage}");

        // Apply Final Damage
        int actualDamage = target.TakeDamage(finalDamage, damageType);

        target.BattleVisuals?.ShowPopup(actualDamage, true);
        // Debug.Log($"[{attacker.Name} -> {target.Name}] Base: {originalBaseDamage}, " +
        //          $"Buff Multi: {buffMultiplier}, Debuff Multi: {debuffMultiplier}, " +
        //         $"Resistance: {resistance * 100}%, Final: {actualDamage}");

        // Change this to specifically trigger player attacks
        if (attacker.IsPlayer)
        {
            attacker.TriggerEffects(EffectTriggerType.OnPlayerAttack);
        }
        else
        {
            attacker.TriggerEffects(EffectTriggerType.OnEnemyAttack);
        }
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
