using Dennis.Placement;
using UnityEngine;
using UnityEngine.InputSystem;
//=== Andy ===//

namespace Andy.Manager
{
    // Zuständig nur für Resume (Paused → Gameplay).
    // Das Einleiten der Pause macht BuildingSystem.
    // Wenn BuildingSystem Escape bereits verbraucht hat (ConsumedEscapeThisFrame),
    // reagiert dieser Manager nicht.
    public class PauseControllerManager : MonoBehaviour
    {
        private void Update()
        {
            if (!Keyboard.current[Key.Escape].wasPressedThisFrame) return;

            // BuildingSystem hat Escape bereits verbraucht → nicht eingreifen
            if (BuildingSystem.Instance != null && BuildingSystem.Instance.ConsumedEscapeThisFrame) return;

            // Nur Resume behandeln – Pause wird von BuildingSystem eingeleitet
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused)
                GameStateManager.Instance.SetState(GameState.Gameplay);
        }
    }
}