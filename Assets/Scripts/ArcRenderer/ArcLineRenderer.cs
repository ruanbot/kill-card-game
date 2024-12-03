using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArcLineRenderer : ArcRenderer
{
    public LineRenderer lineRenderer;
    

    public override void DrawBetweenPoints(Vector3 firstPoint, Vector3 secondPoint)
    {
        lineRenderer.textureScale = new Vector2(Vector3.Distance(firstPoint, secondPoint) * stretchAmount, lineRenderer.textureScale.y);
        
        Vector3[] points = new Vector3[totalPoints];

        for (int i = 0; i < totalPoints; i++)
        {
            points[i] = ArcCalculator.GetLerpedPositionCubic(firstPoint, secondPoint, (1f / totalPoints) * i);
        }

        points = ArcCalculator.DouglassPackerr3D(points);
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
