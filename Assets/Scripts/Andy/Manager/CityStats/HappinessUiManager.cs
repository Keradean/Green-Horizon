using UnityEngine;
using UnityEngine.UI;

//=== Andy ===//

namespace Andy.Manager.CityStats
{
    public class HappinessUiManager : MonoBehaviour
    {
        // Singleton - von überall erreichbar
        public static HappinessUiManager Instance { get; private set; }

        [Header("Happiness Wert")]
        [Range(0f, 100f)]
        public float happinessValue = 100f;

        [Header("Icon")]
        public Image happinessIcon;

        [Header("Farb-Verlauf")]
        public Gradient colorGradient;          // Im Inspector einstellbar

        [Header("Schwellen")]
        public HappinessThreshold[] thresholds;

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
            // Farbe aus Gradient (0 = rot, 1 = grün)
            happinessIcon.color = colorGradient.Evaluate(happinessValue / 100f);

            // Passendes Icon finden
            Sprite bestSprite = null;
            var bestMin = -1f;

            foreach (var t in thresholds)
            {
                if (happinessValue >= t.minValue && t.minValue >= bestMin)
                {
                    bestSprite = t.icon;
                    bestMin = t.minValue;
                }
            }

            if (bestSprite != null)
                happinessIcon.sprite = bestSprite;
        }
    }
}