using System.Collections.Generic;
using UnityEngine;
//=== Can Özbal ===//

public class renewable_energy : MonoBehaviour
{
    // =========================
    // SINGLETON
    // =========================

    public static renewable_energy Instance;

    private void Awake()
    {
        Instance = this;
    }

    // =========================
    // ENERGY TYPES
    // =========================

    public enum EnergyType
    {
        Wind,
        Solar,
        Water
    }

    // =========================
    // INVESTMENT DATA
    // =========================

    [System.Serializable]
    public class EnergyInvestment
    {
        public EnergyType Type;
        public float InvestmentCost;
        public float EnergyOutput;
        public float HappinessBonus;

        // CO2-Stufe die beim Bauen als gute Entscheidung gemeldet wird (1-3)
        [Range(1, 3)]
        public int CO2ReductionLevel = 1;

        public bool IsBuilt;
    }

    // =========================
    // INVESTMENTS
    // =========================

    public List<EnergyInvestment> Investments =
        new List<EnergyInvestment>();

    // =========================
    // TOTALS
    // =========================

    public float TotalEnergyOutput   { get; private set; }
    public float TotalHappinessBonus { get; private set; }

    // =========================
    // INVEST
    // =========================

    public bool Invest(EnergyType type)
    {
        EnergyInvestment investment =
            Investments.Find(i => i.Type == type);

        if (investment == null)
            return false;

        if (investment.IsBuilt)
        {
            Debug.Log(type + " bereits gebaut.");
            return false;
        }

        investment.IsBuilt = true;

        RecalculateTotals();

        // CO2 senken
       // if (CO2BudgetManager.Instance != null)
       //     CO2BudgetManager.Instance.AddGoodDecision(investment.CO2ReductionLevel);

        // Happiness erhöhen
        if (HappinessManager.Instance != null)
            HappinessManager.Instance.AddModifier(
                0,
                HappinessManager.HappinessType.Parks,
                investment.HappinessBonus);

        Debug.Log(type + " gebaut! Output: " + investment.EnergyOutput);
        return true;
    }

    // =========================
    // REMOVE
    // =========================

    public void Remove(EnergyType type)
    {
        EnergyInvestment investment =
            Investments.Find(i => i.Type == type);

        if (investment == null || !investment.IsBuilt)
            return;

        investment.IsBuilt = false;

        RecalculateTotals();

        // CO2 wieder erhöhen
      //  if (CO2BudgetManager.Instance != null)
      //      CO2BudgetManager.Instance.AddBadDecision(investment.CO2ReductionLevel);

        // Happiness wieder senken
        if (HappinessManager.Instance != null)
            HappinessManager.Instance.RemoveModifier(
                0,
                HappinessManager.HappinessType.Parks);

        Debug.Log(type + " entfernt.");
    }

    // =========================
    // RECALCULATE
    // =========================

    private void RecalculateTotals()
    {
        TotalEnergyOutput   = 0f;
        TotalHappinessBonus = 0f;

        foreach (var inv in Investments)
        {
            if (!inv.IsBuilt)
                continue;

            TotalEnergyOutput   += inv.EnergyOutput;
            TotalHappinessBonus += inv.HappinessBonus;
        }
    }

    // =========================
    // GETTERS
    // =========================

    public bool IsBuilt(EnergyType type)
    {
        EnergyInvestment investment =
            Investments.Find(i => i.Type == type);

        return investment != null && investment.IsBuilt;
    }

    public float GetOutput(EnergyType type)
    {
        EnergyInvestment investment =
            Investments.Find(i => i.Type == type);

        return investment != null && investment.IsBuilt
            ? investment.EnergyOutput
            : 0f;
    }
}