using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Placement
{
    public class BuildingSystem : MonoBehaviour
    {
        public const float CellSize = 1f;
        [SerializeField] private BuildingData smallHouse;
        [SerializeField] private BuildingPreview buildingPreviewPrefab;
        [SerializeField] private Building buildingPrefab;
        [SerializeField] private BuildingGrid grid;
        private BuildingPreview _preview;
        private Camera _camera;
        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            _camera = Camera.main;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            var mousePos = GetMousePosition();
            if (_preview != null)
            {
                HandlePreview(mousePos);
            }
            else
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame)
                {
                    _preview = CreatePreview(smallHouse, mousePos);
                }
            }
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
                {
                    PlaceBuilding(buildPosition);
                }
            }
            else
            {
                _preview.ChangeState(BuildingPreview.BuildingPreviewState.Invalid);
            }
            if (Keyboard.current?.rKey.wasPressedThisFrame == true)
            {
                _preview.Rotate(90);
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void PlaceBuilding(List<Vector3> buildPosition)
        {
            var building = Instantiate(buildingPrefab, _preview.transform.position, Quaternion.identity);
            building.Setup(_preview.Data, _preview.BuildingModel.Rotation);
            grid.SetBuilding(building, buildPosition);
            Destroy(_preview.gameObject);
            _preview = null;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private Vector3 GetSnappedCenterPosition(List<Vector3> buildPosition)
        {
            var xs = buildPosition.Select(p => Mathf.FloorToInt(p.x)).ToList();
            var zs = buildPosition.Select(p => Mathf.FloorToInt(p.z)).ToList();
            var centerX = (xs.Min() + xs.Max()) / 2f + CellSize / 2f;
            var centerZ = (zs.Min() + zs.Max()) / 2f + CellSize / 2f;
            return new(centerX, 0, centerZ);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private Vector3 GetMousePosition()
        {
            if (!_camera || Mouse.current == null) return Vector3.zero;
            var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new(Vector3.up, Vector3.zero);
            return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
        {
            var buildingPreview = Instantiate(buildingPreviewPrefab, position, Quaternion.identity, transform);
            buildingPreview.Setup(data);
            return buildingPreview;
        }
    }
}