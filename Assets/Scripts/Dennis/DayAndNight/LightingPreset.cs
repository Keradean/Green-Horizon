using UnityEngine;

namespace Dennis.DayAndNight
{
    [CreateAssetMenu(fileName = "New Light Preset", menuName = "DayAndNight/Create New Light", order = 1)]
    public class LightingPreset : ScriptableObject
    {
        public Gradient ambientColor;
        public Gradient directionalColor;
        public Gradient fogColor;
        
    }
}
