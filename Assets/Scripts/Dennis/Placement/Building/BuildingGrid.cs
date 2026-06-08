using System.Collections.Generic;
using UnityEngine;
using Dennis.Placement.Building;
//*** De Col ***\\
//=== Andy ===//
namespace Dennis.Placement.Building
{
    public class BuildingGrid : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;

        private BuildingGridCell[,] _grid;
        private readonly Dictionary<Building, List<(int x, int y)>> _buildingCells = new();

        private readonly HashSet<Vector2Int>                _roadCells      = new();
        private readonly Dictionary<Vector2Int, GameObject> _roadContainers = new(); // feste Container pro Zelle

        /////////////////////////////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            _grid = new BuildingGridCell[width, height];
            for (var x = 0; x < _grid.GetLength(0); x++)
                for (var y = 0; y < _grid.GetLength(1); y++)
                    _grid[x, y] = new BuildingGridCell(null);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        public void SetBuilding(Building building, List<Vector3> allBuildingPositions)
        {
            var cells = new List<(int x, int y)>();
            foreach (var p in allBuildingPositions)
            {
                var (x, y) = WorldToGridPosition(p);
                _grid[x, y].SetBuilding(building);
                cells.Add((x, y));
            }
            _buildingCells[building] = cells;
        }

        public void RemoveBuilding(Building building)
        {
            if (!_buildingCells.TryGetValue(building, out var cells)) return;
            foreach (var (x, y) in cells) _grid[x, y].Clear();
            _buildingCells.Remove(building);
        }

        public Building GetBuildingAt(Vector3 worldPosition)
        {
            var (x, y) = WorldToGridPosition(worldPosition);
            if (x < 0 || x >= width || y < 0 || y >= height) return null;
            return _grid[x, y].GetBuilding();
        }

        public bool CanBuild(List<Vector3> allBuildingPositions)
        {
            foreach (var p in allBuildingPositions)
            {
                var (x, y) = WorldToGridPosition(p);
                if (x < 0 || x >= width || y < 0 || y >= height) return false;
                if (!_grid[x, y].IsEmpty()) return false;
            }
            return true;
        }

        public bool CanBuildAt(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return false;
            return _grid[cell.x, cell.y].IsEmpty();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        public void SetRoad(Vector2Int cell)
        {
            _roadCells.Add(cell);
            // Container erstellen falls noch nicht vorhanden
            if (!_roadContainers.ContainsKey(cell))
            {
                var container = new GameObject($"Road_{cell.x}_{cell.y}");
                container.transform.SetParent(transform);
                container.transform.position = CellToWorld(cell);
                _roadContainers[cell] = container;
            }
        }

        public bool IsRoad(Vector2Int cell) => _roadCells.Contains(cell);

        // Tauscht das Modell im Container aus — löscht alle Kinder und instantiiert neu
        public void SwapRoadModel(Vector2Int cell, GameObject prefab, float rotation)
        {
            if (!_roadContainers.TryGetValue(cell, out var container)) return;

            // Alle alten Kinder löschen
            foreach (Transform child in container.transform)
                Destroy(child.gameObject);

            // Neues Modell als Kind instantiieren
            var model = Instantiate(prefab, container.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0, rotation, 0);
        }

        public void RemoveRoad(Vector2Int cell)
        {
            _roadCells.Remove(cell);
            if (_roadContainers.TryGetValue(cell, out var container) && container != null)
                Destroy(container);
            _roadContainers.Remove(cell);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            var (x, y) = WorldToGridPosition(worldPosition);
            return new Vector2Int(x, y);
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            return transform.position
                 + new Vector3((cell.x + 0.5f) * BuildingSystem.CellSize,
                               0,
                               (cell.y + 0.5f) * BuildingSystem.CellSize);
        }

        private (int x, int y) WorldToGridPosition(Vector3 worldPosition)
        {
            var x = Mathf.FloorToInt((worldPosition - transform.position).x / BuildingSystem.CellSize);
            var y = Mathf.FloorToInt((worldPosition - transform.position).z / BuildingSystem.CellSize);
            return (x, y);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            if (BuildingSystem.CellSize <= 0 || width <= 0 || height <= 0) return;
            var origin = transform.position;
            for (var y = 0; y <= height; y++)
            {
                Gizmos.DrawLine(
                    origin + new Vector3(0, 0.01f, y * BuildingSystem.CellSize),
                    origin + new Vector3(width * BuildingSystem.CellSize, 0.01f, y * BuildingSystem.CellSize));
            }
            for (var x = 0; x <= width; x++)
            {
                Gizmos.DrawLine(
                    origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, 0),
                    origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, height * BuildingSystem.CellSize));
            }
        }
    }

    public class BuildingGridCell
    {
        private Building _building;
        public BuildingGridCell(Building building) => _building = building;
        public void SetBuilding(Building building)  => _building = building;
        public void Clear()                          => _building = null;
        public Building GetBuilding()                => _building;
        public bool IsEmpty()                        => _building == null;
    }
}