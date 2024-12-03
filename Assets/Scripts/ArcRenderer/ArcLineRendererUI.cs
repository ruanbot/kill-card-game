using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArcLineRendererUI : ArcRenderer
{
    public UILineRenderer lineRenderer;
    

    public override void DrawBetweenPoints(Vector3 firstPoint, Vector3 secondPoint)
    {

        Vector2[] points = new Vector2[totalPoints];

        for (int i = 0; i < totalPoints; i++)
        {
            points[i] = ArcCalculator.GetLerpedPositionCubic(firstPoint, secondPoint, (1f / totalPoints) * i);
        }

        points = ArcCalculator.DouglassPackerr2D(points);
        lineRenderer.points = points;
    }
}