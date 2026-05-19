using UnityEngine;
//=== Andy ===//

// Liegt auf Panel - Pause
// Animiert Holder - PauseMenu von oben rein/raus
public class PauseMenuController : MonoBehaviour
{
    public BottomBarToggle bottomBarToggle;
    public RectTransform pauseMenu;     // Holder - PauseMenu im Inspector zuweisen
    public float slideSpeed = 8f;

    private float openY = 0f;          // Zielposition in der Mitte
    private float closedY = 1500f;     // Startposition oben außerhalb
    private bool isOpen = false;

    void Awake()
    {
        pauseMenu.anchoredPosition = new Vector2(0, closedY);

        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        OnGameStateChanged(GameStateManager.Instance.CurrentGameState);
    }

    void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    void Update()
    {
        // Holder - PauseMenu von oben rein/raus animieren
        float targetY = isOpen ? openY : closedY;
        pauseMenu.anchoredPosition = Vector2.Lerp(
            pauseMenu.anchoredPosition,
            new Vector2(0, targetY),
            Time.deltaTime * slideSpeed
        );
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        isOpen = newGameState == GameState.Paused;
        gameObject.SetActive(true);     // Panel immer aktiv für Animation

        if (newGameState == GameState.Paused)
            bottomBarToggle.CloseBar();
    }
}