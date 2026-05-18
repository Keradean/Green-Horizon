using UnityEngine;
using UnityEngine.UI;
//=== Andy ===//

public class HappinessDisplay : MonoBehaviour
{
    [Header("Happiness Wert")]
    [Range(0f, 100f)]
    public float happinessValue = 100f;

    [Header("Icon")]
    public Image happinessIcon;

    [Header("Schwellen")]
    public HappinessThreshold[] thresholds;

    void Update()
    {
        // Farbe von grün zu rot
        float colorT = 1f - (happinessValue / 100f);
        happinessIcon.color = Color.Lerp(Color.green, Color.red, colorT);

        // Passendes Icon finden
        Sprite bestSprite = null;
        float bestMin = -1f;

        foreach (HappinessThreshold t in thresholds)
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