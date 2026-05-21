using UnityEngine;
//=== Andy ===//

// Liegt auf dem PausePanel GameObject.
// Animiert das Pause-Menü von oben rein/raus wenn der GameState sich ändert.
namespace Andy.Manager
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Referenzen")]
        public BottomBarToggle bottomBarToggle;
        public RectTransform pauseMenu;         // Holder - PauseMenu

        [Header("Animation")]
        public float slideSpeed = 8f;
        public float openY;          // Zielposition: Mitte des Screens
        public float closedY  = 1500f;          // Startposition: oben außerhalb

        private bool _isOpen;
        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            // Sofort außerhalb des Screens positionieren
            pauseMenu.anchoredPosition = new Vector2(0, closedY);

            // Auf GameState-Änderungen hören
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

            // Direkt mit aktuellem State initialisieren
            OnGameStateChanged(GameStateManager.Instance.CurrentGameState);
        }
        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            //Debug.Log($"isOpen: {_isOpen} | Y: {pauseMenu.anchoredPosition.y}");
            var targetY = _isOpen ? openY : closedY;
            pauseMenu.anchoredPosition = Vector2.Lerp(
                pauseMenu.anchoredPosition,
                new Vector2(0, targetY),
                Time.deltaTime * slideSpeed
            );
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void OnGameStateChanged(GameState newGameState)
        {
            _isOpen = newGameState == GameState.Paused;

            // BottomBar schließen wenn Pause aufgeht
            if (newGameState == GameState.Paused && bottomBarToggle != null)
                bottomBarToggle.CloseBar();
        }
    }
}