using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Dennis.Placement.Building;
//*** De Col ***\\
//=== Andy ===//
namespace Dennis.Placement.Road
{
    public class RoadDragHandler : MonoBehaviour
    {
        [SerializeField] private RoadData roadData;
        [SerializeField] private BuildingPreview previewPrefab;
        [SerializeField] private BuildingGrid grid;

        private readonly List<BuildingPreview> _pool        = new();
        private readonly List<Vector2Int>      _scratchPath = new();
        private readonly List<Vector2Int>      _toRecheck   = new();
        private readonly HashSet<Vector2Int>   _tempRoads   = new();
        private int _activeCount;

        private bool       _isDragging;
        private Vector2Int _start;
        private Vector2Int _last;
        private Vector2Int _hoverCell;
        private bool       _hasHover;

        /////////////////////////////////////////////////////////////////////////////////////
        public void Tick(Vector3 mouseWorld)
        {
            var cell = grid.WorldToCell(mouseWorld);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                _isDragging = true;
                _start = cell;
                _last  = cell;
                RebuildPath(_start, cell);
                return;
            }

            switch (_isDragging)
            {
                case true when Mouse.current.leftButton.isPressed:
                {
                    if (cell == _last) return;
                    _last = cell;
                    RebuildPath(_start, cell);
                    return;
                }
                case true when Mouse.current.leftButton.wasReleasedThisFrame:
                    Commit();
                    _isDragging = false;
                    _hasHover   = false;
                    return;
            }

            if (_hasHover && _hoverCell == cell) return;
            _hoverCell = cell;
            _hasHover  = true;
            _scratchPath.Clear();
            _scratchPath.Add(cell);
            ShowPreviews(_scratchPath);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        public void Cancel()
        {
            _isDragging = false;
            _hasHover   = false;
            for (var i = 0; i < _activeCount; i++)
                _pool[i].gameObject.SetActive(false);
            _activeCount = 0;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void RebuildPath(Vector2Int a, Vector2Int b)
        {
            BuildLPath(a, b, _scratchPath);
            ShowPreviews(_scratchPath);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private static void BuildLPath(Vector2Int a, Vector2Int b, List<Vector2Int> outPath)
        {
            outPath.Clear();
            var dx = Mathf.Abs(b.x - a.x);
            var dy = Mathf.Abs(b.y - a.y);
            var sx = b.x > a.x ? 1 : -1;
            var sy = b.y > a.y ? 1 : -1;

            var horizontalFirst = dx >= dy;
            if (Keyboard.current?.shiftKey.isPressed == true)
                horizontalFirst = !horizontalFirst;

            int x = a.x, y = a.y;
            if (horizontalFirst)
            {
                while (x != b.x) { outPath.Add(new Vector2Int(x, y)); x += sx; }
                while (y != b.y) { outPath.Add(new Vector2Int(x, y)); y += sy; }
            }
            else
            {
                while (y != b.y) { outPath.Add(new Vector2Int(x, y)); y += sy; }
                while (x != b.x) { outPath.Add(new Vector2Int(x, y)); x += sx; }
            }
            outPath.Add(new Vector2Int(x, y));
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void ShowPreviews(List<Vector2Int> path)
        {
            EnsurePoolSize(path.Count);

            _tempRoads.Clear();
            foreach (var c in path)
                _tempRoads.Add(c);

            for (var i = 0; i < path.Count; i++)
            {
                var p = _pool[i];
                p.gameObject.SetActive(true);
                p.transform.position = grid.CellToWorld(path[i]);

                var canPlace = grid.CanBuildAt(path[i]) || grid.IsRoad(path[i]);
                p.ChangeState(canPlace
                    ? BuildingPreview.BuildingPreviewState.Valid
                    : BuildingPreview.BuildingPreviewState.Invalid);

                var (prefab, rotation) = RoadResolver.Resolve(path[i], grid, roadData, _tempRoads);
                if (prefab != null)
                    p.SwapModel(prefab, rotation);
            }

            for (var i = path.Count; i < _activeCount; i++)
                _pool[i].gameObject.SetActive(false);

            _activeCount = path.Count;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void EnsurePoolSize(int needed)
        {
            while (_pool.Count < needed)
            {
                var p = Instantiate(previewPrefab, transform);
                p.Setup(roadData.previewData);
                p.gameObject.SetActive(false);
                _pool.Add(p);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void Commit()
        {
            _toRecheck.Clear();

            foreach (var c in _scratchPath)
            {
                if (!grid.CanBuildAt(c) && !grid.IsRoad(c)) continue;
                if (grid.CanBuildAt(c))
                    grid.SetRoad(c);

                _toRecheck.Add(c + Vector2Int.up);
                _toRecheck.Add(c + Vector2Int.down);
                _toRecheck.Add(c + Vector2Int.right);
                _toRecheck.Add(c + Vector2Int.left);
            }

            foreach (var c in _scratchPath.Where(c => grid.IsRoad(c)))
                UpdateRoadVisual(c);

            foreach (var c in _toRecheck.Where(c => grid.IsRoad(c) && !_scratchPath.Contains(c)))
                UpdateRoadVisual(c);

            for (var i = 0; i < _activeCount; i++)
                _pool[i].gameObject.SetActive(false);
            _activeCount = 0;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void UpdateRoadVisual(Vector2Int cell)
        {
            var (prefab, rotation) = RoadResolver.Resolve(cell, grid, roadData);

            if (prefab == null)
            {
                Debug.LogWarning($"RoadData: kein Prefab für Zelle {cell}");
                return;
            }

            grid.SwapRoadModel(cell, prefab, rotation);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        public void UpdateRoadVisualPublic(Vector2Int cell) => UpdateRoadVisual(cell);
    }
}