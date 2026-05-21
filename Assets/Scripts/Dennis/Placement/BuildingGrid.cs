using System.Collections.Generic;
using UnityEngine;
//*** De Col ***\\
namespace Dennis.Placement
{
    public class BuildingGrid : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;
        private BuildingGridCell[,]  _grid;
        private readonly Dictionary<Building, List<(int x, int y)>> _buildingCells = new();
        /////////////////////////////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            _grid = new BuildingGridCell[width, height];
            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    _grid[x, y] = new BuildingGridCell(null);
                }
            }
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
        /////////////////////////////////////////////////////////////////////////////////////
        public void RemoveBuilding(Building building)
        {
            if (!_buildingCells.TryGetValue(building, out var cells)) return;
            foreach (var (x, y) in cells) _grid[x, y].Clear();
            _buildingCells.Remove(building);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public Building GetBuildingAt(Vector3 worldPosition)
        {
            var (x, y) = WorldToGridPosition(worldPosition);
            if (x < 0 || x >= width || y < 0 || y >= height) return null;
            return _grid[x, y].GetBuilding();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
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
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public bool CanBuildAt(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return false;
            return _grid[cell.x, cell.y].IsEmpty();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            var (x, y) = WorldToGridPosition(worldPosition);
            return new Vector2Int(x, y);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return transform.position
                 + new Vector3((cell.x + 0.5f) * BuildingSystem.CellSize,
                               0,
                               (cell.y + 0.5f) * BuildingSystem.CellSize);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
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
                var start = origin + new Vector3(0, 0.01f, y * BuildingSystem.CellSize);
                var end = origin + new Vector3(width * BuildingSystem.CellSize, 0.01f, y * BuildingSystem.CellSize);
                Gizmos.DrawLine(start, end);
            }
            for (var x = 0; x <= width; x++)
            {
                var start = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, 0);   
                var end = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, height * BuildingSystem.CellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
    /************************************************************************************************/
    public class BuildingGridCell
    {
        private Building _building;
        /////////////////////////////////////////////////////////////////////////////////////
        public BuildingGridCell(Building building)
        {
            _building = building;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void SetBuilding(Building building) => _building = building;
        /////////////////////////////////////////////////////////////////////////////////////
        public void Clear() => _building = null;
        /////////////////////////////////////////////////////////////////////////////////////
        public Building GetBuilding() => _building;
        /////////////////////////////////////////////////////////////////////////////////////
        public bool IsEmpty() => _building == null;
    }
}