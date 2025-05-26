using UnityEngine;

public class DrawCircleHelper : MonoBehaviour
{
    public float radius = 1f;
    public int segments = 60;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.renderQueue = 3000;
        lineRenderer.startColor = new Color(1f, 0f, 0f, 0.5f);
        lineRenderer.endColor = new Color(1f, 0f, 0f, 0.5f);
    }

    public void Draw(float radius)
    {
        this.radius = radius;
        Vector3[] positions = new Vector3[segments];
        float angle = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            positions[i] = new Vector3(x, 0.05f, z);
            angle += 360f / segments;
        }
        lineRenderer.SetPositions(positions);
    }
}