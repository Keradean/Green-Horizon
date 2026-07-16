using UnityEngine;
using UnityEngine.InputSystem;

//=== Samil ===//

namespace Samil.Manager
{
    /// <summary>
    /// Lässt UI-Elemente im Hauptmenü der Maus sanft entgegengesetzt zur Kamera folgen,
    /// für einen Tiefen-/Parallax-Effekt wie im osu!-Hauptmenü.
    /// </summary>
    public class MainMenuUIParallax : MonoBehaviour
    {
        [Header("Bewegung")]
        [Tooltip("Wie weit sich das UI maximal verschiebt (in Pixeln).")]
        [SerializeField] private float maxOffset = 15f;

        [Header("Glättung")]
        [Tooltip("Kleiner = träger/weicher, größer = direkter. Ca. 0.15 - 0.4 fühlt sich clean an.")]
        [SerializeField] private float smoothTime = 0.25f;

        private RectTransform _rectTransform;
        private Vector2 _startPosition;
        private Vector2 _velocity;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _startPosition = _rectTransform.anchoredPosition;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            var mousePos = mouse.position.ReadValue();
            var normalizedX = Mathf.Clamp(mousePos.x / Screen.width * 2f - 1f, -1f, 1f);
            var normalizedY = Mathf.Clamp(mousePos.y / Screen.height * 2f - 1f, -1f, 1f);

            // Entgegengesetzt zur Kamerabewegung -> negatives Vorzeichen.
            var targetOffset = new Vector2(-normalizedX * maxOffset, -normalizedY * maxOffset);
            _rectTransform.anchoredPosition = Vector2.SmoothDamp(
                _rectTransform.anchoredPosition,
                _startPosition + targetOffset,
                ref _velocity,
                smoothTime);
        }
    }
}