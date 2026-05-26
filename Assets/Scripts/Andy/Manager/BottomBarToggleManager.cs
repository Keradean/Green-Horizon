using Dennis.Placement;
using UnityEngine;
using UnityEngine.InputSystem;
//=== Andy ===//
namespace Andy.Manager
{
    public class BottomBarToggle : MonoBehaviour
    {
        [Header("Bottom Bar")]
        public RectTransform bottomBar;         // Panel - BottomBar

        [Header("Tab Manager")]
        public TabManager tabManager;

        [Header("Einstellungen")]
        public KeyCode toggleKey = KeyCode.Tab; // Taste zum Öffnen/Schließen
        public float slideSpeed = 8f;           // Geschwindigkeit der Animation

        private bool _isOpen;
        private float _closedY;                  // Position wenn geschlossen (unten versteckt)
        private float _openY = 150f;             // Position wenn offen

        private void Start()
        {
            _closedY = -bottomBar.rect.height;
            bottomBar.anchoredPosition = new Vector2(0, _closedY);
        }
    
        public void CloseBar()
        {
            _isOpen = false;
        }

        private void Update()
        {
            // Nicht reagieren wenn pausiert - aber Animation noch laufen lassen
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused)
            {
                // Nur Animation updaten, kein Input
                AnimateBar();
                return;
            }

            // Bottom Bar togglen
            if (Keyboard.current[Key.Tab].wasPressedThisFrame)
            {
                if (BuildingSystem.Instance != null)
                    BuildingSystem.Instance.CancelAll();
                _isOpen = !_isOpen;
            }


            // Tabs per Zahlentasten
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame) tabManager.ShowPanel(0);
            if (Keyboard.current[Key.Digit2].wasPressedThisFrame) tabManager.ShowPanel(1);
            if (Keyboard.current[Key.Digit3].wasPressedThisFrame) tabManager.ShowPanel(2);
            if (Keyboard.current[Key.Digit4].wasPressedThisFrame) tabManager.ShowPanel(3);
            if (Keyboard.current[Key.Digit5].wasPressedThisFrame) tabManager.ShowPanel(4);

            AnimateBar();
        }

        private void AnimateBar()
        {
            var targetY = _isOpen ? _openY : _closedY;
            var target = new Vector2(bottomBar.anchoredPosition.x, targetY);
            bottomBar.anchoredPosition = Vector2.Lerp(bottomBar.anchoredPosition, target, Time.deltaTime * slideSpeed);
        }
    }
}