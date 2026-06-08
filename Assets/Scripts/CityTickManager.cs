using System.Collections.Generic;
using Andy.Manager;
using Andy.Manager.CityStats;
using Dennis.Manager;
using Dennis.Placement.Building;
using UnityEngine;

public class CityTickManager : MonoBehaviour
{
    public static CityTickManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField]
    private float daysPerSecond = 1f; // Anzahl der Tage, die pro Sekunde vergehen

    [SerializeField]
    private TMPro.TMP_Text goldText; // UI-Text, um das aktuelle Gold anzuzeigen

    [SerializeField]
    private TMPro.TMP_Text residentText;

    public int daysPassed = 0; // Anzahl der Tage, die seit Beginn des Spiels vergangen sind

    private float tickPassed = 0f;
    public List<BuildingData> Buildings { get; private set; } = new List<BuildingData>();

    private void Update()
    {
        // Berechne die Anzahl der Tage, die seit dem letzten Frame vergangen sind
        if (GameStateManager.Instance.CurrentGameState == GameState.Paused) return;

        tickPassed += Time.deltaTime * daysPerSecond;
        var newDaysPassed = Mathf.FloorToInt(tickPassed);
        if (newDaysPassed > daysPassed)
        {
            UpdateDaysPassed(newDaysPassed);
        }
        daysPassed = newDaysPassed;
    }

    private void UpdateDaysPassed(int newDaysPassed)
    {
        Buildings.ForEach(building =>
        {
            GreenCoinManager.Instance.AddGold(building.IncomePerHour * 24);
        });
        // CO2BudgetManager.Instance.AddBadDecision(1);
        // renewable_energy.Instance.Investments.ForEach(investment =>
        // {
        //     if (investment.IsBuilt)
        //     {
        //         CO2BudgetManager.Instance.AddGoodDecision(investment.CO2ReductionLevel);
        //     }
        // });
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Format with comma as thousand separator
        goldText.text = GreenCoinManager.Instance.CurrentGold.ToString("N0");
        var residents = 0;
        Buildings.ForEach(building => residents += building.Residents);
        residentText.text = residents.ToString("N0");
    }
}