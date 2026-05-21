using System.Collections.Generic;
using UnityEngine;

namespace Dennis.Placement
{
    public class BuildingPreview : MonoBehaviour
    {
        public enum BuildingPreviewState { Valid, Invalid }

        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        public BuildingPreviewState State { get; private set; } = BuildingPreviewState.Invalid;
        public BuildingData Data { get; private set; }
        public BuildingModel BuildingModel { get; private set; }

        private readonly List<Renderer> _renderers = new();
        private readonly List<Collider> _colliders = new();
        /////////////////////////////////////////////////////////////////////////////////////
        public void Setup(BuildingData data)
        {
            Data = data;
            BuildingModel = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
            _renderers.AddRange(BuildingModel.GetComponentsInChildren<Renderer>());
            _colliders.AddRange(BuildingModel.GetComponentsInChildren<Collider>());

            foreach (var col in _colliders) col.enabled = false;

            SetPreviewMaterial(State);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void ChangeState(BuildingPreviewState newState)
        {
            if (newState == State) return;
            State = newState;
            SetPreviewMaterial(State);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void Rotate(int rotationStep)
        {
            BuildingModel.Rotate(rotationStep);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void SetPreviewMaterial(BuildingPreviewState newState)
        {
            var previewMat = newState == BuildingPreviewState.Valid ? validMaterial : invalidMaterial;
            foreach (var rend in _renderers)
            {
                var mats = new Material[rend.sharedMaterials.Length];
                for (var i = 0; i < mats.Length; i++) mats[i] = previewMat;
                rend.materials = mats;
            }
        }
    }
}