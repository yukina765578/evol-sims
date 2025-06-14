using UnityEngine;

public class Organism: MonoBehaviour
{
    [Header("Genetic Traits")]
    public float speed = 1f; 
    public float size = 2f;
    public float sensingRadius = 3f;
    public float maxLifespan = 100f; 

    [Header("Current State")]
    public float energy = 50f;
    public float age = 0f;
    public float maxEnergy = 100f; 

    [Header("Boundary")]
    private GameObject boundaryObject; // Reference to the boundary GameObject
    private BoundaryVisualizer boundary; // Reference to the BoundaryVisualizer script

    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;
    private float radius; // Cache the radius for performance
    

    void Start()
    {
        boundaryObject = GameObject.Find("Boundary"); // Replace "Boundary" with your actual boundary object name
        if (boundaryObject != null)
        {
            boundary = boundaryObject.GetComponent<BoundaryVisualizer>();
        }
        // Initialize the genetic traits
        speed = Random.Range(0.5f, 10f);
        size = Random.Range(1f, 3f); 
        sensingRadius = Random.Range(1f, 5f); 
        maxLifespan = Random.Range(50f, 150f); 
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxEnergy = energy * size; 
        radius = size * 0.5f; // Calculate radius based on size
        UpdateColliderSize(); // Set collider size based on initial size
        ChooseNewDirection();
    }

    void Update()
    {
        Move();
        ConsumeEnergy();
        UpdateVisuals();
        Age();
        CheckDeath();
    }

        void Move()
    {
        // Calculate next position
        Vector3 nextPosition = transform.position + (Vector3)(moveDirection * speed * Time.deltaTime);
        
        // Check if we would be outside boundary and handle collision
        if (boundary != null && WouldBeOutsideBoundary(nextPosition))
        {
            HandleBoundaryCollision(nextPosition);
            transform.position = transform.position + (Vector3)(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            if (Random.value < 0.01f)
            {
                ChooseNewDirection();
            }
            transform.position = nextPosition;
        }
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

    void ChooseNewDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }

    void ConsumeEnergy()
    {
        float energyCost = (speed * 0.5f + sensingRadius * 0.1f + size * 0.2f) * Time.deltaTime;
        energy -= energyCost;
    }

    public void GainEnergy(float amount)
    {
        energy += amount;
        if (energy > maxEnergy)
        {
            energy = maxEnergy;
        }
    }

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = false; // Make it a solid collider
        }

        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;
        }

        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Organism";
        }
    }

    void UpdateColliderSize()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = size * 0.5f;
        }
    }

    void UpdateVisuals()
    {
        transform.localScale = Vector3.one * size;

        float energyRatio = energy / maxEnergy;
        spriteRenderer.color = Color.Lerp(Color.red, Color.green, energyRatio);
    }

    void Age()
    {
        age += Time.deltaTime;
    }

    void CheckDeath()
    {
        if (energy <= 0)
        {
            Die();
        }
        else if (age > maxLifespan)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}


