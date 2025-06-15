using UnityEngine;

[RequireComponent(typeof(OrganismController))]
[RequireComponent(typeof(OrganismGenetics))]
[RequireComponent(typeof(OrganismEnergy))]
public class OrganismReproduce : MonoBehaviour
{
    [Header("Reproduction Settings")]
    [SerializeField] private GameObject organismPrefab;
    [SerializeField] private float reproductionCost = 100f;
    [SerializeField] private float reproductionRate = 0.1f;

    private OrganismController controller;
    private OrganismGenetics genetics;
    private OrganismEnergy energy;

    void Awake()
    {
        controller = GetComponent<OrganismController>();
        genetics = GetComponent<OrganismGenetics>();
        energy = GetComponent<OrganismEnergy>();
    }

    void Update()
    {
        if (controller.IsAlive)
        {
            TryToReproduce();
        }
    }

    void TryToReproduce()
    {
        if (CanReproduce())
        {
            energy.ModifyEnergy(-reproductionCost);
            Reproduce();
        }
    }

    bool CanReproduce()
    {
        float currentEnergy = energy.Energy;
        return currentEnergy >= reproductionCost && Random.value < reproductionRate;
    }

    void Reproduce()
    {
        Vector3 spawnPosition = transform.position + (Vector3)Random.insideUnitCircle * 2f;
        GameObject offspringObject = Instantiate(organismPrefab, spawnPosition, Quaternion.identity);

        OrganismController offspringController = offspringObject.GetComponent<OrganismController>();
        OrganismGenetics offspringGenetics = offspringObject.GetComponent<OrganismGenetics>();

        if (offspringController != null && offspringGenetics != null)
        {
            offspringGenetics.Inherit(genetics);
            offspringController.InitializeFromGenetics(offspringGenetics);
        }
    }
}
