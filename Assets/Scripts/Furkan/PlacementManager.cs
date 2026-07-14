using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Furkan
{
    /// <summary>
    /// Manages placement of buildings and roads, handles pathfinding between structures.
    /// </summary>
    public class PlacementManager : MonoBehaviour
    {
        [SerializeField] private List<StructureModel> allStructures = new();
        [SerializeField] private List<StructureModel> houses = new();
        [SerializeField] private List<StructureModel> specialStructures = new();

        private Dictionary<Vector2Int, StructureModel> structureGrid = new();

        private void Awake()
        {
            InitializeStructures();
        }

        private void InitializeStructures()
        {
            // Find all structures in the scene
            allStructures = FindObjectsByType<StructureModel>(FindObjectsSortMode.None).ToList();

            // Separate houses and special structures
            houses.Clear();
            specialStructures.Clear();

            // For now, consider structures without any special tag as houses
            // This can be extended with proper tagging logic
            foreach (var structure in allStructures)
            {
                if (structure != null)
                {
                    houses.Add(structure);
                    structureGrid[structure.RoadPosition] = structure;
                }
            }
        }

        public List<StructureModel> GetAllHouses()
        {
            return new List<StructureModel>(houses);
        }

        public List<StructureModel> GetAllSpecialStructures()
        {
            return new List<StructureModel>(specialStructures);
        }

        public StructureModel GetRandomSpecialStrucutre()
        {
            if (specialStructures.Count == 0)
                return null;

            return specialStructures[Random.Range(0, specialStructures.Count)];
        }

        public StructureModel GetRandomHouseStructure()
        {
            if (houses.Count == 0)
                return null;

            return houses[Random.Range(0, houses.Count)];
        }

        public StructureModel GetStructureAt(Vector2Int position)
        {
            if (structureGrid.TryGetValue(position, out var structure))
            {
                return structure;
            }

            return null;
        }

        /// <summary>
        /// Gets a path between two structures using simple BFS/A* pathfinding.
        /// This is a placeholder implementation - should be integrated with BuildingGrid.
        /// </summary>
        public List<Vector3Int> GetPathBetween(Vector2Int start, Vector2Int end, bool avoidStructures = true)
        {
            // Placeholder: Return a simple path
            var path = new List<Vector3Int>
            {
                new Vector3Int(start.x, 0, start.y),
                new Vector3Int(end.x, 0, end.y)
            };

            return path;
        }

        public void RegisterStructure(StructureModel structure)
        {
            if (!allStructures.Contains(structure))
            {
                allStructures.Add(structure);
                structureGrid[structure.RoadPosition] = structure;
            }
        }

        public void UnregisterStructure(StructureModel structure)
        {
            allStructures.Remove(structure);
            structureGrid.Remove(structure.RoadPosition);
        }
    }
}
