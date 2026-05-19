using UnityEngine;
using UnityEngine.InputSystem;
//=== Andy ===//

// Hört auf Escape-Taste und togglet zwischen Gameplay und Paused
public class PauseControllerManager : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            // Aktuellen State holen und umschalten
            GameState current = GameStateManager.Instance.CurrentGameState;
            GameState next = current == GameState.Gameplay ? GameState.Paused : GameState.Gameplay;
            GameStateManager.Instance.SetState(next);
        }
    }
}