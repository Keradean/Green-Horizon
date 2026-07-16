using Can.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
//=== Andy ===//

public class SceneButton : MonoBehaviour
{
    public void OnClick()
    {
        SceneTransitionManager.LoadScene("MainMenu");
    }
}