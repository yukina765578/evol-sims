using UnityEngine;

[RequireComponent(typeof(OrganismController))]
[RequireComponent(typeof(OrganismGenetics))]
[RequireComponent(typeof(OrganismEnergy))]
public class OrganismReproduce : MonoBehaviour
{
    [Header("Reproduction Settings")]
    [SerializeField] private GameObject organismPrefab;
    [SerializeField] private float reproductionRate = 0.1f;

    private OrganismController controller;
    private OrganismGenetics genetics;
    private OrganismEnergy energy;

    // Threshold and size settings (adjust as needed)
    private float minThreshold = 0.6f;
    private float maxThreshold = 0.8f;
    private float minSize = 1f;
    private float maxSize = 3f;

    void Awake()
    {
        controller = GetComponent<OrganismController>();
        genetics = GetComponent<OrganismGenetics>();
        energy = GetComponent<OrganismEnergy>();
    }

    public void Initialize()
    {
        // Add any initialization logic if needed
    }

    public void ManagedUpdate()
    {
        if (controller.IsAlive)
        {
            TryToReproduce();
        }
    }

    void TryToReproduce()
    {
        float sizeT = Mathf.InverseLerp(minSize, maxSize, genetics.Size);
        float thresholdMultiplier = Mathf.Lerp(maxThreshold, minThreshold, sizeT);
        float reproductionThreshold = energy.MaxEnergy * thresholdMultiplier;

        if (CanReproduce(reproductionThreshold) && OrganismSpawner.Instance != null && OrganismSpawner.Instance.CanSpawn())
        {
            // Cost scales with lifespan and size
            float baseCostMultiplier = Mathf.Lerp(0.2f, 0.8f, Mathf.InverseLerp(genetics.MaxLifeSpanRange.x, genetics.MaxLifeSpanRange.y, genetics.MaxLifeSpan));
            float sizeCostMultiplier = Mathf.Lerp(1f, 2f, sizeT);

            float intendedCost = energy.MaxEnergy * baseCostMultiplier * sizeCostMultiplier;
            // Clamp so at least 20% energy remains after reproduction
            float maxAllowedCost = energy.MaxEnergy * (thresholdMultiplier - 0.2f);
            float scaledReproductionCost = Mathf.Min(intendedCost, maxAllowedCost);

            energy.ModifyEnergy(-scaledReproductionCost);
            Reproduce();
        }
    }

    bool CanReproduce(float reproductionThreshold)
    {
        float currentEnergy = energy.Energy;
        return currentEnergy >= reproductionThreshold && Random.value < reproductionRate;
    }

    void Reproduce()
    {
        // Calculate intended spawn position
        Vector2 spawnOffset = Random.insideUnitCircle * 2f;
        Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

        // Get radius from genetics (assuming Size is diameter, so radius = Size / 2)
        float radius = genetics.Size * 0.5f;

        // Check if inside boundary before spawning
        if (OrganismSpawner.Instance != null && !OrganismSpawner.Instance.IsInsideBoundary(spawnPosition, radius))
        {
            Debug.Log("Reproduction blocked: spawn position outside boundary (size considered).");
            return;
        }

        GameObject offspringObject = Instantiate(organismPrefab, spawnPosition, Quaternion.identity);

        OrganismController offspringController = offspringObject.GetComponent<OrganismController>();
        OrganismGenetics offspringGenetics = offspringObject.GetComponent<OrganismGenetics>();

        if (offspringController != null && offspringGenetics != null)
        {
            offspringGenetics.Inherit(genetics);
            offspringController.Initialize(false);

            // Debug: log offspring's initial age
            Debug.Log($"Offspring initial age: {offspringController.Age:F2}");
        }
    }
}
