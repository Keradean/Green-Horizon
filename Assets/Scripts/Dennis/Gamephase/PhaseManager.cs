using UnityEngine;
using System;
using System.Linq;
using Samil.Manager;
using Furkan;
//*** De Col ***//
namespace Dennis.Gamephase
{
    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }

        public GamePhase CurrentPhase { get; private set; } = GamePhase.Start;

        public event Action<GamePhase> OnPhaseChanged;

        [Header("Phase Thresholds")]
        public int residentsForGrowth   = 50;
        public float co2ForCrisis       = 0.7f;
        public int residentsForChange   = 200;
        public float co2ForChange       = 0.4f;

        [Header("CO2 Normalization")]
        [Tooltip("At this CO2 value (tons) co2 = 1.0")]
        public float maxFootprint       = 1000f;

        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            CheckPhaseTransition();
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void CheckPhaseTransition()
        {
            var residents = GetResidents();
            var co2     = GetCO2();
            var newPhase  = CurrentPhase;

            switch (CurrentPhase)
            {
                case GamePhase.Start:
                    if (residents >= residentsForGrowth)
                        newPhase = GamePhase.Growth;
                    break;

                case GamePhase.Growth:
                    if (co2 >= co2ForCrisis)
                        newPhase = GamePhase.Crisis;
                    break;

                case GamePhase.Crisis:
                    if (residents >= residentsForChange && co2 <= co2ForChange)
                        newPhase = GamePhase.Change;
                    break;

                case GamePhase.Change:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (newPhase != CurrentPhase)
                SwitchPhase(newPhase);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void SwitchPhase(GamePhase newPhase)
        {
            Debug.Log($"Phase switch: {CurrentPhase} -> {newPhase}");
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private int GetResidents()
        {
            return CityTickManager.Instance == null ? 0 : CityTickManager.Instance.Buildings.Sum(b => b.Residents);
        }

        private float GetCO2()
        {
            return Co2BudgetManager.Instance == null ? 0f : Mathf.Clamp01(Co2BudgetManager.Instance.currentFootprint / maxFootprint);
        }
    }
}