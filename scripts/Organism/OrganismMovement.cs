using UnityEngine;

[RequireComponent(typeof(OrganismGenetics))]
public class OrganismMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float directionChangeChance = 0.001f;
    [SerializeField] private float steeringForce = 1f; // How strongly to steer towards targets

    [Header("Current State")]
    [SerializeField] private Vector2 moveDirection;
    [SerializeField] private Vector2 targetDirection = Vector2.zero;
    [SerializeField] private bool hasTarget = false;

    private OrganismGenetics genetics;
    private BoundaryVisualizer boundary;
    private float radius; // Cache the radius for performance

    public Vector2 CurrentDirection => moveDirection;
    public bool HasTarget => hasTarget;

    void Awake()
    {
        genetics = GetComponent<OrganismGenetics>();
    }

    public void UpdateRadius()
    {
        radius = genetics.Size * 0.5f;
    }

    void Update()
    {
        Move();
    }

    public void Initialize(BoundaryVisualizer boundaryVisualizer)
    {
        boundary = boundaryVisualizer;
        radius = genetics.Size * 0.5f; // Update radius based on size
        ChooseNewDirection();
    }

    public void Move()
    {
        Vector2 desiredDirection = moveDirection;
        
        // If we have a target (like food), steer towards it
        if (hasTarget)
        {
            // Blend current direction with target direction for smooth steering
            desiredDirection = Vector2.Lerp(moveDirection, targetDirection, steeringForce * Time.deltaTime);
            desiredDirection = desiredDirection.normalized;
        }
        else
        {
            // Random direction changes when wandering
            if (Random.value < directionChangeChance)
            {
                ChooseNewDirection();
            }
        }

        // Calculate next position
        Vector3 nextPosition = transform.position + (Vector3)(desiredDirection * genetics.Speed * Time.deltaTime);

        // Handle boundary collisions
        if (CheckBoundaryCollision(nextPosition))
        {
            HandleBoundaryCollision(nextPosition);
            // Recalculate next position after boundary collision
            nextPosition = transform.position + (Vector3)(moveDirection * genetics.Speed * Time.deltaTime);
        }
        else
        {
            // Update movement direction if no collision
            moveDirection = desiredDirection;
        }

        transform.position = nextPosition;
    }

    bool CheckBoundaryCollision(Vector3 position)
    {
        if (boundary == null) return false;

        float halfWidth = boundary.width * 0.5f - radius;
        float halfHeight = boundary.height * 0.5f - radius;

        return position.x < -halfWidth || position.x > halfWidth || 
               position.y < -halfHeight || position.y > halfHeight;
    }

    void HandleBoundaryCollision(Vector3 nextPosition)
    {
        float halfWidth = boundary.width * 0.5f;
        float halfHeight = boundary.height * 0.5f;
        
        // Reflect direction off boundaries
        if (nextPosition.x + radius > halfWidth || nextPosition.x - radius < -halfWidth)
        {
            moveDirection.x = -moveDirection.x;
            targetDirection.x = -targetDirection.x; // Also reflect target direction
        }
        if (nextPosition.y + radius > halfHeight || nextPosition.y - radius < -halfHeight)
        {
            moveDirection.y = -moveDirection.y;
            targetDirection.y = -targetDirection.y; // Also reflect target direction
        }
        
        // Clear target after boundary collision to avoid getting stuck
        ClearTarget();
    }

    public void ChooseNewDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }

    public void SetDirection(Vector2 newDirection)
    {
        targetDirection = newDirection.normalized;
        hasTarget = true;
    }
    
    public void ClearTarget()
    {
        hasTarget = false;
        targetDirection = Vector2.zero;
    }

    // Allow external control of steering force
    public void SetSteeringForce(float force)
    {
        steeringForce = Mathf.Clamp01(force);
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw current movement direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, moveDirection * 2f);

        // Draw target direction if we have one
        if (hasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, targetDirection * 2f);
        }

        // Draw organism boundary
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}