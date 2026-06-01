using System.Collections.Generic;
using UnityEngine;
//=== Can Özbal ===//

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

        public float InvestmentCost;

        public float CO2Reduction;

        public float HappinessBonus;

        public float EnergyRecovered;

        [Range(1, 10)]
        public int CO2ReductionLevel = 1;

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

    public float TotalCO2Reduction   { get; private set; }
    public float TotalHappinessBonus { get; private set; }
    public float TotalEnergyRecovered { get; private set; }

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

        mechanic.IsActive = true;

        RecalculateTotals();

        // CO2 senken
   //     if (CO2BudgetManager.Instance != null)
   //         CO2BudgetManager.Instance.AddGoodDecision(mechanic.CO2ReductionLevel);

        // Happiness erhöhen
        if (HappinessManager.Instance != null)
            HappinessManager.Instance.AddModifier(
                0,
                HappinessManager.HappinessType.Parks,
                mechanic.HappinessBonus);

        Debug.Log(type + " aktiviert! CO2 Reduktion: " + mechanic.CO2Reduction);
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
       // if (CO2BudgetManager.Instance != null)
        //    CO2BudgetManager.Instance.AddBadDecision(mechanic.CO2ReductionLevel);

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
        TotalCO2Reduction    = 0f;
        TotalHappinessBonus  = 0f;
        TotalEnergyRecovered = 0f;

        foreach (var mechanic in Mechanics)
        {
            if (!mechanic.IsActive)
                continue;

            TotalCO2Reduction    += mechanic.CO2Reduction;
            TotalHappinessBonus  += mechanic.HappinessBonus;
            TotalEnergyRecovered += mechanic.EnergyRecovered;
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