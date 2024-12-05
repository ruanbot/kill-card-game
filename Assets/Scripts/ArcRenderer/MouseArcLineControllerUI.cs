using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MouseArcLineControllerUI : MonoBehaviour
{
    
    public ArcLineRendererUI arcRenderer;

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        
        Vector3 rectScreenWorldPosition = arcRenderer.lineRenderer.rectTransform.TransformPoint(Vector3.zero);

        if (arcRenderer.lineRenderer.isUsingCameraCanvas)
        {
            rectScreenWorldPosition = arcRenderer.lineRenderer.canvasCamera.WorldToScreenPoint(rectScreenWorldPosition);
        }
        
        arcRenderer.DrawBetweenPoints(rectScreenWorldPosition, (Vector2)mousePosition );
    }
}