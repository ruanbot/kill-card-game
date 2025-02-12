using UnityEngine;

[System.Serializable]
public abstract class EnemyAttackSpecialEffects : ScriptableObject
{
    public abstract void ApplyEffect(BattleEntities target);
}
