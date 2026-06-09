using UnityEngine;
using UnityEngine.Serialization;

namespace Furkan
{
    public class Ereignisse : MonoBehaviour
    {
        public static Ereignisse Instance { get; private set; }

        [FormerlySerializedAs("Geld")] public int geld = 0;
        public int müll = 0;

        private bool _droughtTriggered = false;
        private bool _floodsTriggered = false;
        private bool _heatWaveTriggered = false;
        private bool _politicalUnrestTriggered = false;

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
            if (Co2BudgetManager.Instance != null)
            {
                float currentCo2 = Co2BudgetManager.Instance.currentFootprint;
                
                // Dürren-Trigger
                if (currentCo2 == 70 && !_droughtTriggered)
                {
                    TriggerDrought();
                    _droughtTriggered = true;
                }
                else if (currentCo2 != 70)
                {
                    _droughtTriggered = false;
                }

                // Überflutungen-Trigger
                if (currentCo2 == 75 && müll > 50 && !_floodsTriggered)
                {
                    TriggerFloods();
                    _floodsTriggered = true;
                }
                else if (currentCo2 != 75 || müll <= 50)
                {
                    _floodsTriggered = false;
                }

                // Hitzewellen-Trigger
                if (currentCo2 == 50 && !_heatWaveTriggered)
                {
                    TriggerHeatWave();
                    _heatWaveTriggered = true;
                }
                else if (currentCo2 != 50)
                {
                    _heatWaveTriggered = false;
                }
                
                if (currentCo2 == 50 && geld < 30 && !_politicalUnrestTriggered)
                {
                    TriggerPoliticalUnrest();
                    _politicalUnrestTriggered = true;
                }
                else if (currentCo2 != 50 || geld >= 30)
                {
                    _politicalUnrestTriggered = false;
                }
            }
        }

        // Event-Funktionen
        private void TriggerDrought()
        {
            geld -= 10;
            müll += 5;
            if (Co2BudgetManager.Instance != null)
            {
                Co2BudgetManager.Instance.AddBadDecision(1); // CO2 steigt leicht an (z.B. durch Waldbrände)
            }
            Debug.Log("Dürren-Event ausgelöst! Geld -10, Müll +5, CO2 +10");
        }

        private void TriggerFloods()
        {
            geld -= 40;
            müll += 20;
            Debug.Log("Überflutungs-Event ausgelöst! Geld -40, Müll +20");
        }

        private void TriggerHeatWave()
        {
            geld -= 10;
            Debug.Log("Hitzewellen-Event ausgelöst! Geld -10");
        }

        private void TriggerPoliticalUnrest()
        {
            müll += 35;
            if (Co2BudgetManager.Instance != null)
            {
                Co2BudgetManager.Instance.AddBadDecision(2);
            }
            Debug.Log("Politische Unruhen-Event ausgelöst! Müll +35, CO2 +50");
        }
    }
}
