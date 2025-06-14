using UnityEngine;

public class BoundaryVisualizer : MonoBehaviour
{
    [Header("Boundary Settings")]
    public float width = 20f;
    public float height = 15f;

    private LineRenderer lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateVisualBoundary();
    }

    void CreateVisualBoundary()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.positionCount = 5; // 4 corners + 1 to close the loop
        lineRenderer.useWorldSpace = true;

        UpdateBoundary();
    }

    void UpdateBoundary()
    {
        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(-width / 2, -height / 2, 0); // Bottom left
        corners[1] = new Vector3(width / 2, -height / 2, 0);  // Bottom right
        corners[2] = new Vector3(width / 2, height / 2, 0);   // Top right
        corners[3] = new Vector3(-width / 2, height / 2, 0);  // Top left
        corners[4] = corners[0]; // Close the loop by repeating the first corner

        lineRenderer.SetPositions(corners);
    }
    public bool IsOutsideBoundary(Vector3 position)
    {
        // Check if the position is outside the defined boundary
        return position.x < -width / 2 || position.x > width / 2 ||
               position.y < -height / 2 || position.y > height / 2;
    }

    public Vector3 ClampToBoundary(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -width / 2, width / 2);
        position.y = Mathf.Clamp(position.y, -height / 2, height / 2);
        return position;
    }
}
