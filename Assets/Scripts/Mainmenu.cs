using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string settingsSceneName = "Settings";

    private void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    public void StartGame()
    {
        Debug.Log("Starting game...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Opening settings...");
        SceneManager.LoadScene(settingsSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}