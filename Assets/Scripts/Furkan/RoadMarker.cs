using System.Collections.Generic;
using UnityEngine;

namespace Furkan
{
    public enum RoadMarkerShape
    {
        Single,
        Straight,
        Corner,
        TJunction,
        Cross
    }

    public class RoadMarker : MonoBehaviour
    {
        [SerializeField] private Vector2Int cell;
        [SerializeField] private RoadMarkerShape shape = RoadMarkerShape.Single;
        [SerializeField] private bool openForConnections = true;
        [SerializeField] private List<RoadMarker> adjacentMarkers = new();

        public Vector2Int Cell
        {
            get => cell;
            set => cell = value;
        }

        public Vector3 Position => transform.position;

        public RoadMarkerShape Shape
        {
            get => shape;
            set => shape = value;
        }

        public bool OpenForConnections
        {
            get => openForConnections;
            set => openForConnections = value;
        }

        public List<RoadMarker> AdjacentMarkers => adjacentMarkers;

        public void SetConnections(List<RoadMarker> markers)
        {
            adjacentMarkers.Clear();
            adjacentMarkers.AddRange(markers);
        }

        public List<Vector3> GetAdjacentPositions()
        {
            var positions = new List<Vector3>();
            foreach (var marker in adjacentMarkers)
            {
                if (marker != null)
                    positions.Add(marker.Position);
            }
            return positions;
        }
    }
}
