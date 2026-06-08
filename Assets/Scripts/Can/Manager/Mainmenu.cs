using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//=== Can Özbal ===//

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Button settingsBackButton;

    private List<Resolution> _resolutions;

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
        Audiomanager.Instance.PlayMainMenu();
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
    #region Initialisation
    private void InitResolutionDropdown()
    {
        _resolutions = new List<Resolution>();
        var seen = new HashSet<string>();

        foreach (var r in Screen.resolutions)
        {
            var key = $"{r.width}x{r.height}";
            if (seen.Add(key))
                _resolutions.Add(r);
        }
        var options = new List<string>();
        var currentIndex = 0;
        var savedW = PlayerPrefs.GetInt("ResW", Screen.currentResolution.width);
        var savedH = PlayerPrefs.GetInt("ResH", Screen.currentResolution.height);
        for (var i = 0; i < _resolutions.Count; i++)
        {
            options.Add($"{_resolutions[i].width} x {_resolutions[i].height}");

            if (_resolutions[i].width == savedW && _resolutions[i].height == savedH)
                currentIndex = i;
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }
    private void InitGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new List<string> { "Low", "Medium", "High" });
        graphicsDropdown.value = PlayerPrefs.GetInt("Quality", 1);
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

    #endregion

    #region Menu Button Functions

    private static void StartGame()
    {
        Audiomanager.Instance.PlaySfx(1);
        SceneManager.LoadScene("Game");
    }

    private void OpenSettings()
    {
        Audiomanager.Instance.PlaySfx(1);
        ShowPanel(settingsPanel);
    }

    private void CloseSettings()
    {
        Audiomanager.Instance.PlaySfx(1);
        PlayerPrefs.Save();
        ShowPanel(mainMenuPanel);
    }

    private void QuitGame()
    {
        Audiomanager.Instance.PlaySfx(1);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion

    #region Settings Callbacks

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
           
        var maxLevel = QualitySettings.names.Length - 1;
        var mappedLevel = Mathf.RoundToInt(index * (maxLevel / 2f));
        QualitySettings.SetQualityLevel(mappedLevel);
        PlayerPrefs.SetInt("Quality", index);
    }

    private void OnResolutionChanged(int index)
    {
        var r = _resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResW", r.width);
        PlayerPrefs.SetInt("ResH", r.height);
    }

    #endregion

    #region Helpers

    private void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(panelToShow == mainMenuPanel);
        settingsPanel.SetActive(panelToShow == settingsPanel);
    }

    #endregion
}