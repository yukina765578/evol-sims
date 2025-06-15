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

    public void Initialize()
    {
        // 初期化処理があれば追加
    }

    // Update()を削除し、代わりにManagedUpdate()を使用
    public void ManagedUpdate()
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
            float scaledReproductionCost = energy.MaxEnergy * 0.5f;
            energy.ModifyEnergy(-scaledReproductionCost);
            Reproduce();
        }
    }

    bool CanReproduce()
    {
        float maxEnergy = energy.MaxEnergy;
        float currentEnergy = energy.Energy;
        float reproductionThreshold = energy.MaxEnergy * 0.8f;

        return currentEnergy >= reproductionThreshold && Random.value < reproductionRate;
    }

    void Reproduce()
    {
        Debug.Log($"{gameObject.name} is reproducing!");
        Vector3 spawnPosition = transform.position + (Vector3)Random.insideUnitCircle * 2f;
        GameObject offspringObject = Instantiate(organismPrefab, spawnPosition, Quaternion.identity);

        OrganismController offspringController = offspringObject.GetComponent<OrganismController>();
        OrganismGenetics offspringGenetics = offspringObject.GetComponent<OrganismGenetics>();

        if (offspringController != null && offspringGenetics != null)
        {
            offspringGenetics.Inherit(genetics);
            offspringController.Initialize(false);
        }
    }
}