using System.Collections.Generic;
using UnityEngine;

namespace Furkan
{
    /// <summary>
    /// Represents a marker point on a road or building with navigation information.
    /// </summary>
    public class Marker : MonoBehaviour
    {
        [SerializeField] private List<Vector3> adjacentPositions = new();

        public Vector3 Position => transform.position;

        public bool OpenForconnections { get; set; } = true;

        public void SetAdjacentPositions(List<Vector3> positions)
        {
            adjacentPositions = new List<Vector3>(positions);
        }

        public List<Vector3> GetAdjacentPositions()
        {
            return new List<Vector3>(adjacentPositions);
        }
    }
}
