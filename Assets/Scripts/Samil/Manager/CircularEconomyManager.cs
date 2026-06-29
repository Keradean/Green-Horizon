using Dennis.Manager;
using Dennis.Placement.Building;
using UnityEngine;

namespace Samil.Manager
{
    public class CircularEconomyManager : MonoBehaviour
    {
        public static CircularEconomyManager Instance { get; private set; }

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

        // ===========================
        // READ-ONLY TOTALS (per day)
        // ===========================

        public int TotalDailyCO2Reduction { get; private set; }
        public int TotalDailyCostSavings  { get; private set; }
        public int RecyclingBuildingCount { get; private set; }

        // ===========================
        // DAY TICK (called by CityTickManager)
        // ===========================

        // Recomputes totals and applies GreenCoin savings for one game-day.
        // Returns the net pollution after circular-economy buildings have offset it.
        public float ProcessDayTick(float grossPollution)
        {
            RecalculateTotals();

            if (TotalDailyCostSavings > 0)
                GreenCoinManager.Instance.AddGold(TotalDailyCostSavings);

            var netPollution = Mathf.Max(0f, grossPollution - TotalDailyCO2Reduction);
            return netPollution;
        }

        // ===========================
        // INTERNAL HELPERS
        // ===========================

        private void RecalculateTotals()
        {
            TotalDailyCO2Reduction = 0;
            TotalDailyCostSavings  = 0;
            RecyclingBuildingCount = 0;

            if (CityTickManager.Instance == null) return;

            foreach (var building in CityTickManager.Instance.Buildings)
            {
                if (building.CO2Reduction <= 0 && building.CostSavingsPerDay <= 0) continue;

                TotalDailyCO2Reduction += building.CO2Reduction;
                TotalDailyCostSavings  += building.CostSavingsPerDay;
                RecyclingBuildingCount++;
            }
        }

        // ===========================
        // DEBUG
        // ===========================

        [ContextMenu("Debug: Print Circular Economy Status")]
        private void PrintStatus()
        {
            RecalculateTotals();
            Debug.Log($"[CircularEconomy] Gebäude: {RecyclingBuildingCount} | " +
                      $"CO2 Reduktion/Tag: {TotalDailyCO2Reduction} | " +
                      $"GreenCoin Ersparnis/Tag: {TotalDailyCostSavings}");
        }
    }
}
