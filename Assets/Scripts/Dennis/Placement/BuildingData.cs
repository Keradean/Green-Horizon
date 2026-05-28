using UnityEngine;
//*** De Col ***\\
namespace Dennis.Placement
{
    [CreateAssetMenu(menuName = "Building/BuildingData", fileName = "BuildingData")]
    public class BuildingData : ScriptableObject
    {
        [Header("Building Data")]
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public BuildingModel Model { get; private set; }

        [Header("Tooltip Info")]
        [field: SerializeField] public string BuildingName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Residents { get; private set; }       // Einwohner
        [field: SerializeField] public int IncomePerHour { get; private set; }   // Geld pro Stunde
        [field: SerializeField] public int EnergyUsage { get; private set; }     // Energie Verbrauch
        [field: SerializeField] public int Pollution { get; private set; }       // Umweltverschmutzung
    }
}