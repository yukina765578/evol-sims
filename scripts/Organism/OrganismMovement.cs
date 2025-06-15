
using UnityEngine;

[RequireComponent(typeof(OrganismGenetics))]
public class OrganismMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float directionChangeChance = 0.001f;

    [Header("Current State")]
    [SerializeField] private Vector2 moveDirection;

    private OrganismGenetics genetics;
    private BoundaryVisualizer boundary;
    private float radius; // Cache the radius for performance

    public Vector2 CurrentDirection => moveDirection;

    void Awake()
    {
        genetics = GetComponent<OrganismGenetics>()
    }

    void Update()
    {
        Move();
    }

    public void Initialize(BoundaryVisualizer boundaryVisualizer)
    {
        boundary = boundaryVisualizer;
        radius = genetics.Size * 0.5f; // update radius
        ChooseNewDirection();
    }

    public void Move()
    {
        Vector3 nextPosition = transform.position + (Vector3)(moveDirection * genetics.Speed * Time.deltaTime);

        if (CheckBoudnaryCollision(nextPosition))
        {
            HandleBoundaryCollision();
            nextPosition = transform.position + (Vector3)(moveDirection * genetics.Speed * Time.deltaTime);
        }
        else
        {
            if (Random.value < directionChangeChance)
            {
                ChooseNewDirection();
            }
        }

        transform.position = nextPosition;
    }

    bool CheckBoudnaryCollision(Vector3 position)
    {
        if (boundary == null) return false;

        float halfWidth = boundary.width * 0.5f - radius;
        float halfHeight = boundary.height * 0.5f - radius;

        return position.x < -halfWidth || position.x > halfWidth || position.y < -halfHeight || position.y > halfHeight;
    }

    void HandleBoundaryCollision(Vector3 nextPosition)
    {
        float halfWidth = boundary.width * 0.5f;
        float halfHeight = boundary.height * 0.5f;
        
        if (nextPosition.x + radius > halfWidth || nextPosition.x - radius < -halfWidth)
        {
            moveDirection.x = -moveDirection.x;
        }
        if (nextPosition.y + radius > halfHeight || nextPosition.y - radius < -halfHeight)
        {
            moveDirection.y = -moveDirection.y;
        }
    }

    public void ChooseNewDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }

    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }

    // Debbugging method
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw movement direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, moveDirection * genetics.SensingRadius);

        // Draw sensing radius
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, genetics.SensingRadius);
    }
}
