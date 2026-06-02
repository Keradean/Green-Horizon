using UnityEngine;
using System;
using System.Collections.Generic;

namespace Furkan.Ereignisse
{
    public class Ereignisse : MonoBehaviour
    {
        public static Ereignisse Instance { get; private set; }

        public int Geld = 0;
        public int müll = 0;

        private bool droughtTriggered = false;
        private bool floodsTriggered = false;
        private bool heatWaveTriggered = false;
        private bool politicalUnrestTriggered = false;

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

        private void Update()
        {
            if (CO2BudgetManager.Instance != null)
            {
                float currentCo2 = CO2BudgetManager.Instance.currentFootprint;
                
                // Dürren-Trigger
                if (currentCo2 == 70 && !droughtTriggered)
                {
                    TriggerDrought();
                    droughtTriggered = true;
                }
                else if (currentCo2 != 70)
                {
                    droughtTriggered = false;
                }

                // Überflutungen-Trigger
                if (currentCo2 == 75 && müll > 50 && !floodsTriggered)
                {
                    TriggerFloods();
                    floodsTriggered = true;
                }
                else if (currentCo2 != 75 || müll <= 50)
                {
                    floodsTriggered = false;
                }

                // Hitzewellen-Trigger
                if (currentCo2 == 50 && !heatWaveTriggered)
                {
                    TriggerHeatWave();
                    heatWaveTriggered = true;
                }
                else if (currentCo2 != 50)
                {
                    heatWaveTriggered = false;
                }
                
                if (currentCo2 == 50 && Geld < 30 && !politicalUnrestTriggered)
                {
                    TriggerPoliticalUnrest();
                    politicalUnrestTriggered = true;
                }
                else if (currentCo2 != 50 || Geld >= 30)
                {
                    politicalUnrestTriggered = false;
                }
            }
        }

        // Event-Funktionen
        private void TriggerDrought()
        {
            Geld -= 10;
            müll += 5;
            if (CO2BudgetManager.Instance != null)
            {
                CO2BudgetManager.Instance.AddBadDecision(1); // CO2 steigt leicht an (z.B. durch Waldbrände)
            }
            Debug.Log("Dürren-Event ausgelöst! Geld -10, Müll +5, CO2 +10");
        }

        private void TriggerFloods()
        {
            Geld -= 40;
            müll += 20;
            Debug.Log("Überflutungs-Event ausgelöst! Geld -40, Müll +20");
        }

        private void TriggerHeatWave()
        {
            Geld -= 10;
            Debug.Log("Hitzewellen-Event ausgelöst! Geld -10");
        }

        private void TriggerPoliticalUnrest()
        {
            müll += 35;
            if (CO2BudgetManager.Instance != null)
            {
                CO2BudgetManager.Instance.AddBadDecision(2);
            }
            Debug.Log("Politische Unruhen-Event ausgelöst! Müll +35, CO2 +50");
        }
    }
}
