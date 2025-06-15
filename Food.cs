using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Properties")]
    public float energyValue = 10f;
    public float size = 0.5f;

    public FoodSpawner Spawner { get; set; }

    void Start()
    {
        transform.localScale = Vector3.one * size;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.blue;
        }
        try
        {
            gameObject.tag = "Food";
        }
        catch { }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Organism"))
        {
            OrganismController controller = other.GetComponent<OrganismController>();
            if (controller != null && controller.IsAlive)
            {
                Spawner.ReturnFoodToPool(this.gameObject);
            }
        }
    }

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = size * 0.5f;
        }
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}