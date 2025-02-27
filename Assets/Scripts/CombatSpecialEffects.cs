using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class CombatSpecialEffects : ScriptableObject
{
    [SerializeField] protected Sprite BuffIconSprite;
    [SerializeField] protected int consumeCharge = 1;
    [SerializeField] protected bool stackable = false;

    [Header("Trigger Settings")]
    [SerializeField] 
    [Tooltip("When should this effect trigger?")]
    protected EffectTriggerType triggerType = EffectTriggerType.OnPlayerAttack;  // Default to OnAttack

    public string effectName;

    public abstract CombatEffect CreateEffect();
    public virtual bool TriggerEffect(BattleEntities target, EffectTriggerType triggerType) => false;
}
