using System;
using UnityEngine;

namespace Furkan
{
    public class Co2BudgetManager : MonoBehaviour
    {
        public static Co2BudgetManager Instance { get; private set; }

        [Header("CO2 Budget Settings")]
        [Tooltip("Startwert des CO2-Fußabdrucks in Tonnen")]
        public float startFootprint = 0f;

        [Tooltip("Maximaler CO2-Fußabdruck, bevor Strafen anfallen")]
        public float maxFootprint = 1000f;

        [Tooltip("Prozentualer Schwellenwert für die Warnung (0.0 bis 1.0)")]
        [Range(0f, 1f)]
        public float warningThreshold = 0.8f;

        [Tooltip("Geldstrafe pro Tag, wenn das CO2-Limit überschritten ist")]
        public int pollutionPenalty = 100;

        [Tooltip("Aktueller CO2-Fußabdruck in Tonnen")]
        public float currentFootprint;

        public event Action<float> OnBudgetChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentFootprint = startFootprint;
        }

        // Erhöhe den CO2-Fußabdruck je nach Stufe der schlechten Entscheidung
        // Stufe 1 = klein, Stufe 2 = mittel, Stufe 3 = groß
        public void AddBadDecision(int level)
        {
            var amount = level switch
            {
                1 => 10f // Beispielwert für kleine schlechte Entscheidung
                ,
                2 => 50f // Beispielwert für mittlere schlechte Entscheidung
                ,
                3 => 200f // Beispielwert für große schlechte Entscheidung
                ,
                _ => 0f
            };
            currentFootprint += amount;
            OnBudgetChanged?.Invoke(currentFootprint);
        }

        public void AddGoodDecision(int level)
        {
            var amount = level switch
            {
                1 => 10f // Beispielwert für kleine gute Entscheidung
                ,
                2 => 50f // Beispielwert für mittlere gute Entscheidung
                ,
                3 => 200f // Beispielwert für große gute Entscheidung
                ,
                _ => 0f
            };

            // Fußabdruck verringern, aber nicht < 0
            currentFootprint = Mathf.Max(0f, currentFootprint - amount);
            OnBudgetChanged?.Invoke(currentFootprint);
        }

        public void AddPollution(float amount)
        {
            currentFootprint += amount;

            // Warnung prüfen
            if (currentFootprint >= maxFootprint * warningThreshold && currentFootprint < maxFootprint)
            {
                Debug.LogWarning($"[CO2 Warnung] Dein CO2-Fußabdruck ist hoch ({currentFootprint:F1}/{maxFootprint})!");
            }

            // Strafe prüfen
            if (currentFootprint >= maxFootprint)
            {
                Debug.LogError($"[CO2 ALARM] Limit überschritten! Strafe von {pollutionPenalty} Gold fällig.");
                Dennis.Manager.GreenCoinManager.Instance.SpendGold(pollutionPenalty);
            }

            OnBudgetChanged?.Invoke(currentFootprint);
        }

        public float GetCurrentFootprint()
        {
            return currentFootprint;
        }

        public void ResetFootprint()
        {
            currentFootprint = startFootprint;
            OnBudgetChanged?.Invoke(currentFootprint);
        }
    }
}
