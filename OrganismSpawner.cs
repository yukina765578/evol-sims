using UnityEngine;

public class OrganismSpawner : MonoBehaviour
{
    public static OrganismSpawner Instance { get; private set; } // Singleton for easy access

    [Header("Spawning Settings")]
    public GameObject organismPrefab; // Your circle prefab goes here
    public int spawnCount = 10;
    
    [Header("Population Control")]
    public int maxPopulation = 100;
    private int currentPopulation = 0;

    [Header("Boundary Reference")]
    public GameObject boundaryObject; // Drag your boundary object here
    
    [Header("Spawn Options")]
    public float edgeBuffer = 1f; // Distance from boundary edges
    public float minDistance = 2f; // Minimum distance between organisms
    
    private BoundaryVisualizer boundary;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
            Debug.LogError("No boundary object assigned to spawner!");
            return;
        }

        SpawnOrganisms();
    }
    
    void SpawnOrganisms()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (currentPopulation >= maxPopulation) break;
            Vector2 spawnPos = FindValidSpawnPosition();
            GameObject org = Instantiate(organismPrefab, spawnPos, Quaternion.identity);
            // Do NOT call RegisterOrganism() here; let each organism register itself in its Awake/Start
        }
    }
    
    Vector2 FindValidSpawnPosition()
    {
        Vector2 position;
        int attempts = 0;
        
        do
        {
            // Random position within boundary bounds (minus edge buffer)
            float spawnWidth = boundary.width - (edgeBuffer * 2);
            float spawnHeight = boundary.height - (edgeBuffer * 2);
            
            position = new Vector2(
                Random.Range(-spawnWidth/2, spawnWidth/2),
                Random.Range(-spawnHeight/2, spawnHeight/2)
            );
            
            attempts++;
        }
        while (IsTooCloseToOthers(position) && attempts < 50);
        
        return position;
    }
    
    bool IsTooCloseToOthers(Vector2 position)
    {
        // Find all existing organisms (you might need to tag them)
        GameObject[] existingOrganisms = GameObject.FindGameObjectsWithTag("Organism"); // Use your organism tag
        
        foreach (GameObject organism in existingOrganisms)
        {
            // Skip the spawner itself and boundary
            if (organism == gameObject || organism == boundaryObject) continue;
            
            if (Vector2.Distance(position, organism.transform.position) < minDistance)
            {
                return true;
            }
        }
        return false;
    }

    // --- Population control methods ---
    public bool CanSpawn()
    {
        return currentPopulation < maxPopulation;
    }

    public void RegisterOrganism()
    {
        currentPopulation++;
    }

    public void UnregisterOrganism()
    {
        currentPopulation = Mathf.Max(0, currentPopulation - 1);
    }

    public int GetPopulation() => currentPopulation;
    public int GetMaxPopulation() => maxPopulation;

    public bool IsInsideBoundary(Vector2 position, float radius = 0f)
    {
        if (boundary == null) return true; // fallback: allow if no boundary

        float halfWidth = boundary.width / 2f - radius;
        float halfHeight = boundary.height / 2f - radius;

        return Mathf.Abs(position.x) <= halfWidth && Mathf.Abs(position.y) <= halfHeight;
    }
}
