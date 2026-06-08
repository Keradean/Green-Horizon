using UnityEngine;
using Dennis.Placement.Building;

namespace Dennis.Placement.Road
{
    [CreateAssetMenu(menuName = "Building/RoadData")]
    public class RoadData : ScriptableObject  // nicht mehr von BuildingData erben!
    {
        [Header("Preview")]
        public BuildingData previewData;        // Ein normales BuildingData SO reinziehen

        [Header("Road Prefabs")]
        public GameObject straightPrefab;
        public GameObject cornerPrefab;
        public GameObject tJunctionPrefab;
        public GameObject crossPrefab;
    }
}