using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public abstract class ArcRenderer
{
    public int totalPoints = 10;
    public float stretchAmount = 1f;


    public abstract void DrawBetweenPoints(Vector3 firstPoint, Vector3 secondPoint);
}
