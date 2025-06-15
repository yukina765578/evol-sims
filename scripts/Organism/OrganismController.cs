using UnityEngine;

[RequireComponent(typeof(OrganismGenetics))]
[RequireComponent(typeof(OrganismMovement))]
[RequireComponent(typeof(OrganismEnergy))]
public class OrganismController : MonoBehaviour
{
    [Header("Current State")]
    [SerializeField] private float age = 0f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color lowEnergyColor = Color.red;

    [Header("Reproduction Settings")]
    [SerializeField] private GameObject organismPrefab;
    [SerializeField] private float reproductionCost = 100f;
    [SerializeField] private float reproductionRate = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Components
    private OrganismGenetics genetics;
    private OrganismMovement movement;
    private OrganismEnergy energy;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    
    // World References
    private BoundaryVisualizer boundary;
    private static BoundaryVisualizer cachedBoundary;
    
    // Properties
    public float Age => age;
    public bool IsAlive { get; private set; } = true;
    
    void Awake()
    {
        // Get required components
        genetics = GetComponent<OrganismGenetics>();
        movement = GetComponent<OrganismMovement>();
        energy = GetComponent<OrganismEnergy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        
        // Add sprite renderer if missing
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            // Set default circle sprite
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Circle");
            if (spriteRenderer.sprite == null)
            {
                // Create a default white circle sprite if not found
                GameObject tempCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spriteRenderer.sprite = tempCircle.GetComponent<SpriteRenderer>().sprite;
                Destroy(tempCircle);
            }
        }
        
        // Add collider if missing
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = false;
        }
        
        // Add Rigidbody2D if missing
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;
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
        Initialize();
    }
    
    void Update()
    {
        if (!IsAlive) return;
        UpdateAge();
        CheckDeath();
    }
    
    public void Initialize(bool useRandomGenetics = true)
    {
        if (useRandomGenetics)
        {
            genetics.InitializeRandom();
        }
        movement.Initialize(boundary);
        energy.Initialize();

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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(lowEnergyColor, healthyColor, energyPercentage);
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
        // Debug.Log($"{gameObject.name} died from {cause} at age {age:F1}");
        
        StartCoroutine(DeathAnimation());
    }
    
    System.Collections.IEnumerator DeathAnimation()
    {
        movement.enabled = false;
        energy.enabled = false;
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
        }
    }
    
    // Debug GUI
    // void OnGUI()
    // {
    //     if (!showDebugInfo || !IsAlive) return;
    //     
    //     // Convert world position to screen position
    //     Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
    //     screenPos.y = Screen.height - screenPos.y; // Flip Y coordinate
    //     
    //     // Create label style
    //     GUIStyle style = new GUIStyle(GUI.skin.label);
    //     style.fontSize = 10;
    //     style.normal.textColor = Color.white;
    //     style.alignment = TextAnchor.MiddleCenter;
    //     
    //     // Display info above organism
    //     string info = $"E: {energy.Energy:F0}/{energy.MaxEnergy:F0}\nA: {age:F0}/{genetics.MaxLifeSpan:F0}";
    //     GUI.Label(new Rect(screenPos.x - 40, screenPos.y - 50, 80, 30), info, style);
    // }
    
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
