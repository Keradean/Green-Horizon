using UnityEngine;
//*** De Col ***\\
namespace Dennis.Placement
{
    [CreateAssetMenu(menuName = "Building/BuildingData",  fileName = "BuildingData")]
    public class BuildingData : ScriptableObject
    {
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public BuildingModel Model { get; private set; }
    }
}
