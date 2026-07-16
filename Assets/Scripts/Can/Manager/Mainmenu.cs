using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//=== Can Özbal ===//

namespace Can.Manager
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitButton;

        [SerializeField] private GameObject mainMenuPanel;

        [SerializeField] private Slider volumeSlider;

        private List<Resolution> _resolutions;

        private void Awake()
        {
            startGameButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(QuitGame);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        private void Start()
        {
            InitVolumeSlider();

            ShowPanel(mainMenuPanel);
            Audiomanager.Instance.PlayMainMenu();
        }

        private void OnDestroy()
        {
            startGameButton.onClick.RemoveListener(StartGame);
            quitButton.onClick.RemoveListener(QuitGame);
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        private void InitVolumeSlider()
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            AudioListener.volume = volumeSlider.value;
        }


        private static void StartGame()
        {
            Audiomanager.Instance.PlaySfx(1);
            Audiomanager.Instance.PlayBGM();
            SceneTransitionManager.LoadScene("MainScene");
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

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("Volume", value);
        }

        private void ShowPanel(GameObject panelToShow)
        {
            mainMenuPanel.SetActive(panelToShow == mainMenuPanel);
        }
    }
}