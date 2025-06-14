using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food Spawning Settings")]
    public GameObject foodPrefab; // Drag your food prefab here
    public int maxFoodCount = 20; // Maximum food items at once
    public float spawnRate = 2f; // Time between spawns (seconds)
    public int initialFoodCount = 10; // Food to spawn at start
    
    [Header("Boundary Reference")]
    public GameObject boundaryObject; // Drag your boundary object here
    
    [Header("Spawn Options")]
    public float edgeBuffer = 0.5f; // Distance from boundary edges
    
    private BoundaryVisualizer boundary;
    private float lastSpawnTime;
    
    void Start()
    {
        // Get the boundary component
        if (boundaryObject != null)
        {
            boundary = boundaryObject.GetComponent<BoundaryVisualizer>();
            if (boundary == null)
            {
                Debug.LogError("BoundaryVisualizer component not found on the boundary object.");
                return;
            }
        }
        else
        {
            Debug.LogError("No boundary object assigned to FoodSpawner!");
            return;
        }

        // Spawn initial food
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFood();
        }
        
        lastSpawnTime = Time.time;
    }
    
    void Update()
    {
        // Check if we need to spawn more food
        if (Time.time - lastSpawnTime >= spawnRate)
        {
            int currentFoodCount = GameObject.FindGameObjectsWithTag("Food").Length;
            
            if (currentFoodCount < maxFoodCount)
            {
                SpawnFood();
                lastSpawnTime = Time.time;
            }
        }
    }
    
    void SpawnFood()
    {
        if (foodPrefab == null)
        {
            Debug.LogError("No food prefab assigned to FoodSpawner!");
            return;
        }
        
        Vector2 spawnPos = GetRandomSpawnPosition();
        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        // Random position within boundary bounds (minus edge buffer)
        float spawnWidth = boundary.width - (edgeBuffer * 2);
        float spawnHeight = boundary.height - (edgeBuffer * 2);
        
        Vector2 position = new Vector2(
            Random.Range(-spawnWidth/2, spawnWidth/2),
            Random.Range(-spawnHeight/2, spawnHeight/2)
        );
        
        return position;
    }
    
    // Optional: Manual spawn method for testing
    [ContextMenu("Spawn Food Now")]
    public void SpawnFoodNow()
    {
        SpawnFood();
    }
}
