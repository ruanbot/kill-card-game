
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Attack")]
public class EnemyAttack : ScriptableObject
{
    public string attackName;
    public int damage;
    public float cooldown;
    public DamageType damageType;
    public string animationTrigger; // Add this for attack-specific animations
    public EnemyAttackSpecialEffects specialEffect;

    public void Execute(BattleEntities attacker, BattleEntities target)
    {
        // Play attack animation
        attacker.BattleVisuals?.PlaySpecificAttackAnimation(animationTrigger);

        // Deal damage
        float multiplier = target.GetBuffedDamageMultiplier(damageType);
        int adjustedDamage = Mathf.FloorToInt(damage * multiplier);
        int actualDamage = target.TakeDamage(adjustedDamage, damageType);

        Debug.Log($"{attackName}: Dealt {actualDamage} {damageType} damage to {target.Name}");

        // Apply special effects if any
        specialEffect?.ApplyEffect(target);
    }
}
