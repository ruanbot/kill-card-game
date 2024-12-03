using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArcDotRendererUI : ArcRenderer
{
    
    public RectTransform arrowPrefab;
    public RectTransform dotPrefab;
    public int poolSize = 50;
    [HideInInspector]public List<RectTransform> dotPool = new List<RectTransform>();
    [HideInInspector]public RectTransform arrowInstance;
    public float spacing = 50;
    public float arrowAngleAdjustment = 0;
    public Vector2 arrowPositionOffset;
    public int dotsToSkip = 1;
    private Vector3 arrowDirection;
    void UpdateArc(Vector3 start, Vector3 end)
    {
        int numDots = Mathf.CeilToInt(Vector3.Distance(start, end) / spacing);

        for (int i = 0; i < numDots && i < dotPool.Count; i++)
        {
            float t = i / (float)numDots;
            t = Mathf.Clamp(t, 0f, 1f);

            Vector3 position = ArcCalculator.GetLerpedPositionQuadratic(start,end,t);

            if (i != numDots - dotsToSkip)
            {
                dotPool[i].anchoredPosition = position;
                dotPool[i].gameObject.SetActive(true);
            }
            if (i == numDots - (dotsToSkip + 1) && i - dotsToSkip + 1 >= 0)
            {
                arrowDirection = dotPool[i].anchoredPosition;
            }
        }

        // Deactive unused dots
        for (int i = numDots - dotsToSkip; i < dotPool.Count; i++)
        {
            if (i > 0)
            {
                dotPool[i].gameObject.SetActive(false);
            }
        }
    }

    void PositionAndRotateArrow(Vector3 position)
    {
        arrowInstance.anchoredPosition = (Vector2)position + arrowPositionOffset;
        Vector3 direction = arrowDirection - position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += arrowAngleAdjustment;
        arrowInstance.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    public override void DrawBetweenPoints(Vector3 firstPoint, Vector3 secondPoint)
    {
        UpdateArc(firstPoint,secondPoint);
        PositionAndRotateArrow(secondPoint);
    }

    
}
    
