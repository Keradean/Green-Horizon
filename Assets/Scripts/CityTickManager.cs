using UnityEngine;

// =========================
// CityTickManager
// Berechnet happinessValue aus dem Stadtzustand.
// Alle Gewichte im Inspector justierbar.
// =========================

public class CityTickManager : MonoBehaviour
{
    // =========================
    // SINGLETON
    // =========================

    public static CityTickManager Instance;

    private void Awake()
    {
        Instance = this;
    }

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
        float happiness = baseHappiness;

        // CO2 Last senkt Happiness (co2Value: 0.035 = leer, 1 = voll)
       // if (Co2Manager.Instance != null)
   //         happiness -= Co2Manager.Instance.co2Value * co2Penalty;

        // Kreislaufwirtschaft Bonus
      //  if (CircularEconomyManager.Instance != null)
        {
            int activeCount = 0;

        //    foreach (var mechanic in CircularEconomyManager.Instance.Mechanics)
    //            if (mechanic.IsActive) activeCount++;

            happiness += activeCount * circularEconomyBonus;
        }

        // Erneuerbare Energie Bonus
     //   if (RenewableEnergyManager.Instance != null)
        {
            int builtCount = 0;

      //      foreach (var inv in RenewableEnergyManager.Instance.Investments)
    //            if (inv.IsBuilt) builtCount++;

            happiness += builtCount * renewableEnergyBonus;
        }

        // Auf 0-100 klemmen und anwenden
        happiness = Mathf.Clamp(happiness, 0f, 100f);

    //    if (HappinessUiManager.Instance != null)
   //         HappinessUiManager.Instance.happinessValue = happiness;
    }

#if UNITY_EDITOR
    // =========================
    // DEBUG
    // =========================

    [ContextMenu("Tick manuell auslösen")]
    private void DebugTick()
    {
        Tick();
   //     Debug.Log("Happiness nach Tick: " + HappinessUiManager.Instance?.happinessValue);
    }
#endif
}
