using System.Collections.Generic;
using UnityEngine;

namespace Placement
{
    public class BuildingGrid : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;
        private BuildingGridCell[,]  _grid;
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
            foreach (var p in allBuildingPositions)
            {
                var (x, y) = WorldToGridPosition(p);
                _grid[x, y].SetBuilding(building);
            }
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
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public BuildingGridCell(Building building)
        {
            _building = building;
        }    
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public void SetBuilding(Building building)
        {
            _building = building;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public bool IsEmpty()
        {
            return _building == null; 
        }
    }
}
