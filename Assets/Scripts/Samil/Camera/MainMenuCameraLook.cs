using UnityEngine;
using UnityEngine.InputSystem;

//=== Samil ===//

namespace Samil.Manager
{
    /// <summary>
    /// Lässt die Kamera im Hauptmenü sanft der Mausposition folgen (Parallax-Effekt),
    /// ähnlich dem Menü-Feeling von osu! oder dem Inventar in Apex Legends.
    /// </summary>
    public class MainMenuCameraLook : MonoBehaviour
    {
        [Header("Bewegung")]
        [Tooltip("Wie weit sich die Kamera maximal seitlich/vertikal verschiebt.")]
        [SerializeField] private float maxOffset = 0.4f;

        [Header("Rotation")]
        [Tooltip("Wie stark die Kamera sich maximal neigt/dreht (in Grad).")]
        [SerializeField] private float maxTilt = 3f;

        [Header("Glättung")]
        [Tooltip("Kleiner = träger/weicher, größer = direkter. Ca. 0.15 - 0.4 fühlt sich clean an.")]
        [SerializeField] private float smoothTime = 0.25f;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private Vector3 _positionVelocity;
        private Vector2 _rotationVelocity;
        private Vector2 _currentTilt;

        private void Awake()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Mausposition auf -1..1 normalisieren, Ursprung in Bildschirmmitte.
            var mousePos = mouse.position.ReadValue();
            var normalizedX = Mathf.Clamp(mousePos.x / Screen.width * 2f - 1f, -1f, 1f);
            var normalizedY = Mathf.Clamp(mousePos.y / Screen.height * 2f - 1f, -1f, 1f);

            var targetOffset = new Vector3(normalizedX * maxOffset, normalizedY * maxOffset, 0f);
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                _startPosition + targetOffset,
                ref _positionVelocity,
                smoothTime);

            var targetTilt = new Vector2(-normalizedY * maxTilt, normalizedX * maxTilt);
            _currentTilt.x = Mathf.SmoothDamp(_currentTilt.x, targetTilt.x, ref _rotationVelocity.x, smoothTime);
            _currentTilt.y = Mathf.SmoothDamp(_currentTilt.y, targetTilt.y, ref _rotationVelocity.y, smoothTime);

            transform.localRotation = _startRotation * Quaternion.Euler(_currentTilt.x, _currentTilt.y, 0f);
        }
    }
}