//=== Andy ===//

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Andy.Manager
{
    public class GameStateManager
    {
        private static GameStateManager _instance;      // Überall erreichbar mit: GameStateManager.Instance
    
        public static GameStateManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameStateManager();
                return _instance;
            }
        }

        public GameState CurrentGameState { get; private set; }
    
        // Andere Scripts können sich hier anmelden um auf State-Änderungen zu reagieren
        public delegate void GameStateChangeHandler(GameState newGameState);
        public event GameStateChangeHandler OnGameStateChanged;


        private GameStateManager()
        {
            // Der Manager überlebt Scene-Wechsel (statisches Singleton), der State
            // darf aber nicht hängen bleiben: neue Scene startet immer im Gameplay,
            // sonst ist z.B. das Pause-Menü nach Rückkehr ins MainScene noch offen.
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetState(GameState.Gameplay);
        }

        // State setzen - ignoriert wenn neuer State gleich dem aktuellen ist
        public void SetState(GameState newGameState)
        {
            if (newGameState == CurrentGameState)
                return;

            CurrentGameState = newGameState;
            //Debug.Log("GameStateManager: SetState → " + newGameState);
            OnGameStateChanged?.Invoke(newGameState);   // Alle angemeldeten Scripts benachrichtigen
        }
    }
}