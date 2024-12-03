using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MouseArcDotControllerUI : MonoBehaviour
{

    public ArcDotRendererUI arcRenderer;
    private RectTransform _rectTransform;
    
    [Header("Camera Canvas")]
    public bool isUsingCameraCanvas = true;
    public Camera cameraUI;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        cameraUI = GameObject.Find("UICamera").GetComponent<Camera>();
        Init();
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;


        if (isUsingCameraCanvas)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, mousePosition, cameraUI, out Vector2 localPoint);
            arcRenderer.DrawBetweenPoints(Vector3.zero, localPoint);
        }
        else
        {
            arcRenderer.DrawBetweenPoints(Vector3.zero, (Vector2)mousePosition );
        }
        
        
    }
    
    public void Init()
    {
        arcRenderer.arrowInstance = Instantiate(arcRenderer.arrowPrefab, transform);
        arcRenderer.arrowInstance.anchoredPosition = Vector3.zero;
        InitializeDotPool(arcRenderer.poolSize);
    }
    void InitializeDotPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            RectTransform dot = Instantiate(arcRenderer.dotPrefab, Vector3.zero, Quaternion.identity, transform);
            dot.anchoredPosition3D = Vector3.zero;
            dot.gameObject.SetActive(false);
            arcRenderer.dotPool.Add(dot);
        }
    }
}