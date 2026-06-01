using UnityEngine;
using Dennis.Placement.Building;

namespace Dennis.Placement.Road
{
    [CreateAssetMenu(menuName = "Building/RoadData")]
    public class RoadData : BuildingData
    {
        [field: SerializeField] public new BuildingModel Model { get; private set; }

        public GameObject straightPrefab;
        public GameObject cornerPrefab;
        public GameObject tJunctionPrefab;
        public GameObject crossPrefab;
    }
}