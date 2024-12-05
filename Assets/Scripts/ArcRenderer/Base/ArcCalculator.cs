using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;


public class ArcCalculator
{
   

    public static Vector3 GetLerpedPositionQuadratic(Vector3 firstPosition, Vector3 secondPosition, float delta)
    {
        float distance = Vector3.Distance(firstPosition, secondPosition) / 2;

        Vector3 p1up = firstPosition + Vector3.up * distance;
        Vector3 p2up = secondPosition + Vector3.up * distance;
        p1up = Vector3.Lerp(p1up, Vector3.Lerp(firstPosition, secondPosition, 0.5f), Mathf.Clamp01(1 - distance * 0.002f));
        p2up = Vector3.Lerp(p2up, Vector3.Lerp(firstPosition, secondPosition, 0.5f), Mathf.Clamp01(1 - distance * 0.002f));
        
        BezierCurve bezierCurve = new BezierCurve(firstPosition,p1up,p2up , secondPosition);

        return CurveUtility.EvaluatePosition(bezierCurve, delta);;
    }
    
    public static Vector3 GetLerpedPositionCubic(Vector3 firstPosition, Vector3 secondPosition, float delta)
    {
        Vector3 midPoint = CalculateMidPoint(firstPosition, secondPosition);
        return QuadraticBezierPoint(firstPosition, midPoint, secondPosition, delta);
    }

    public static Vector3 GetPointOnCubicBezierCurve(Vector3 firstPoint, Vector3 secondPoint, Vector3 firstAnchor, Vector3 secondAnchor, float time)
    {
        float term = 1 - time;
            
        float termSquared = term * term;
        float timeSquared = time * time;
            
        float termCubed = termSquared * term;
        float timeCubed = timeSquared * time;
        
        return firstPoint * termCubed + 
               secondPoint * (3 * termSquared * time) +
               firstAnchor * (3 * timeSquared * term) +
               secondAnchor * timeCubed;
    }

    public static Vector2[] DouglassPackerr2D(Vector2[] array)
    {
        List<Vector2> newList = array.ToList();

        bool isCleared = false;
        int index = 0;
        while (isCleared == false)
        {
            if (newList.Count <= 2 || index + 1 >= newList.Count)
                return newList.ToArray();
            
            if ((newList[index] + newList[index + 1]).magnitude < 0.1f)
            {
                newList.RemoveAt(index + 1);
            }
            else
            {
                index++;
            }

            if (index >= newList.Count)
            {
                isCleared = true;
            }
        }
        
        return newList.ToArray();
    }
    
    public static Vector3[] DouglassPackerr3D(Vector3[] array)
    {
        List<Vector3> newList = array.ToList();

        bool isCleared = false;
        int index = 0;
        while (isCleared == false)
        {
            if (newList.Count <= 2 || index + 1 >= newList.Count)
                return newList.ToArray();
            
            if ((newList[index] + newList[index + 1]).magnitude < 0.1f)
            {
                newList.RemoveAt(index + 1);
            }
            else
            {
                index++;
            }

            if (index >= newList.Count)
            {
                isCleared = true;
            }
        }
        
        return newList.ToArray();
    }
    
    public static Vector3 CalculateMidPoint(Vector3 start, Vector3 end)
    {
        Vector3 midpoint = (start + end) / 2;
        float arcHeight = Vector3.Distance(start, end) / 3f;
        midpoint.y += arcHeight;
        return midpoint;
    }

    public static Vector3 QuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = uu * start;
        point += 2 * u * t * control;
        point += tt * end;
        return point;
    }
    
}
