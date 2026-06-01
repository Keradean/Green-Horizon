using UnityEngine;

namespace Dennis.Placement.Road
{
    [CreateAssetMenu(menuName = "Building/RoadData")]
    public class RoadData : ScriptableObject
    {
        public GameObject straightPrefab;
        public GameObject cornerPrefab;
        public GameObject tJunctionPrefab;
        public GameObject crossPrefab;
    }
}
