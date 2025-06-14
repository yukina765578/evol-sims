using UnityEngine;

public class CircleMovement : MonoBehaviour
{
    [Header("Circle Settings")]
    public float speed = 10f;
    public float size = 5f;
    
    [Header("Boundary Reference")]
    public GameObject boundaryObject; // Reference to the boundary GameObject
    
    private BoundaryVisualizer boundary; // Reference to the BoundaryVisualizer script
    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;
    private float radius; // Cache the radius for performance
    
    void Start()
    {
        // Set direction to up
        moveDirection = Vector2.up;
        spriteRenderer = GetComponent<SpriteRenderer>();
        radius = size * 0.5f; // Calculate radius once
        
        // Get boundary reference
        if (boundaryObject != null)
        {
            boundary = boundaryObject.GetComponent<BoundaryVisualizer>();
            if (boundary == null)
            {
                Debug.LogError("BoundaryVisualizer component not found on the boundary object.");
            }
        }
        else
        {
            Debug.LogWarning("No boundary object assigned to CircleMovement script.");
        }
    }
    
    void Update()
    {
        Move();
    }
    
    void Move()
    {
        // Calculate next position
        Vector3 nextPosition = transform.position + (Vector3)(moveDirection * speed * Time.deltaTime);
        
        // Check if we would be outside boundary and handle collision
        if (boundary != null && WouldBeOutsideBoundary(nextPosition))
        {
            HandleBoundaryCollision(nextPosition);
        }
        
        // Move to the new position (collision handling adjusts moveDirection if needed)
        transform.position = transform.position + (Vector3)(moveDirection * speed * Time.deltaTime);
    }
    
    bool WouldBeOutsideBoundary(Vector3 position)
    {
        if (boundary == null) return false;
        
        // Check if any part of the circle would be outside the boundary
        return position.x + radius > boundary.width / 2 ||
               position.x - radius < -boundary.width / 2 ||
               position.y + radius > boundary.height / 2 ||
               position.y - radius < -boundary.height / 2;
    }
    
    void HandleBoundaryCollision(Vector3 nextPosition)
    {
        // Check horizontal boundaries
        if (nextPosition.x + radius > boundary.width / 2 || nextPosition.x - radius < -boundary.width / 2)
        {
            moveDirection.x = -moveDirection.x;
        }
        
        // Check vertical boundaries
        if (nextPosition.y + radius > boundary.height / 2 || nextPosition.y - radius < -boundary.height / 2)
        {
            moveDirection.y = -moveDirection.y;
        }
    }
    
    // Optional: Method to change direction manually
    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }
    
    // Optional: Method to update size and recalculate radius
    public void SetSize(float newSize)
    {
        size = newSize;
        radius = size * 0.5f;
        
        // Update the visual scale if using a sprite
        if (spriteRenderer != null)
        {
            transform.localScale = Vector3.one * size;
        }
    }
}
