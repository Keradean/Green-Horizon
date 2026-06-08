using System.Collections.Generic;
using System.Linq;
using Dennis.Placement.Road;
using UnityEngine;
using UnityEngine.InputSystem;

//*** De Col ***//
namespace Dennis.Placement.Building
{
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }
        [SerializeField] private RoadDragHandler roadHandler;
        [SerializeField] private BuildingPreview buildingPreviewPrefab;
        [SerializeField] private Building buildingPrefab;
        [SerializeField] private BuildingGrid grid;
        [SerializeField] private Material demolishHighlightMaterial;
        public const float CellSize = 1f;
        private BuildingPreview _preview;
        private Building _hoveredBuilding;
        private bool _isDemolishMode;
        private bool _isRoadMode;
        private Camera _camera;

        private bool _isDraggingDemolish;
        private Vector2Int _demolishLast;
        private GameObject _demolishPreviewPlane;

        public bool ConsumedEscapeThisFrame { get; private set; }
        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            Instance = this;
            _camera = Camera.main;

            // Demolish Preview Plane erstellen
            _demolishPreviewPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _demolishPreviewPlane.GetComponent<Renderer>().material = demolishHighlightMaterial;
            _demolishPreviewPlane.transform.rotation = Quaternion.Euler(90, 0, 0);
            _demolishPreviewPlane.transform.localScale = new Vector3(CellSize, CellSize, CellSize);
            Destroy(_demolishPreviewPlane.GetComponent<Collider>());
            _demolishPreviewPlane.SetActive(false);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            ConsumedEscapeThisFrame = false;
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_preview != null)  { CancelPreview();    ConsumedEscapeThisFrame = true; return; }
                if (_isDemolishMode)   { ExitDemolishMode(); ConsumedEscapeThisFrame = true; return; }
                if (_isRoadMode)       { ExitRoadMode();     ConsumedEscapeThisFrame = true; return; }
                if (Andy.Manager.GameStateManager.Instance.CurrentGameState != Andy.Manager.GameState.Paused)
                {
                    Andy.Manager.GameStateManager.Instance.SetState(Andy.Manager.GameState.Paused);
                    ConsumedEscapeThisFrame = true;
                    return;
                }
            }

            if (Andy.Manager.GameStateManager.Instance.CurrentGameState == Andy.Manager.GameState.Paused) return;

            var mousePos = GetMousePosition();

            if (_isDemolishMode) { HandleDemolishMode(mousePos); return; }
            if (_isRoadMode)     { HandleRoadMode(mousePos);     return; }
            if (_preview != null){ HandlePreview(mousePos);      return; }

            if (Keyboard.current.xKey.wasPressedThisFrame)
                EnterDemolishMode();
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void StartPlacing(BuildingData data)
        {
            CancelAll();
            _preview = CreatePreview(data, GetMousePosition());
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void StartRoadMode()
        {
            CancelAll();
            EnterRoadMode();
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void StartDemolishMode()
        {
            CancelAll();
            EnterDemolishMode();
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void EnterRoadMode() => _isRoadMode = true;

        private void ExitRoadMode()
        {
            _isRoadMode = false;
            roadHandler.Cancel();
        }
        private void HandleRoadMode(Vector3 mousePos) => roadHandler.Tick(mousePos);
        private void EnterDemolishMode() => _isDemolishMode = true;
        private void ExitDemolishMode()
        {
            _isDemolishMode = false;
            _isDraggingDemolish = false;
            _demolishPreviewPlane.SetActive(false);
            if (_hoveredBuilding == null) return;
            _hoveredBuilding.Unhighlight();
            _hoveredBuilding = null;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void HandleDemolishMode(Vector3 mousePos)
        {
            if (Keyboard.current.xKey.wasPressedThisFrame) { ExitDemolishMode(); return; }

            var cell = grid.WorldToCell(mousePos);

            // Preview Plane positionieren
            if (grid.IsRoad(cell) || grid.GetBuildingAt(mousePos) != null)
            {
                _demolishPreviewPlane.SetActive(true);
                _demolishPreviewPlane.transform.position = grid.CellToWorld(cell) + new Vector3(0, 0.02f, 0);
            }
            else
            {
                _demolishPreviewPlane.SetActive(false);
            }

            // Straßen Drag-Demolish
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isDraggingDemolish = true;
                _demolishLast = cell;
                TryDemolishCell(cell);
                return;
            }

            if (_isDraggingDemolish && Mouse.current.leftButton.isPressed)
            {
                if (cell != _demolishLast)
                {
                    _demolishLast = cell;
                    TryDemolishCell(cell);
                }
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                    _isDraggingDemolish = true;
                    _demolishLast = cell;
                    TryDemolishCell(cell);
                    return;
                }

            // Gebäude hover highlight
            var building = grid.GetBuildingAt(mousePos);
            if (building != _hoveredBuilding)
            {
                if (_hoveredBuilding != null) _hoveredBuilding.Unhighlight();
                _hoveredBuilding = building;
                if (_hoveredBuilding != null) _hoveredBuilding.Highlight(demolishHighlightMaterial);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void TryDemolishCell(Vector2Int cell)
        {
            // Straße löschen
            if (grid.IsRoad(cell))
            {
                grid.RemoveRoad(cell);

                var neighbours = new[]
                {
                    cell + Vector2Int.up,
                    cell + Vector2Int.down,
                    cell + Vector2Int.right,
                    cell + Vector2Int.left
                };
                foreach (var n in neighbours.Where(n => grid.IsRoad(n)))
                    roadHandler.UpdateRoadVisualPublic(n);

                return;
            }

            // Gebäude löschen
            var building = grid.GetBuildingAt(grid.CellToWorld(cell));
            if (building == null) return;
            if (_hoveredBuilding == building)
            {
                _hoveredBuilding.Unhighlight();
                _hoveredBuilding = null;
            }
            grid.RemoveBuilding(building);
            Destroy(building.gameObject);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void HandlePreview(Vector3 mousePosition)
        {
            _preview.transform.position = mousePosition;

            var buildPosition = _preview.BuildingModel.GetAllBuildingPositions();
            var canBuild = grid.CanBuild(buildPosition) &&
                           buildPosition.All(p => !grid.IsRoad(grid.WorldToCell(p)));

            if (canBuild)
            {
                _preview.transform.position = GetSnappedCenterPosition(buildPosition);
                _preview.ChangeState(BuildingPreview.BuildingPreviewState.Valid);
                if (Mouse.current.leftButton.wasPressedThisFrame &&
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    PlaceBuilding(buildPosition);
            }
            else
            {
                _preview.ChangeState(BuildingPreview.BuildingPreviewState.Invalid);
            }

            if (Keyboard.current?.rKey.wasPressedThisFrame == true)
                _preview.Rotate(90);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void CancelPreview()
        {
            Destroy(_preview.gameObject);
            _preview = null;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void PlaceBuilding(List<Vector3> buildPosition)
        {
            var rotation = Quaternion.Euler(0, _preview.BuildingModel.Rotation, 0);
            var building = Instantiate(buildingPrefab, _preview.transform.position, rotation);
            building.Setup(_preview.Data, _preview.BuildingModel.Rotation);
            grid.SetBuilding(building, buildPosition);
            Destroy(_preview.gameObject);
            _preview = null;
        }

        public void CancelAll()
        {
            if (_preview != null) CancelPreview();
            if (_isDemolishMode)  ExitDemolishMode();
            if (_isRoadMode)      ExitRoadMode();
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private static Vector3 GetSnappedCenterPosition(List<Vector3> buildPosition)
        {
            var xs = buildPosition.Select(p => Mathf.FloorToInt(p.x)).ToList();
            var zs = buildPosition.Select(p => Mathf.FloorToInt(p.z)).ToList();
            var centerX = (xs.Min() + xs.Max()) / 2f + CellSize / 2f;
            var centerZ = (zs.Min() + zs.Max()) / 2f + CellSize / 2f;
            return new Vector3(centerX, 0, centerZ);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private Vector3 GetMousePosition()
        {
            if (!_camera || Mouse.current == null) return Vector3.zero;
            var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var groundPlane = new Plane(Vector3.up, grid.transform.position);
            return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
        {
            var preview = Instantiate(buildingPreviewPrefab, position, Quaternion.identity, transform);
            preview.Setup(data);
            return preview;
        }
    }
}