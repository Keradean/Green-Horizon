using System.Collections.Generic;
using Andy.Manager;
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
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
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

        this.tickPassed += Time.deltaTime * daysPerSecond;
        int newDaysPassed = Mathf.FloorToInt(this.tickPassed);
        if (newDaysPassed > this.daysPassed)
        {
            UpdateDaysPassed(newDaysPassed);
        }
        this.daysPassed = newDaysPassed;
    }

    private void UpdateDaysPassed(int newDaysPassed)
    {
        Debug.Log("Tag updated " + newDaysPassed);
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
        goldText.text = GreenCoinManager.Instance.CurrentGold.ToString();
        int residents = 0;
        Buildings.ForEach(building => residents += building.Residents);
        residentText.text = residents.ToString();
    }
}
