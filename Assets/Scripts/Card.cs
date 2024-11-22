using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cards")]
public abstract class Card : ScriptableObject
{
    public string cardName;
    public string cardDescription;
    public Sprite artwork;
    public int manaCost;
    public string cardClass;
    public int cardCost;
    public string rarity;

    public TargetType targetType;

    public abstract void Use(BattleEntities caster, BattleEntities target);

    public virtual void Upgrade() { }


}

