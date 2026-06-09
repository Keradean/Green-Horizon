using UnityEngine;
using UnityEngine.UI;
//=== Andy ===//

namespace Andy.Manager
{
    public class DemolishButton : MonoBehaviour
    {
        [Header("Icons")]
        public Sprite iconNormal;       // Icon wenn nicht im Abriss-Modus
        public Sprite iconActive;       // Icon wenn im Abriss-Modus

        private Image _image;
        private bool _isActive = false;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Toggle()
        {
            _isActive = !_isActive;
            _image.sprite = _isActive ? iconActive : iconNormal;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            _image.sprite = _isActive ? iconActive : iconNormal;
        }
    }
}