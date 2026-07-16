using Dennis.Placement;
using Dennis.Placement.Building;
using UnityEngine;
using UnityEngine.InputSystem;
//=== Andy ===//
namespace Andy.Manager
{
    public class BottomBarToggle : MonoBehaviour
    {
        [Header("Bottom Bar")]
        public RectTransform bottomBar;

        [Header("Tab Manager")]
        public TabManager tabManager;

        [Header("Grid")]
        public GameObject gridVisual;           // Grid - Visual im Inspector zuweisen

        [Header("Einstellungen")]
        public float slideSpeed = 8f;
        
        
        private bool _isOpen = false;
        private float _closedY;                  // Position wenn geschlossen (unten versteckt)
        private float _openY = 150f;             // Position wenn offen

        private void Start()
        {
            _closedY = -bottomBar.rect.height;
            bottomBar.anchoredPosition = new Vector2(0, _closedY);
            gridVisual.SetActive(false);        // Grid startet unsichtbar
        }

        public void CloseBar()
        {
            _isOpen = false;
            gridVisual.SetActive(false);
        }

        private void Update()
        {
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused)
            {
                AnimateBar();
                return;
            }

            if (Keyboard.current[Key.Tab].wasPressedThisFrame)
            {
                if (BuildingSystem.Instance != null)
                    BuildingSystem.Instance.CancelAll();
                _isOpen = !_isOpen;
                gridVisual.SetActive(_isOpen);
                
                Dennis.Tutorial.TutorialManager.Instance?.NotifyEvent(Dennis.Tutorial.TutorialTrigger.Tab);
            }
            // Tabs per Zahlentasten
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame) tabManager.ShowPanel(0);
            if (Keyboard.current[Key.Digit2].wasPressedThisFrame) tabManager.ShowPanel(1);
            if (Keyboard.current[Key.Digit3].wasPressedThisFrame) tabManager.ShowPanel(2);
            if (Keyboard.current[Key.Digit4].wasPressedThisFrame) tabManager.ShowPanel(3);
            if (Keyboard.current[Key.Digit5].wasPressedThisFrame) tabManager.ShowPanel(4);
            if (Keyboard.current[Key.Digit6].wasPressedThisFrame)
            {
                tabManager.ShowPanel(5);
                Dennis.Tutorial.TutorialManager.Instance?.NotifyEvent(Dennis.Tutorial.TutorialTrigger.numbers);
            }
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