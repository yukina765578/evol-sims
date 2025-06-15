using UnityEngine;

public class OrganismGenetics : MonoBehaviour
{
    [Header("Genetic Traits")]
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float size = 1.0f;
    [SerializeField] private float sensingRadius = 5.0f;
    [SerializeField] private float maxLifeSpan = 100.0f;
    
    [Header("Trait Ranges")] // x = min, y = max
    [SerializeField] private Vector2 speedRange = new Vector2(0.5f, 10.0f);
    [SerializeField] private Vector2 sizeRange = new Vector2(0.5f, 5.0f);
    [SerializeField] private Vector2 sensingRadiusRange = new Vector2(1.0f, 10.0f);
    [SerializeField] private Vector2 maxLifeSpanRange = new Vector2(50.0f, 200.0f);
    
    [Header("Mutation Settings")]
    [SerializeField] private float mutationRate = 0.1f; // Probability of mutation
    [SerializeField] private float mutationStrength = 0.1f;
    
    // プロパティ
    public float Speed => speed;
    public float Size => size;
    public float SensingRadius => sensingRadius;
    public float MaxLifeSpan => maxLifeSpan;
    
    public void InitializeRandom()
    {
        speed = Random.Range(speedRange.x, speedRange.y);
        size = Random.Range(sizeRange.x, sizeRange.y);
        sensingRadius = Random.Range(sensingRadiusRange.x, sensingRadiusRange.y);
        maxLifeSpan = Random.Range(maxLifeSpanRange.x, maxLifeSpanRange.y);
    }
    
    public void Inherit(OrganismGenetics parent)
    {
        speed = MutateTrait(parent.speed, speedRange);
        size = MutateTrait(parent.size, sizeRange);
        sensingRadius = MutateTrait(parent.sensingRadius, sensingRadiusRange);
        maxLifeSpan = MutateTrait(parent.maxLifeSpan, maxLifeSpanRange);
    }
    
    private float MutateTrait(float traitValue, Vector2 range)
    {
        if (Random.value < mutationRate)
        {
            float mutation = Random.Range(-mutationStrength, mutationStrength) * traitValue;
            float newValue = traitValue + mutation;
            return Mathf.Clamp(newValue, range.x, range.y);
        }
        return traitValue;
    }
    
    public float CalculateEnergyConsumptionRate()
    {
        return (speed * 0.05f + size * 0.03f + sensingRadius * 0.01f) * 0.1f;
    }
    
    public float CalculateMaxEnergy(float baseEnergy)
    {
        return size * baseEnergy;
    }
    
    // Debugging method
    public override string ToString()
    {
        return $"Speed: {speed:F1}, Size: {size:F1}, Sensing: {sensingRadius:F1}, Lifespan: {maxLifeSpan:F0}";
    }
}
