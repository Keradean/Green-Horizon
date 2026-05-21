//=== Andy ===//

using UnityEngine;

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

        private GameStateManager() { }

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