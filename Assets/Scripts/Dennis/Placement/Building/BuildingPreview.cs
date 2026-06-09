using System.Collections.Generic;
using Andy;
using UnityEngine;
//*** De Col ***\\
//=== Andy ===//
namespace Dennis.Placement.Building
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

            // Nur Renderer sammeln die KEIN DestroyOnPlace haben (also kein Pfeil)
            foreach (var r in BuildingModel.GetComponentsInChildren<Renderer>())
            {
                if (r.GetComponentInParent<DestroyOnPlace>() == null)
                    _renderers.Add(r);
            }

            _colliders.AddRange(BuildingModel.GetComponentsInChildren<Collider>());
            foreach (var col in _colliders) col.enabled = false;
            SetPreviewMaterial(State);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        // Tauscht das angezeigte Modell für die Road Preview
        public void SwapModel(GameObject prefab, float rotation)
        {
            // Altes Modell löschen
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            _renderers.Clear();

            // Neues Modell instantiieren
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(0, rotation, 0);

            // Collider deaktivieren
            foreach (var col in go.GetComponentsInChildren<Collider>())
                col.enabled = false;

            // Renderer neu sammeln und Preview Material anwenden
            _renderers.AddRange(go.GetComponentsInChildren<Renderer>());
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
            BuildingModel?.Rotate(rotationStep);
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