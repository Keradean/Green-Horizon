using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//=== Can Özbal ===//

namespace Can.Manager.CityStats
{
    public class RenewableEnergy : MonoBehaviour
    {
        // =========================
        // SINGLETON
        // =========================

        public static RenewableEnergy Instance;

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
            [FormerlySerializedAs("Type")] public EnergyType type;
            [FormerlySerializedAs("InvestmentCost")] public float investmentCost;
            [FormerlySerializedAs("EnergyOutput")] public float energyOutput;
            [FormerlySerializedAs("HappinessBonus")] public float happinessBonus;

            // CO2-Stufe die beim Bauen als gute Entscheidung gemeldet wird (1-3)
            [FormerlySerializedAs("CO2ReductionLevel")] [Range(1, 3)]
            public int co2ReductionLevel = 1;

            [FormerlySerializedAs("IsBuilt")] public bool isBuilt;
        }

        // =========================
        // INVESTMENTS
        // =========================

        [FormerlySerializedAs("Investments")] public List<EnergyInvestment> investments =
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
                investments.Find(i => i.type == type);

            if (investment == null)
                return false;

            if (investment.isBuilt)
            {
                Debug.Log(type + " bereits gebaut.");
                return false;
            }

            investment.isBuilt = true;

            RecalculateTotals();

            // CO2 senken
            // if (CO2BudgetManager.Instance != null)
            //     CO2BudgetManager.Instance.AddGoodDecision(investment.CO2ReductionLevel);

            // Happiness erhöhen
            if (HappinessManager.Instance != null)
                HappinessManager.Instance.AddModifier(
                    0,
                    HappinessManager.HappinessType.Parks,
                    investment.happinessBonus);

            Debug.Log(type + " gebaut! Output: " + investment.energyOutput);
            return true;
        }

        // =========================
        // REMOVE
        // =========================

        public void Remove(EnergyType type)
        {
            EnergyInvestment investment =
                investments.Find(i => i.type == type);

            if (investment == null || !investment.isBuilt)
                return;

            investment.isBuilt = false;

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

            foreach (var inv in investments)
            {
                if (!inv.isBuilt)
                    continue;

                TotalEnergyOutput   += inv.energyOutput;
                TotalHappinessBonus += inv.happinessBonus;
            }
        }

        // =========================
        // GETTERS
        // =========================

        public bool IsBuilt(EnergyType type)
        {
            EnergyInvestment investment =
                investments.Find(i => i.type == type);

            return investment != null && investment.isBuilt;
        }

        public float GetOutput(EnergyType type)
        {
            EnergyInvestment investment =
                investments.Find(i => i.type == type);

            return investment != null && investment.isBuilt
                ? investment.energyOutput
                : 0f;
        }
    }
}