using UnityEngine;
using Andy.Manager;
//=== Andy ===//

public class ResumeButton : MonoBehaviour
{
    public void OnClick()
    {
        GameStateManager.Instance.SetState(GameState.Gameplay);
    }
}