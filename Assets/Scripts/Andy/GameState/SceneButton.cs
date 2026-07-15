using UnityEngine;
using UnityEngine.SceneManagement;
//=== Andy ===//

public class SceneButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void OnClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}