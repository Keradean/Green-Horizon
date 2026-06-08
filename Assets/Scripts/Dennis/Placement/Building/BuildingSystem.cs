using System.Collections.Generic;
using System.Linq;
using Dennis.Manager;
using Dennis.Placement.Road;
using UnityEngine;
using UnityEngine.InputSystem;

//*** De Col ***\\
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
        public const float CellSize = 0.5f;
        private BuildingPreview _preview;
        private Building _hoveredBuilding;
        private bool _isDemolishMode;
        private bool _isRoadMode;
        private Camera _camera;

        public bool ConsumedEscapeThisFrame { get; private set; }
        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            Instance = this;
            _camera = Camera.main;
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
            /////////////////////////////////////////////////////////////////////////////////////
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
            if (_hoveredBuilding == null) return;
            _hoveredBuilding.Unhighlight();
            _hoveredBuilding = null;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void HandleDemolishMode(Vector3 mousePos)
        {
            if (Keyboard.current.xKey.wasPressedThisFrame) { ExitDemolishMode(); return; }

            var building = grid.GetBuildingAt(mousePos);
            if (building != _hoveredBuilding)
            {
                if (_hoveredBuilding != null) _hoveredBuilding.Unhighlight();
                _hoveredBuilding = building;
                if (_hoveredBuilding != null) _hoveredBuilding.Highlight(demolishHighlightMaterial);
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame || _hoveredBuilding == null) return;
            grid.RemoveBuilding(_hoveredBuilding);
            Destroy(_hoveredBuilding.gameObject);
            _hoveredBuilding = null;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void HandlePreview(Vector3 mousePosition)
        {
            _preview.transform.position = mousePosition;

            var buildPosition = _preview.BuildingModel.GetAllBuildingPositions();
            var canBuild = grid.CanBuild(buildPosition);

            if (canBuild)
            {
                _preview.transform.position = GetSnappedCenterPosition(buildPosition);
                _preview.ChangeState(BuildingPreview.BuildingPreviewState.Valid);
                if (Mouse.current.leftButton.wasPressedThisFrame)
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
            if (_preview == null) return;
            if (GreenCoinManager.Instance.currentGold < _preview.Data.Cost) return;

            GreenCoinManager.Instance.SpendGold(_preview.Data.Cost);
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