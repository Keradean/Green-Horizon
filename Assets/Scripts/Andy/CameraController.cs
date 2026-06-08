using Andy.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//=== Andy ===//

namespace Andy
{
    public class CameraController : MonoBehaviour
    {
        [Header("Bewegung")]
        public float moveSpeed = 20f;
        public float smoothSpeed = 8f;

        [Header("Zoom")]
        public float zoomSpeed = 5f;
        public float minZoom = 5f;
        public float maxZoom = 30f;

        [Header("Rotation")]
        public bool enableRotation = true;      // Im Inspector ein/ausschalten
        public float rotationSpeed = 100f;

        [Header("Grenzen")]
        public Vector2 minBounds;
        public Vector2 maxBounds;

        private Vector3 _targetPosition;
        private float _targetZoom;
        private float _targetRotation;

        private void Start()
        {
            _targetPosition = transform.position;
            _targetZoom = transform.position.y;
            _targetRotation = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused) return;

            HandleMovement();
            HandleZoom();
            if (enableRotation) HandleRotation();
            ApplyMovement();
        }

        private void HandleMovement()
        {
            var input = Vector3.zero;

            if (Keyboard.current[Key.W].isPressed) input += Vector3.forward;
            if (Keyboard.current[Key.S].isPressed) input += Vector3.back;
            if (Keyboard.current[Key.A].isPressed) input += Vector3.left;
            if (Keyboard.current[Key.D].isPressed) input += Vector3.right;

            // Bewegung relativ zur Kamera-Rotation
            input = Quaternion.Euler(0, transform.eulerAngles.y, 0) * input;

            _targetPosition += input * (moveSpeed * Time.deltaTime);

            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minBounds.x, maxBounds.x);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, minBounds.y, maxBounds.y);
        }

        private void HandleZoom()
        {
            if (EventSystem.current == null) return; 
            // Nicht zoomen wenn Maus über UI ist
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            var scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll == 0) return;

            _targetZoom -= scroll * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
        }

        private void HandleRotation()
        {
            if (!Mouse.current.rightButton.isPressed) return;

            var mouseDelta = Mouse.current.delta.ReadValue();
            _targetRotation += mouseDelta.x * rotationSpeed * Time.deltaTime;
        }

        private void ApplyMovement()
        {
            var targetPos = new Vector3(_targetPosition.x, _targetZoom, _targetPosition.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, _targetRotation, 0), Time.deltaTime * smoothSpeed);
        }
    }
}