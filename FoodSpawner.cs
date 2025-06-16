using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food Spawning Settings")]
    public GameObject foodPrefab;
    public int maxFoodCount = 500;
    public float spawnRate = 1.0f; // 1秒ごとにバッチ生成するようにデフォルト値を変更
    public int initialFoodCount = 100; // 初期生成数を調整

    // ★★★ 変更点1: 一度に生成する数を設定する変数を追加 ★★★
    [Tooltip("一度に生成するエサの数")]
    public int spawnBatchSize = 10;

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

        // spawnRateで設定した時間が経過したら実行
        if (spawnTimer >= spawnRate)
        {
            spawnTimer = 0f; // タイマーをリセット
            
            // Count actual active food instead of relying on counter
            int actualActiveFood = 0;
            foreach (GameObject food in foodPool)
            {
                if (food.activeInHierarchy) actualActiveFood++;
            }
         
            // Fix any discrepancies
            activeFoodCount = actualActiveFood;

            // spawnBatchSizeの数だけエサを生成するループ
            int spawned = 0;
            for (int i = 0; i < spawnBatchSize; i++)
            {
                // ただし、最大数を超えないようにチェック
                if (activeFoodCount < maxFoodCount)
                {
                    if (SpawnFoodFromPool())
                    {
                        spawned++;
                    }
                }
                else
                {
                    // 最大数に達したらループを抜ける
                    break;
                }
            }
        
        }
    }

    void InitializePool()
    {
        foodPool = new List<GameObject>();
        for (int i = 0; i < maxFoodCount; i++)
        {
            GameObject food = Instantiate(foodPrefab, Vector3.zero, Quaternion.identity, this.transform);
            // FoodコンポーネントにSpawnerの参照を渡す
            Food foodComponent = food.GetComponent<Food>();
            if (foodComponent != null)
            {
                foodComponent.Spawner = this;
            }
            food.SetActive(false);
            foodPool.Add(food);
        }
    }

    void SpawnInitialFood()
    {
        int count = Mathf.Min(initialFoodCount, maxFoodCount);
        for (int i = 0; i < count; i++)
        {
            SpawnFoodFromPool();
        }
    }

    bool SpawnFoodFromPool()
    {
        foreach (GameObject food in foodPool)
        {
            if (!food.activeInHierarchy)
            {
                food.transform.position = GetRandomSpawnPosition();
                food.SetActive(true);
                activeFoodCount++;
                return true;
            }
        }
        
        Debug.LogWarning("Could not find inactive food in pool!");
        return false;
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
