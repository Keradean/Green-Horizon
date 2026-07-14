using System.Collections.Generic;
using UnityEngine;

namespace Furkan
{
    /// <summary>
    /// Interface for structures that need road access.
    /// </summary>
    public interface INeedingRoad
    {
        Vector2Int RoadPosition { get; }
    }

    /// <summary>
    /// Represents a marker with position and connection info.
    /// </summary>
    public class MarkerInfo
    {
        public Vector3 Position { get; set; }
    }

    /// <summary>
    /// Represents a structure (house, special building, etc.) that can be placed on the grid.
    /// </summary>
    public class StructureModel : MonoBehaviour, INeedingRoad
    {
        [SerializeField] private Vector2Int roadPosition;
        private List<MarkerInfo> pedestrianMarkers = new();
        private List<MarkerInfo> carMarkers = new();

        public Vector2Int RoadPosition => roadPosition;

        public void SetRoadPosition(Vector2Int position)
        {
            roadPosition = position;
        }

        public MarkerInfo GetPedestrianSpawnMarker(Vector3 nearPosition)
        {
            return new MarkerInfo { Position = transform.position };
        }

        public MarkerInfo GetNearestPedestrianMarkerTo(Vector3 position)
        {
            return new MarkerInfo { Position = transform.position };
        }

        public List<MarkerInfo> GetPedestrianMarkers()
        {
            return pedestrianMarkers;
        }

        public MarkerInfo GetCarSpawnMarker(Vector3 direction)
        {
            return new MarkerInfo { Position = transform.position + direction.normalized * 0.5f };
        }

        public MarkerInfo GetCarEndMarker(Vector3 direction)
        {
            return new MarkerInfo { Position = transform.position + direction.normalized * 0.5f };
        }

        public List<MarkerInfo> GetCarMarkers()
        {
            return carMarkers;
        }

        public MarkerInfo GetNearestCarMarkerTo(Vector3 position)
        {
            return new MarkerInfo { Position = transform.position };
        }
    }
}
