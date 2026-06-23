using Andy.Manager;
using Andy.Manager.CityStats;
using Can.Manager;
using Can.Manager.CityStats;
using Dennis.Manager;
using UnityEngine;

namespace Samil.Manager
{
    public class EnergyBalanceManager : MonoBehaviour
    {
        public static EnergyBalanceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        [Header("Strom-Strafwerte")]
        [SerializeField] private float happinessPenalty = 20f;
        [SerializeField] private int goldPenaltyPerDay = 100;
        [SerializeField] private int announcementCooldownDays = 5;

        public float TotalEnergyDemand { get; private set; }
        public float EnergySupply => RenewableEnergy.Instance != null ? RenewableEnergy.Instance.TotalEnergyOutput : 0f;
        public float EnergyDeficit => Mathf.Max(0f, TotalEnergyDemand - EnergySupply);
        public bool IsUnderSupplied => TotalEnergyDemand > 0f && EnergyDeficit > 0f;

        private bool _wasUnderSupplied;
        private int _daysSinceLastAnnouncement;

        public void ProcessDayTick()
        {
            TotalEnergyDemand = 0f;
            if (CityTickManager.Instance != null)
                foreach (var b in CityTickManager.Instance.Buildings)
                    TotalEnergyDemand += b.EnergyUsage;

            if (TotalEnergyDemand <= 0f)
            {
                if (_wasUnderSupplied)
                    RemoveBlackoutEffects();
                _wasUnderSupplied = false;
                return;
            }

            if (IsUnderSupplied)
            {
                if (GreenCoinManager.Instance != null)
                    GreenCoinManager.Instance.SpendGold(goldPenaltyPerDay);

                if (!_wasUnderSupplied)
                {
                    ApplyBlackoutEffects();
                    _daysSinceLastAnnouncement = 0;
                }
                else
                {
                    _daysSinceLastAnnouncement++;
                    if (_daysSinceLastAnnouncement >= announcementCooldownDays)
                    {
                        ShowBlackoutAnnouncement();
                        _daysSinceLastAnnouncement = 0;
                    }
                }

                _wasUnderSupplied = true;
            }
            else
            {
                if (_wasUnderSupplied)
                    RemoveBlackoutEffects();
                _wasUnderSupplied = false;
            }
        }

        private void ApplyBlackoutEffects()
        {
            if (HappinessManager.Instance != null)
                HappinessManager.Instance.AddModifier(
                    0, HappinessManager.HappinessType.Noise, -happinessPenalty);

            if (HappinessUiManager.Instance != null)
                HappinessUiManager.Instance.happinessValue =
                    Mathf.Max(0f, HappinessUiManager.Instance.happinessValue - happinessPenalty);

            ShowBlackoutAnnouncement();
        }

        private void RemoveBlackoutEffects()
        {
            if (HappinessManager.Instance != null)
                HappinessManager.Instance.RemoveModifier(0, HappinessManager.HappinessType.Noise);

            if (HappinessUiManager.Instance != null)
                HappinessUiManager.Instance.happinessValue =
                    Mathf.Min(100f, HappinessUiManager.Instance.happinessValue + happinessPenalty);

            if (AnnouncementManager.Instance != null)
                AnnouncementManager.Instance.Show("Energieversorgung wiederhergestellt! Stromausfall beendet.", 4f);
        }

        private void ShowBlackoutAnnouncement()
        {
            if (AnnouncementManager.Instance != null)
                AnnouncementManager.Instance.Show(
                    $"Stromausfall! Bedarf {TotalEnergyDemand:F0} kW – Erzeugung {EnergySupply:F0} kW. " +
                    $"Happiness -{happinessPenalty:F0} | -{goldPenaltyPerDay} Gold/Tag", 6f);
        }
    }
}
