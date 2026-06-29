using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//=== Can Özbal ===//

public class HappinessManager : MonoBehaviour
{
    // =========================
    // SINGLETON
    // =========================

    public static HappinessManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // =========================
    // HAPPINESS TYPES
    // =========================

    public enum HappinessType
    {
        Pollution,
        Crime,
        Traffic,
        Noise,
        Parks,
        Healthcare,
        Education,
        Taxes,
        Entertainment,
        Mobility,
        FairWages
    }

    // =========================
    // MODIFIER
    // =========================

    [System.Serializable]
    public class HappinessModifier
    {
        public HappinessType Type;

        public float Value;

        public bool IsPercent;

        public float Duration = -1f;

        public string Source = "";
    }

    // =========================
    // HAPPINESS BUILDING DATA
    // =========================

    [System.Serializable]
    public class HappinessBuildingData
    {
        public int BuildingID;

        [Range(0, 100)]
        public float BaseHappiness = 50f;

        [Range(0, 100)]
        public float CurrentHappiness = 50f;

        public List<HappinessModifier> Modifiers =
            new List<HappinessModifier>();

        [HideInInspector]
        public bool IsDirty = true;

        public Slider HappinessSlider;
    }

    // =========================
    // BUILDINGS
    // =========================

    public List<HappinessBuildingData> Buildings =
        new List<HappinessBuildingData>();

    // =========================
    // UPDATE LOOP
    // =========================

    private void Update()
    {
        for (int i = 0; i < Buildings.Count; i++)
        {
            HappinessBuildingData building = Buildings[i];

            UpdateModifierDurations(building);

            if (building.IsDirty)
            {
                RecalculateHappiness(building);

                building.IsDirty = false;
            }

            UpdateUI(building);
        }
    }

    // =========================
    // RECALCULATE
    // =========================

    private void RecalculateHappiness(HappinessBuildingData building)
    {
        float happiness = building.BaseHappiness;

        float percentMultiplier = 1f;

        foreach (var mod in building.Modifiers)
        {
            if (mod.IsPercent)
            {
                percentMultiplier += mod.Value;
            }
            else
            {
                happiness += mod.Value;
            }
        }

        happiness *= percentMultiplier;

        building.CurrentHappiness =
            Mathf.Clamp(happiness, 0f, 100f);
    }

    // =========================
    // UPDATE MODIFIER TIMERS
    // =========================

    private void UpdateModifierDurations(HappinessBuildingData building)
    {
        for (int i = building.Modifiers.Count - 1; i >= 0; i--)
        {
            HappinessModifier mod = building.Modifiers[i];

            if (mod.Duration > 0)
            {
                mod.Duration -= Time.deltaTime;

                if (mod.Duration <= 0)
                {
                    building.Modifiers.RemoveAt(i);

                    building.IsDirty = true;
                }
            }
        }
    }

    // =========================
    // ADD MODIFIER
    // =========================

    public void AddModifier(
        int buildingID,
        HappinessType type,
        float value,
        bool isPercent = false,
        float duration = -1f,
        string source = "")
    {
        HappinessBuildingData building =
            Buildings.Find(b => b.BuildingID == buildingID);

        if (building == null)
            return;

        building.Modifiers.Add(new HappinessModifier
        {
            Type = type,
            Value = value,
            IsPercent = isPercent,
            Duration = duration,
            Source = source
        });

        building.IsDirty = true;
    }

    // =========================
    // REMOVE MODIFIER
    // =========================

    public void RemoveModifier(
        int buildingID,
        HappinessType type,
        string source = "")
    {
        HappinessBuildingData building =
            Buildings.Find(b => b.BuildingID == buildingID);

        if (building == null)
            return;

        if (source != "")
            building.Modifiers.RemoveAll(
                m => m.Type == type && m.Source == source);
        else
            building.Modifiers.RemoveAll(
                m => m.Type == type);

        building.IsDirty = true;
    }

    // =========================
    // GET HAPPINESS
    // =========================

    public float GetHappiness(int buildingID)
    {
        HappinessBuildingData building =
            Buildings.Find(b => b.BuildingID == buildingID);

        if (building == null)
            return 0f;

        return building.CurrentHappiness;
    }

    // =========================
    // UI
    // =========================

    private void UpdateUI(HappinessBuildingData building)
    {
        if (building.HappinessSlider != null)
        {
            building.HappinessSlider.value =
                building.CurrentHappiness / 100f;
        }
    }

    // =========================
    // DEBUG TESTS
    // =========================

    [ContextMenu("Test Add Park Bonus")]
    private void TestParkBonus()
    {
        if (Buildings.Count == 0)
            return;

        AddModifier(
            Buildings[0].BuildingID,
            HappinessType.Parks,
            15f);
    }

    [ContextMenu("Test Pollution")]
    private void TestPollution()
    {
        if (Buildings.Count == 0)
            return;

        AddModifier(
            Buildings[0].BuildingID,
            HappinessType.Pollution,
            -25f);
    }
}