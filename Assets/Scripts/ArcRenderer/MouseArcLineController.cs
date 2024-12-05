using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MouseArcLineController : MonoBehaviour
{
    
    public ArcLineRenderer arcRenderer;

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        Plane plane = new Plane((Camera.main.gameObject.transform.position).normalized, Vector3.zero);

        if (!plane.Raycast(ray, out float enterDistance))
            return;

        arcRenderer.DrawBetweenPoints(Vector3.zero, ray.GetPoint(enterDistance));
    }
}
