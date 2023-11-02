using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRenderController : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = transform.childCount;
        for (int i = 0; i < transform.childCount; i++)
        {
            lineRenderer.SetPosition(i, transform.GetChild(i).position);
        }
    }
}
