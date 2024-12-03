using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseArcController : MonoBehaviour
{

    public ArcRenderer arcRenderer;
   
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        Plane plane = new Plane((Camera.main.gameObject.transform.position).normalized, arcRenderer.transform.position);

        if (!plane.Raycast(ray, out float enterDistance))
            return;

        arcRenderer.DrawBetweenPoints(arcRenderer.transform.position, ray.GetPoint(enterDistance));


    }
}
