using UnityEngine;

//*** De Col ***\\
namespace Dennis.Placement.Building
{
    [CreateAssetMenu(menuName = "Building/BuildingData", fileName = "BuildingData")]
    public class BuildingData : ScriptableObject
    {
        [Header("Building Data")]
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public BuildingModel Model { get; private set; } 
        [field: SerializeField] public bool RequiresRoad { get; private set; } = true;
        [field: SerializeField] public BuildingData RequiredBuilding { get; private set; }

        [Header("Tooltip Info")]
        [field: SerializeField] public string BuildingName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Residents { get; private set; }       // Einwohner
        [field: SerializeField] public int IncomePerHour { get; private set; }   // Geld pro Stunde
        [field: SerializeField] public string GridSize { get; private set; }  // z.B. "2x3" oder "1x1"
        [field: SerializeField] public int Pollution { get; private set; }       // Umweltverschmutzung

        [Header("Circular Economy")]
        [field: SerializeField] public int CO2Reduction { get; private set; }      // CO2 Reduktion pro Tag (Recycling-Gebäude)
        [field: SerializeField] public int CostSavingsPerDay { get; private set; } // GreenCoins Ersparnis pro Tag
    }
}