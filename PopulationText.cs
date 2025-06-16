using UnityEngine;
using TMPro; // If using TextMeshPro

public class PopulationUI : MonoBehaviour
{
    public TextMeshProUGUI populationText; // Assign in Inspector

    void Update()
    {
        if (OrganismSpawner.Instance != null)
        {
            int current = OrganismSpawner.Instance.GetPopulation();
            int max = OrganismSpawner.Instance.GetMaxPopulation();
            populationText.text = $"Population: {current} / {max}";
        }
    }
}