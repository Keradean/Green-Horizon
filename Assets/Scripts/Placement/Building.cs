using UnityEngine;

namespace Placement
{
    public class Building : MonoBehaviour
    {
        public string Description => _data.Description;
        public int Cost => _data.Cost;
        private BuildingModel _model;
        private BuildingData _data; 
        /////////////////////////////////////////////////////////////////////////////////////
        public void Setup(BuildingData data, float rotation)
        {
            this._data = data;
            _model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
            _model.Rotate(rotation);
        }
    }
}
