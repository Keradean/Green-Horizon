using System.Collections.Generic;
using UnityEngine;


public class CircularEconomyManager : MonoBehaviour
{
    // =========================
    // SINGLETON
    // =========================

    public static CircularEconomyManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // =========================
    // MECHANIC TYPES
    // =========================

    public enum CircularMechanicType
    {
        Recycling,
        ZeroWaste,
        EnergyRecovery
    }

    // =========================
    // MECHANIC DATA
    // =========================

    [System.Serializable]
    public class CircularMechanic
    {
        public CircularMechanicType Type;

        // Kosten in GreenCoins zum Aktivieren
        public int InvestmentCost;

        // CO2 Stufe (1=klein, 2=mittel, 3=groß) für CO2BudgetManager
        [Range(1, 3)]
        public int CO2ReductionLevel = 1;

        // Happiness Bonus
        public float HappinessBonus;

        // Zurückgewonnene Energie
        public float EnergyRecovered;

        // GreenCoins die pro Tick zurückgewonnen werden
        public int CostSavingsPerTick;

        public bool IsActive;
    }

    // =========================
    // MECHANICS
    // =========================

    public List<CircularMechanic> Mechanics =
        new List<CircularMechanic>();

    // =========================
    // TOTALS
    // =========================

    public float TotalHappinessBonus  { get; private set; }
    public float TotalEnergyRecovered { get; private set; }
    public int   TotalCostSavings     { get; private set; }

    // =========================
    // SAVINGS TICK
    // =========================

    [Tooltip("Wie oft pro Sekunde GreenCoins zurückgewonnen werden")]
    public float savingsTickRate = 10f;

    private float _savingsTimer;

    private void Update()
    {
        _savingsTimer += Time.deltaTime;

        if (_savingsTimer >= savingsTickRate)
        {
            _savingsTimer = 0f;
            ApplyCostSavings();
        }
    }

    private void ApplyCostSavings()
    {
        if (TotalCostSavings <= 0) return;

     //   if (GreenCoinManager.Instance != null)
     //       GreenCoinManager.Instance.AddGold(TotalCostSavings);
    }

    // =========================
    // ACTIVATE
    // =========================

    public bool Activate(CircularMechanicType type)
    {
        CircularMechanic mechanic =
            Mechanics.Find(m => m.Type == type);

        if (mechanic == null)
            return false;

        if (mechanic.IsActive)
        {
            Debug.Log(type + " bereits aktiv.");
            return false;
        }

        // Kosten abziehen
     //   if (GreenCoinManager.Instance != null)
        {
      //      if (!GreenCoinManager.Instance.SpendGold(mechanic.InvestmentCost))
            {
                Debug.Log("Nicht genug GreenCoins für " + type);
                return false;
            }
        }

        mechanic.IsActive = true;

        RecalculateTotals();

        // CO2 senken
      //  if (CO2BudgetManager.Instance != null)
      //      CO2BudgetManager.Instance.AddGoodDecision(mechanic.CO2ReductionLevel);

        // Happiness erhöhen
        if (HappinessManager.Instance != null)
            HappinessManager.Instance.AddModifier(
                0,
                HappinessManager.HappinessType.Parks,
                mechanic.HappinessBonus);

        Debug.Log(type + " aktiviert!");
        return true;
    }

    // =========================
    // DEACTIVATE
    // =========================

    public void Deactivate(CircularMechanicType type)
    {
        CircularMechanic mechanic =
            Mechanics.Find(m => m.Type == type);

        if (mechanic == null || !mechanic.IsActive)
            return;

        mechanic.IsActive = false;

        RecalculateTotals();

        // CO2 wieder erhöhen
    //    if (CO2BudgetManager.Instance != null)
     //       CO2BudgetManager.Instance.AddBadDecision(mechanic.CO2ReductionLevel);

        // Happiness wieder senken
        if (HappinessManager.Instance != null)
            HappinessManager.Instance.RemoveModifier(
                0,
                HappinessManager.HappinessType.Parks);

        Debug.Log(type + " deaktiviert.");
    }

    // =========================
    // RECALCULATE
    // =========================

    private void RecalculateTotals()
    {
        TotalHappinessBonus  = 0f;
        TotalEnergyRecovered = 0f;
        TotalCostSavings     = 0;

        foreach (var mechanic in Mechanics)
        {
            if (!mechanic.IsActive)
                continue;

            TotalHappinessBonus  += mechanic.HappinessBonus;
            TotalEnergyRecovered += mechanic.EnergyRecovered;
            TotalCostSavings     += mechanic.CostSavingsPerTick;
        }
    }

    // =========================
    // GETTERS
    // =========================

    public bool IsActive(CircularMechanicType type)
    {
        CircularMechanic mechanic =
            Mechanics.Find(m => m.Type == type);

        return mechanic != null && mechanic.IsActive;
    }

    public float GetEnergyRecovered(CircularMechanicType type)
    {
        CircularMechanic mechanic =
            Mechanics.Find(m => m.Type == type);

        return mechanic != null && mechanic.IsActive
            ? mechanic.EnergyRecovered
            : 0f;
    }

    // =========================
    // DEBUG TESTS
    // =========================

    [ContextMenu("Test Recycling aktivieren")]
    private void TestRecycling()
    {
        Activate(CircularMechanicType.Recycling);
    }

    [ContextMenu("Test Zero Waste aktivieren")]
    private void TestZeroWaste()
    {
        Activate(CircularMechanicType.ZeroWaste);
    }

    [ContextMenu("Test Energie-Rückgewinnung aktivieren")]
    private void TestEnergyRecovery()
    {
        Activate(CircularMechanicType.EnergyRecovery);
    }
}