using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ArcDotControllerUI : MonoBehaviour
{

    public ArcDotRendererUI arcRenderer;
    private RectTransform _rectTransform;
    
    [Header("Camera Canvas")]
    public bool isUsingCameraCanvas = true;
    public Camera cameraUI;

    private Vector3 _worldTarget;
    private bool _isOnMouse;
    
    //Line
    [SerializeField] private bool _isUsingLine;
    public ArcLineRendererUI arcLineRendererUI;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        cameraUI = GameObject.Find("UICamera").GetComponent<Camera>();
        _isOnMouse = true;
        Init();
    }

    private void OnEnable()
    {
        if(!_isUsingLine)
            arcRenderer.arrowInstance.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        for (int i = 0; i < arcRenderer.poolSize; i++)
        {
            arcRenderer.dotPool[i].gameObject.SetActive(false);
        }
        
        arcRenderer.arrowInstance.gameObject.SetActive(false);
        
        //Line

        arcLineRendererUI.lineRenderer.points = Array.Empty<Vector2>();
    }

    void Update()
    {
        Vector3 target = Vector3.zero;

        if (_isOnMouse)
        {
            target = Input.mousePosition;
        }
        else
        {
            target = cameraUI.WorldToScreenPoint(_worldTarget);
        }


        if (!_isUsingLine)
        {
            if (isUsingCameraCanvas)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, target, cameraUI, out Vector2 localPoint);
                arcRenderer.DrawBetweenPoints(Vector3.zero, localPoint);
            }
            else
            {
                arcRenderer.DrawBetweenPoints(Vector3.zero, (Vector2)target );
            }
        }
        else
        {
            if (isUsingCameraCanvas)
            {
                Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(cameraUI, _rectTransform.TransformPoint(Vector3.zero - new Vector3(0,50,0)));
                arcLineRendererUI.DrawBetweenPoints(screenPoint, target);
            }
            else
            {
                arcLineRendererUI.DrawBetweenPoints(Vector3.zero, (Vector2)target );
            }
        }
        
       
    }

    public void SetTarget(Vector3 target)
    {
        _isOnMouse = false;
        _worldTarget = target;
    }

    public void DeselectTarget()
    {
        _isOnMouse = true;
    }
    
    public void Init()
    {
        arcRenderer.arrowInstance = Instantiate(arcRenderer.arrowPrefab, transform);
        arcRenderer.arrowInstance.anchoredPosition = Vector3.zero;
        arcRenderer.arrowInstance.gameObject.SetActive(false);
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