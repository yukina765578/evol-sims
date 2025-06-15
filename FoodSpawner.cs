using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food Spawning Settings")]
    public GameObject foodPrefab;
    public int maxFoodCount = 500;
    public float spawnRate = 0.1f; 
    public int initialFoodCount = 500;

    [Header("Boundary Reference")]
    public GameObject boundaryObject;
    public float edgeBuffer = 0.5f;

    private BoundaryVisualizer boundary;
    private float spawnTimer;

    // オブジェクトプールのためのリスト
    private List<GameObject> foodPool;
    private int activeFoodCount = 0;

    void Start()
    {
        if (boundaryObject != null)
        {
            boundary = boundaryObject.GetComponent<BoundaryVisualizer>();
        }
        else
        {
            Debug.LogError("No boundary object assigned to FoodSpawner!");
            return;
        }

        InitializePool();
        SpawnInitialFood();
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate && activeFoodCount < maxFoodCount)
        {
            SpawnFoodFromPool();
            spawnTimer = 0f;
        }
    }

    void InitializePool()
    {
        foodPool = new List<GameObject>();
        for (int i = 0; i < maxFoodCount; i++)
        {
            GameObject food = Instantiate(foodPrefab, Vector3.zero, Quaternion.identity, this.transform);
            food.GetComponent<Food>().Spawner = this;
            food.SetActive(false);
            foodPool.Add(food);
        }
    }

    void SpawnInitialFood()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFoodFromPool();
        }
    }

    void SpawnFoodFromPool()
    {
        foreach (GameObject food in foodPool)
        {
            if (!food.activeInHierarchy)
            {
                food.transform.position = GetRandomSpawnPosition();
                food.SetActive(true);
                activeFoodCount++;
                return;
            }
        }
    }

    public void ReturnFoodToPool(GameObject food)
    {
        food.SetActive(false);
        activeFoodCount--;
    }

    Vector2 GetRandomSpawnPosition()
    {
        float spawnWidth = boundary.width - (edgeBuffer * 2);
        float spawnHeight = boundary.height - (edgeBuffer * 2);
        
        return new Vector2(
            Random.Range(-spawnWidth / 2, spawnWidth / 2),
            Random.Range(-spawnHeight / 2, spawnHeight / 2)
        );
    }
}