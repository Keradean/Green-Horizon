using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Placement
{
    public class BuildingModel : MonoBehaviour
    {
        public float Rotation => wrapper.localEulerAngles.y;
        [SerializeField] private Transform wrapper;
        private BuildingShapeUnit[] _shapeUnits;
        /////////////////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            _shapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public void Rotate(float rotationStep)
        {
            wrapper.Rotate(new(0, rotationStep, 0));
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public List<Vector3> GetAllBuildingPositions()
        {
            return _shapeUnits.Select(unit => unit.transform.position).ToList();
        }
    }
}
