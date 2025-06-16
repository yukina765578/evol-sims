using UnityEngine;

[RequireComponent(typeof(OrganismGenetics))]
[RequireComponent(typeof(OrganismMovement))]
[RequireComponent(typeof(OrganismEnergy))]
[RequireComponent(typeof(OrganismFoodSensor))]
[RequireComponent(typeof(OrganismReproduce))]
public class OrganismController : MonoBehaviour
{
    [Header("Current State")]
    [SerializeField] private float age = 0f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color lowEnergyColor = Color.red;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Components
    private OrganismGenetics genetics;
    private OrganismMovement movement;
    private OrganismEnergy energy;
    private OrganismFoodSensor foodSensor;
    private OrganismReproduce reproduce;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private OrganismVisual organismVisual; // Add this line
    
    // World References
    private BoundaryVisualizer boundary;
    private static BoundaryVisualizer cachedBoundary;
    
    // Properties
    public float Age => age;
    public bool IsAlive { get; private set; } = true;
    private bool IsInitialized = false;
    
    void Awake()
    {
        // Get required components
        genetics = GetComponent<OrganismGenetics>();
        movement = GetComponent<OrganismMovement>();
        energy = GetComponent<OrganismEnergy>();
        foodSensor = GetComponent<OrganismFoodSensor>();
        reproduce = GetComponent<OrganismReproduce>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        organismVisual = GetComponent<OrganismVisual>();

        // Register with spawner
        OrganismSpawner.Instance?.RegisterOrganism();
        
        // Add sprite renderer if missing
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            // Set default circle sprite
            GameObject tempCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spriteRenderer.sprite = tempCircle.GetComponent<SpriteRenderer>().sprite;
            Destroy(tempCircle);
        }
        
        // Add collider if missing
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = false;  // Keep solid for collisions
        }
        
        // Add Rigidbody2D if missing - CHANGED FOR PHYSICS-BASED COLLISIONS
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;  // Changed from Kinematic
            rb.linearDamping = 10f;  // High damping to prevent sliding
            rb.angularDamping = 10f;  // Prevent spinning
            rb.freezeRotation = true;  // No rotation
            rb.gravityScale = 0f;  // No gravity
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // Better collision detection
        }
        
        // Set tag
        try
        {
            gameObject.tag = "Organism";
        }
        catch
        {
            Debug.LogWarning("Organism tag not found. Please add it in Tags & Layers.");
        }
        
        // Get boundary reference
        if (cachedBoundary == null)
        {
            GameObject boundaryObject = GameObject.Find("Boundary");
            if (boundaryObject != null)
            {
                cachedBoundary = boundaryObject.GetComponent<BoundaryVisualizer>();
            }
            else
            {
                Debug.LogWarning("Boundary object not found in scene!");
            }
        }
        boundary = cachedBoundary;
        
        // Subscribe to energy events
        if (energy != null)
        {
            energy.OnEnergyDepleted.AddListener(OnEnergyDepleted);
            energy.OnEnergyChanged.AddListener(OnEnergyChanged);
            energy.OnCriticalEnergy.AddListener(OnCriticalEnergy);
        }
    }
    
    void Start()
    {
        if (!IsInitialized)
        {
            Initialize(true);
        }
    }
    
    void Update()
    {
        if (!IsAlive) return;
        // Debug.Log($"{gameObject.name} Age: {age:F1}, Energy: {energy.Energy}, Reproduce: {energy.MaxEnergy * 0.8f}");
        UpdateAge();
        foodSensor.ManagedUpdate();
        movement.ManagedUpdate();
        energy.ManagedUpdate();
        reproduce.ManagedUpdate();
        CheckDeath();
    }
    
    public void Initialize(bool useRandomGenetics = true)
    {
        if (IsInitialized) return;
        IsInitialized = true;
        if (useRandomGenetics)
        {
            genetics.InitializeRandom();
            // log genetics
            Debug.Log($"Initialized {gameObject.name} with random genetics: {genetics.ToString()}");
        }
        else
        {
            Debug.Log("Using inherited genetics from parent organism.");
        }

        age = 0f;
        
        // 各コンポーネントの初期化
        movement.Initialize(boundary);
        energy.Initialize();
        foodSensor.Initialize();
        reproduce.Initialize();

        UpdateSize();
        OnEnergyChanged(energy.EnergyPercentage);
    }
    
    void UpdateAge()
    {
        age += Time.deltaTime;
    }
    
    void UpdateSize()
    {
        transform.localScale = Vector3.one * genetics.Size;
        
        if (circleCollider != null)
        {
            circleCollider.radius = 0.5f; // Base sprite is 1 unit
        }
        
        movement.UpdateRadius();
        energy.UpdateMaxEnergy();
    }
    
    void OnEnergyChanged(float energyPercentage)
    {
        Color lerpedColor = Color.Lerp(lowEnergyColor, healthyColor, energyPercentage);
        if (organismVisual != null)
        {
            organismVisual.SetOrganismColor(lerpedColor);
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = lerpedColor;
        }
    }
    
    void OnEnergyDepleted()
    {
        Die("Starvation");
    }
    
    void OnCriticalEnergy()
    {
        // Debug.Log($"{gameObject.name} is in critical energy state!");
    }
    
    void CheckDeath()
    {
        if (age >= genetics.MaxLifeSpan)
        {
            Die("Old Age");
        }
    }
    
    void Die(string cause)
    {
        if (!IsAlive) return;

        IsAlive = false;
        Debug.Log($"{gameObject.name} died from {cause} at age {age:F1}");

        // Unregister from spawner
        OrganismSpawner.Instance?.UnregisterOrganism();

        StartCoroutine(DeathAnimation());
    }
    
    System.Collections.IEnumerator DeathAnimation()
    {
        // Disable all components
        movement.enabled = false;
        energy.enabled = false;
        foodSensor.enabled = false;
        reproduce.enabled = false;
        circleCollider.enabled = false;
        
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Shrink
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            
            // Fade
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = newColor;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // Food detection
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;
        
        // Check if it's food
        Food food = other.GetComponent<Food>();
        if (food != null)
        {
            // Gain energy from food
            energy.ModifyEnergy(food.energyValue);
            foodSensor.OnFoodConsumed();
        }
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw sensing radius
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, genetics.SensingRadius);
        
        // Draw organism info
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, 
            $"{gameObject.name}\n{genetics.ToString()}\n{energy.GetEnergyInfo()}");
    }
}
