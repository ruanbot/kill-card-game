using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cards")]
public class Card : ScriptableObject
{
    public int id;
    public string cardName;
    public string cardDescription;
    public Sprite artwork;
    public int manaCost;
    public string cardClass;
    public int cardCost;

    public int power;

    public TargetType targetType;

    public int drawXcards;
    public int addXmaxMana;
    public int addMana;

    public int returnXcards;
    public int healXhealth;

    public string attackAnim;





}

