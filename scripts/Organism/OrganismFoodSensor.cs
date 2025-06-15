using UnityEngine;

[RequireComponent(typeof(OrganismGenetics))]
[RequireComponent(typeof(OrganismMovement))]
public class OrganismFoodSensor : MonoBehaviour
{
    [Header("Food Detection Settings")]
    [SerializeField] private float foodCheckInterval = 0.5f;
    [SerializeField] private LayerMask foodLayerMask = -1; // All layers by default
    
    [Header("Current State")]
    [SerializeField] private GameObject targetFood = null;
    [SerializeField] private bool hasTargetFood = false;
    
    private OrganismGenetics genetics;
    private OrganismMovement movement;
    private float lastFoodCheckTime = 0f;
    
    // Properties
    public GameObject TargetFood => targetFood;
    public bool HasTargetFood => hasTargetFood && targetFood != null;
    public Vector2 DirectionToFood => HasTargetFood ? 
        (targetFood.transform.position - transform.position).normalized : Vector2.zero;
    
    void Awake()
    {
        genetics = GetComponent<OrganismGenetics>();
        movement = GetComponent<OrganismMovement>();
    }

    public void Initialize()
    {
        // 初期化処理があれば追加
        lastFoodCheckTime = 0f;
    }
    
    // Update()を削除し、代わりにManagedUpdate()を使用
    public void ManagedUpdate()
    {
        CheckForFood();
        UpdateMovementTarget();
    }
    
    void CheckForFood()
    {
        // Only check for NEW food at intervals to optimize performance
        if (Time.time - lastFoodCheckTime < foodCheckInterval)
        {
            // But still update direction to current target every frame
            return;
        }
            
        lastFoodCheckTime = Time.time;
        
        // Clear target if it no longer exists
        if (targetFood == null)
        {
            hasTargetFood = false;
        }
        
        // Find food within sensing radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position, 
            genetics.SensingRadius, 
            foodLayerMask
        );
        
        GameObject nearestFood = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider2D collider in colliders)
        {
            // Check if it's food (either by tag or Food component)
            if (collider.CompareTag("Food") || collider.GetComponent<Food>() != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestFood = collider.gameObject;
                }
            }
        }
        
        // Update target
        if (nearestFood != targetFood)
        {
            targetFood = nearestFood;
            hasTargetFood = targetFood != null;
            
            if (hasTargetFood)
            {
                // Debug.Log($"{gameObject.name} found food at distance {nearestDistance:F1}");
            }
        }
    }
    
    void UpdateMovementTarget()
    {
        if (HasTargetFood)
        {
            // Continuously update direction to food every frame for direct steering
            movement.SetDirection(DirectionToFood);
        }
        else
        {
            // Clear target if no food
            movement.ClearTarget();
        }
    }
    
    public void OnFoodConsumed()
    {
        // Call this when food is consumed to clear the target
        if (targetFood != null)
        {
            targetFood = null;
            hasTargetFood = false;
        }
    }
    
    public void ClearTarget()
    {
        targetFood = null;
        hasTargetFood = false;
    }
    
    // Debugging methods
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw sensing radius
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Yellow for food sensing
        Gizmos.DrawWireSphere(transform.position, genetics.SensingRadius);
        
        // Draw line to target food
        if (HasTargetFood)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetFood.transform.position);
            
            // Draw target food highlight
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetFood.transform.position, 0.5f);
        }
    }
    
    // Get info for debugging
    public string GetSensorInfo()
    {
        return $"Target Food: {(HasTargetFood ? "Yes" : "No")}, " +
               $"Sensing Radius: {genetics.SensingRadius:F1}";
    }
}