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

    /// <summary>
    /// Calculate damage without playing animations or applying effects.
    /// Animations are now handled by CombatActionQueue.
    /// </summary>
    public int CalculateDamage(BattleEntities attacker)
    {
        float multiplier = attacker.GetBuffedDamageMultiplier(damageType);
        return Mathf.FloorToInt(damage * multiplier);
    }

    /// <summary>
    /// Apply special effects (bleed, etc.) to the target.
    /// </summary>
    public void ApplySpecialEffects(BattleEntities target)
    {
        if (specialEffect != null)
        {
            var effect = specialEffect.CreateEffect();
            if (effect != null)
            {
                target.ApplyEffect(effect);
                Debug.Log($"Applied {effect.EffectName} from {attackName}.");
            }
        }
    }
}
