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

    // =========================
    // TICK SETTINGS
    // =========================

    [Header("Tick")]
    [Tooltip("Sekunden zwischen zwei Berechnungen")]
    public float tickInterval = 3f;

    private float _timer;

    // =========================
    // FORMEL GEWICHTE
    // =========================

    [Header("Basiswert")]
    [Range(0f, 100f)]
    [Tooltip("Happiness ohne irgendwelche Einflüsse")]
    public float baseHappiness = 60f;

    [Header("CO2 Einfluss (Co2Manager)")]
    [Tooltip("Wie stark co2Value (0-1) die Happiness senkt")]
    [Range(0f, 100f)]
    public float co2Penalty = 40f;

    [Header("Kreislaufwirtschaft Bonus")]
    [Tooltip("Bonus wenn Circular Economy aktiv ist")]
    [Range(0f, 30f)]
    public float circularEconomyBonus = 15f;

    [Header("Erneuerbare Energie Bonus")]
    [Tooltip("Bonus pro gebautem Energieprojekt")]
    [Range(0f, 20f)]
    public float renewableEnergyBonus = 10f;

    // =========================
    // UPDATE
    // =========================

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= tickInterval)
        {
            _timer = 0f;
            Tick();
        }
    }

    // =========================
    // TICK
    // =========================

    private void Tick()
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
