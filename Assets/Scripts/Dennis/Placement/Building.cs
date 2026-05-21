using System.Collections.Generic;
using UnityEngine;

namespace Dennis.Placement
{
    public class Building : MonoBehaviour
    {
        public string Description => _data.Description;
        public int Cost => _data.Cost;
        private BuildingModel _model;
        private BuildingData _data;
        private readonly List<Renderer> _renderers = new();
        private Material[][] _originalMaterials;
        private bool _isHighlighted;
        /////////////////////////////////////////////////////////////////////////////////////
        public void Setup(BuildingData data, float rotation)
        {
            _data = data;
            _model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
            _model.Rotate(rotation);
            _renderers.AddRange(_model.GetComponentsInChildren<Renderer>());
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void Highlight(Material highlightMaterial)
        {
            if (_isHighlighted) return;
            _isHighlighted = true;
            _originalMaterials = new Material[_renderers.Count][];
            for (var i = 0; i < _renderers.Count; i++)
            {
                _originalMaterials[i] = _renderers[i].sharedMaterials;
                var mats = new Material[_renderers[i].sharedMaterials.Length];
                for (var j = 0; j < mats.Length; j++) mats[j] = highlightMaterial;
                _renderers[i].materials = mats;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void Unhighlight()
        {
            if (!_isHighlighted) return;
            _isHighlighted = false;
            for (var i = 0; i < _renderers.Count; i++)
            {
                _renderers[i].materials = _originalMaterials[i];
            }
        }
    }
}