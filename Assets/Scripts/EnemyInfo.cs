using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Enemy")]
public class EnemyInfo : ScriptableObject
{
    public string EnemyName;
    public int Level;
    public int BaseHealth;
    public int CurrentHealth;
    public int MaxHealth;
    public int BaseStr;
    public int BaseInitiative;
    public GameObject EnemyVisualPrefab; // use in battle scene

}
