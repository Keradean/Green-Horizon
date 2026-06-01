using System.Collections.Generic;
using UnityEngine;
using Dennis.Placement.Building; 
//*** De Col ***\\
namespace Dennis.Placement.Building
{
    public class BuildingGrid : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;

        private BuildingGridCell[,] _grid;
        private readonly Dictionary<Building, List<(int x, int y)>> _buildingCells = new();

        // ── Road Tracking ──────────────────────────────────────────────────────
        private readonly HashSet<Vector2Int>                _roadCells   = new();
        private readonly Dictionary<Vector2Int, GameObject> _roadObjects = new();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            _grid = new BuildingGridCell[width, height];
            for (var x = 0; x < _grid.GetLength(0); x++)
                for (var y = 0; y < _grid.GetLength(1); y++)
                    _grid[x, y] = new BuildingGridCell(null);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // ── Building Placement ────────────────────────────────────────────────

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
        // ── Road Tracking ─────────────────────────────────────────────────────

        /// <summary>Markiert eine Zelle als Straße und speichert das GameObject.</summary>
        public void SetRoad(Vector2Int cell, GameObject roadObject)
        {
            _roadCells.Add(cell);
            _roadObjects[cell] = roadObject;
        }

        /// <summary>Gibt true zurück wenn die Zelle eine Straße enthält.</summary>
        public bool IsRoad(Vector2Int cell) => _roadCells.Contains(cell);

        /// <summary>
        /// Ersetzt das visuelle GameObject einer Straßenzelle.
        /// Das alte Objekt wird zerstört, das neue gespeichert.
        /// </summary>
        public void ReplaceRoadObject(Vector2Int cell, GameObject newObject)
        {
            if (_roadObjects.TryGetValue(cell, out var old) && old != null)
                Destroy(old);
            _roadObjects[cell] = newObject;
        }

        /// <summary>Entfernt eine Straßenzelle komplett (Grid + Visual).</summary>
        public void RemoveRoad(Vector2Int cell)
        {
            _roadCells.Remove(cell);
            if (_roadObjects.TryGetValue(cell, out var obj) && obj != null)
                Destroy(obj);
            _roadObjects.Remove(cell);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // ── Koordinaten ───────────────────────────────────────────────────────

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
    /************************************************************************************************/
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