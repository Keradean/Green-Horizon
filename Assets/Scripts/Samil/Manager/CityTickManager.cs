using System.Collections.Generic;
using Andy.Manager;
using Dennis.Manager;
using Dennis.Placement.Building;
using Furkan;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Samil.Manager
{
    public class CityTickManager : MonoBehaviour
    {
        public static CityTickManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [SerializeField]
        private float daysPerSecond = 1f;

        [SerializeField]
        private TMPro.TMP_Text goldText;

        [SerializeField]
        private TMPro.TMP_Text residentText;

        public int daysPassed = 0;
        private float _tickPassed = 0f;
        private int _lastResidents = -1;

        public List<BuildingData> Buildings { get; private set; } = new List<BuildingData>();
        public void RefreshUI() => UpdateUI();

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var allTexts = FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Exclude);
            foreach (var text in allTexts)
            {
                if (text.name == "Einwohner - Text Amount")
                    residentText = text;
                if (text.name == "Money - Text Amount")
                    goldText = text;
            }

            UpdateUI();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused) return;

            _tickPassed += Time.deltaTime * daysPerSecond;
            var newDaysPassed = Mathf.FloorToInt(_tickPassed);
            if (newDaysPassed > daysPassed)
            {
                UpdateDaysPassed(newDaysPassed);
            }
            daysPassed = newDaysPassed;
        }

        private void UpdateDaysPassed(int newDaysPassed)
        {
            var totalIncome = 0;
            var totalPollution = 0f;
            var totalResidents = 0;

            Buildings.ForEach(building =>
            {
                totalIncome += building.IncomePerHour * 24;
                totalPollution += building.Pollution;
                totalResidents += building.Residents;
            });

            if (totalIncome > 0)
                GreenCoinManager.Instance.AddGold(totalIncome);

            if (CircularEconomyManager.Instance != null)
                totalPollution = CircularEconomyManager.Instance.ProcessDayTick(totalPollution);

            if (totalPollution > 0 && Co2BudgetManager.Instance != null)
                Co2BudgetManager.Instance.AddPollution(totalPollution);

            if (EnergyBalanceManager.Instance != null)
                EnergyBalanceManager.Instance.ProcessDayTick();

            if (FloatingNumberManager.Instance != null && totalResidents != _lastResidents)
            {
                var diff = totalResidents - _lastResidents;
                if (_lastResidents >= 0)
                    FloatingNumberManager.Instance.ShowResidentChange(diff);
                _lastResidents = totalResidents;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (goldText == null || residentText == null) return;
            goldText.text = GreenCoinManager.Instance.CurrentGold.ToString("N0");
            var residents = 0;
            Buildings.ForEach(building => residents += building.Residents);
            residentText.text = residents.ToString("N0");
        }
    }
}