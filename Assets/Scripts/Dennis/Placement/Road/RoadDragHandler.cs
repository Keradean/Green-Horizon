using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Dennis.Placement.Building; // für BuildingGrid
//*** De Col ***\\
namespace Dennis.Placement.Road
{
    public class RoadDragHandler : MonoBehaviour
    {
        [SerializeField] private RoadData roadData;
        [SerializeField] private BuildingPreview previewPrefab;
        [SerializeField] private BuildingGrid grid;

        private readonly List<BuildingPreview> _pool        = new();
        private readonly List<Vector2Int>      _scratchPath = new();
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

            // Hover – einzelne Preview unter dem Cursor
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

            for (var i = 0; i < path.Count; i++)
            {
                var p = _pool[i];
                p.gameObject.SetActive(true);
                p.transform.position = grid.CellToWorld(path[i]);

                var canBuild = grid.CanBuildAt(path[i]);
                p.ChangeState(canBuild
                    ? BuildingPreview.BuildingPreviewState.Valid
                    : BuildingPreview.BuildingPreviewState.Invalid);
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
                p.Setup(roadData);
                p.gameObject.SetActive(false);
                _pool.Add(p);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void Commit()
        {
            // 1. Alle neuen Zellen als Straße im Grid markieren
            foreach (var c in _scratchPath)
            {
                if (!grid.CanBuildAt(c)) continue;
                grid.SetRoad(c, null);
            }

            // 2. Alle betroffenen Zellen + Nachbarn sammeln
            var toUpdate = new HashSet<Vector2Int>();
            foreach (var c in _scratchPath)
            {
                toUpdate.Add(c);
                toUpdate.Add(c + Vector2Int.up);
                toUpdate.Add(c + Vector2Int.down);
                toUpdate.Add(c + Vector2Int.right);
                toUpdate.Add(c + Vector2Int.left);
            }

            // 3. Jede Straßenzelle neu auswerten und visuell updaten
            foreach (var c in toUpdate.Where(c => grid.IsRoad(c)))
            {
                UpdateRoadVisual(c);
            }

            // Previews ausblenden
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
                Debug.LogWarning($"RoadData: kein Prefab für Zelle {cell} – bitte im Inspector zuweisen.");
                return;
            }

            var world     = grid.CellToWorld(cell);
            var newObject = Instantiate(prefab, world, Quaternion.Euler(0, rotation, 0));

            grid.ReplaceRoadObject(cell, newObject);
        }
    }
}