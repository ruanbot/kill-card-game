using System.Collections;
using UnityEngine;
using TMPro;

public class EnergyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyText;

    private int currentEnergy = 0;
    [SerializeField] private float energyIncreaseInterval = 2f;
    [SerializeField] private int maxEnergy = 8;

    void Start()
    {
        StartCoroutine(IncreaseEnergyOverTime());
        UpdateEnergyUI();
    }

    private IEnumerator IncreaseEnergyOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(energyIncreaseInterval);

            if (currentEnergy < maxEnergy)
            {
                currentEnergy++;
                UpdateEnergyUI();
            }
        }
    }

    private void UpdateEnergyUI()
    {
        energyText.text = currentEnergy.ToString();
    }

    // Method to check if there's enough energy for a card
    public bool HasEnoughEnergy(int cost)
    {
        return currentEnergy >= cost;
    }

    // Method to spend energy
    public void SpendEnergy(int cost)
    {
        if (currentEnergy >= cost)
        {
            currentEnergy -= cost;
            UpdateEnergyUI();
        }
    }

    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }
}
