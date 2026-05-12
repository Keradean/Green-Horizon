using System.Collections.Generic;
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

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown graphicsDropdown;   
    [SerializeField] private TMP_Dropdown resolutionDropdown;  
    [SerializeField] private Button settingsBackButton;

 
    private Resolution[] _resolutions;



    private void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        settingsBackButton.onClick.AddListener(CloseSettings);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        graphicsDropdown.onValueChanged.AddListener(OnGraphicsQualityChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void Start()
    {
        InitResolutionDropdown();
        InitGraphicsDropdown();
        InitVolumeSlider();
        InitFullscreenToggle();

        ShowPanel(mainMenuPanel);
    }

    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitButton.onClick.RemoveListener(QuitGame);
        settingsBackButton.onClick.RemoveListener(CloseSettings);

        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        graphicsDropdown.onValueChanged.RemoveListener(OnGraphicsQualityChanged);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
    }



    private void InitResolutionDropdown()
    {
        _resolutions = Screen.resolutions;

        var options = new List<string>();
        int savedIndex = PlayerPrefs.GetInt("Resolution", -1);
        int currentIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            Resolution r = _resolutions[i];
            options.Add($"{r.width} x {r.height}");

            if (savedIndex == i)
                currentIndex = i;
            else if (savedIndex == -1
                     && r.width  == Screen.currentResolution.width
                     && r.height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void InitGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new List<string>(QualitySettings.names));
        graphicsDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        graphicsDropdown.RefreshShownValue();
    }

    private void InitVolumeSlider()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        AudioListener.volume = volumeSlider.value;
    }

    private void InitFullscreenToggle()
    {
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = fullscreenToggle.isOn;
    }



    public void StartGame()
    {
        Debug.Log("Starting game...");
        SceneManager.LoadScene("Game");
    }

    public void OpenSettings()
    {
        Debug.Log("Opening settings...");
        ShowPanel(settingsPanel);
    }

    public void CloseSettings()
    {
        Debug.Log("Closing settings...");
        PlayerPrefs.Save();
        ShowPanel(mainMenuPanel);
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



    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    private void OnGraphicsQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
    }

    private void OnResolutionChanged(int index)
    {
        Resolution r = _resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", index);
    }



    private void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(panelToShow == mainMenuPanel);
        settingsPanel.SetActive(panelToShow == settingsPanel);
    }
}