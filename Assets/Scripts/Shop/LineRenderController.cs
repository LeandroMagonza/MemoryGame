using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRenderController : MonoBehaviour
{
    public RectTransform startObject;
    public RectTransform target;
    public LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetLineRendererPositions();
    }


    void SetLineRendererPositions()
    {
        if (startObject != null && target != null && lineRenderer != null)
        {
            Vector3 startPoint = Camera.main.WorldToScreenPoint(startObject.position);
            Vector3 endPoint = Camera.main.WorldToScreenPoint(target.position);

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }
}
