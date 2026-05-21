using UnityEngine;

//=== Andy ===//

namespace Andy.Manager.CityStats
{
    [System.Serializable]
    public class HappinessThreshold
    {
        public Sprite icon;          // Das Smiley Bild
        [Range(0f, 100f)]
        public float minValue;       // Ab welchem Prozentwert dieses Bild gilt
    }
}