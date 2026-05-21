using UnityEngine;
using UnityEngine.UI;

//=== Andy ===//

namespace Andy.Manager.CityStats
{
    public class Co2Manager: MonoBehaviour
    {
        // Singleton - von überall erreichbar mit:  CityStatsManager.Instance.co2Value += 0.1f;
        public static Co2Manager Instance { get; private set; }

        [Header("CO2")]
        [Range(0f, 1f)]
        public float co2Value = 0.035f;             // 0.035 = leer, 1 = voll
        public Image co2Fill;                   // Co2 - Image Fill

        private void Awake()
        {
            // Singleton Setup
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Update()
        {
            co2Fill.fillAmount = co2Value;

            // Unter 0.9 = weiß, ab 0.9 langsam zu rot
            var colorT = Mathf.InverseLerp(0.9f, 1f, co2Value);
            co2Fill.color = Color.Lerp(Color.white, Color.red, colorT);
        }
    }
}