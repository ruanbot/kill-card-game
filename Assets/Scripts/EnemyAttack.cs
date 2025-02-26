
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Attack")]
public class EnemyAttack : ScriptableObject
{
    public string attackName;
    public int damage;
    public float cooldown;
    public DamageType damageType;
    public string animationTrigger; // Add this for attack-specific animations
    public CombatSpecialEffects specialEffect;

    public void Execute(BattleEntities attacker, BattleEntities target)
    {
        // Play attack animation
        attacker.BattleVisuals?.PlaySpecificAttackAnimation(animationTrigger);
 

        // Deal damage
        float multiplier = attacker.GetBuffedDamageMultiplier(damageType);
        int adjustedDamage = Mathf.FloorToInt(damage * multiplier);
        int actualDamage = target.TakeDamage(adjustedDamage, damageType);

        target.BattleVisuals?.ShowPopup(actualDamage, true);

        Debug.Log($"{attackName}: Dealt {actualDamage} {damageType} damage to {target.Name}");

        // Apply special effects if any
        if (specialEffect != null)
        {
            var effect = specialEffect.CreateEffect();
            if (effect != null)
            {
                target.ApplyEffect(effect);
                Debug.Log($"Applied {effect.EffectName} from {specialEffect.effectName}.");
            }
        }

    }
}
