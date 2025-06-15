using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Properties")]
    public float energyValue = 10f; // How much energy this food gives
    public float size = 0.5f; // Size of the food item
    
    void Start()
    {
        // Set the food size
        transform.localScale = Vector3.one * size;
        
        // Make sure the food has a blue color
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.blue; // Bright blue to distinguish from organisms
        }
        
        // Try to set tag, but don't break if it doesn't exist
        try
        {
            gameObject.tag = "Food";
        }
        catch
        {
            // Tag doesn't exist, that's okay
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if an organism touched this food (look for Organism component)
        
        if (other.CompareTag("Organism"))
        {
            // Destroy this food item
            Destroy(gameObject);
        }
    }
    
    // Add a CircleCollider2D as trigger for collision detection
    void Awake()
    {
        // Add collider if it doesn't exist
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = size * 0.5f;
        }
        
        // Add Rigidbody2D for collision detection (but make it kinematic so it doesn't fall)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Won't be affected by physics
        }
    }
}
