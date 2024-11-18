using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy
{
    public string EnemyName;
    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public GameObject EnemyVisualPrefab;

}
