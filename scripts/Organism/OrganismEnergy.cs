using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OrganismGenetics))]
public class OrganismEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private float startingEnergy = 50.0f;
    [SerializeField] private float baseEnergyConsumptionRate = 0.001f;

    [Header("Current State")]
    [SerializeField] private float currentEnergy;
    [SerializeField] private float maxEnergy;

    [Header("Energy Thresholds")]
    [SerializeField] private float criticalEnergyThreshold = 10f;

    [Header("Events")]
    public UnityEvent<float> OnEnergyChanged = new UnityEvent<float>();
    public UnityEvent OnEnergyDepleted = new UnityEvent();
    public UnityEvent OnCriticalEnergy = new UnityEvent();
    public UnityEvent OnReproductionReady = new UnityEvent();

    private OrganismGenetics genetics;

    public float Energy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyPercentage => currentEnergy / maxEnergy;
    public bool IsEnergyDepleted => currentEnergy <= 0f;
    public bool IsEnergyCritical => currentEnergy <= criticalEnergyThreshold;

    void Awake()
    {
        genetics = GetComponent<OrganismGenetics>();
    }

    public void Initialize()
    {
        maxEnergy = genetics.CalculateMaxEnergy(startingEnergy);
        currentEnergy = maxEnergy * 0.5f;
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    // Update()を削除し、代わりにManagedUpdate()を使用
    public void ManagedUpdate()
    {
        ConsumeEnergy();
    }

    void ConsumeEnergy()
    {
        float energyCost = CalculateEnergyConsumption();
        ModifyEnergy(-energyCost);
    }

    float CalculateEnergyConsumption()
    {
        float geneticCost = genetics.CalculateEnergyConsumptionRate();
        return geneticCost * baseEnergyConsumptionRate;
    }

    public void ModifyEnergy(float amount)
    {
        float previousEnergy = currentEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
        if (Mathf.Abs(previousEnergy - currentEnergy) > Mathf.Epsilon)
        {
            OnEnergyChanged?.Invoke(EnergyPercentage);
            CheckEnergyThresholds(previousEnergy);
        }
    }

    void CheckEnergyThresholds(float previousEnergy)
    {
        if (currentEnergy <= 0f && previousEnergy > 0f)
        {
            OnEnergyDepleted?.Invoke();
        }
        else if (currentEnergy <= criticalEnergyThreshold && previousEnergy > criticalEnergyThreshold)
        {
            OnCriticalEnergy?.Invoke();
        }
    }

    public void UpdateMaxEnergy()
    {
        float newMaxEnergy = genetics.CalculateMaxEnergy(startingEnergy);
        if (newMaxEnergy != maxEnergy)
        {
            maxEnergy = newMaxEnergy;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            OnEnergyChanged?.Invoke(EnergyPercentage);
        }
    }

    // Debugging method
    public string GetEnergyInfo()
    {
        return $"Current Energy: {currentEnergy}, Max Energy: {maxEnergy}, " +
               $"Energy Percentage: {EnergyPercentage * 100f}%";
    }
}