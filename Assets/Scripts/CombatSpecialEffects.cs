using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class CombatSpecialEffects : ScriptableObject
{
    [SerializeField] protected Sprite BuffIconSprite;
    [SerializeField] protected int consumeCharge = 1;
    [SerializeField] protected bool stackable = false;

    public string effectName;

    public abstract CombatEffect CreateEffect();
}
