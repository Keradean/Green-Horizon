using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MainMenu
{
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
            // Deduplicate: keep only one entry per unique width x height
            _resolutions = new List<Resolution>();
            var seen = new HashSet<string>();

            foreach (Resolution r in Screen.resolutions)
            {
                string key = $"{r.width}x{r.height}";
                if (seen.Add(key))
                    _resolutions.Add(r);
            }

            var options = new List<string>();
            int currentIndex = 0;
            int savedW = PlayerPrefs.GetInt("ResW", Screen.currentResolution.width);
            int savedH = PlayerPrefs.GetInt("ResH", Screen.currentResolution.height);

            for (int i = 0; i < _resolutions.Count; i++)
            {
                options.Add($"{_resolutions[i].width} x {_resolutions[i].height}");

                if (_resolutions[i].width == savedW && _resolutions[i].height == savedH)
                    currentIndex = i;
            }

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);

            // Suppress the callback firing during init
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
            // 0 = Low, 1 = Medium, 2 = High — maps evenly across Unity quality levels
            int maxLevel = QualitySettings.names.Length - 1;
            int mappedLevel = Mathf.RoundToInt(index * (maxLevel / 2f));
            QualitySettings.SetQualityLevel(mappedLevel);
            PlayerPrefs.SetInt("Quality", index);
        }

        private void OnResolutionChanged(int index)
        {
            Resolution r = _resolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResW", r.width);
            PlayerPrefs.SetInt("ResH", r.height);
            Debug.Log($"Resolution changed to {r.width} x {r.height}");
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
}