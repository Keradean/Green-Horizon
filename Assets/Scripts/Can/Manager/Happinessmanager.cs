using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("Type")] public HappinessType type;

        [FormerlySerializedAs("Value")] public float value;

        [FormerlySerializedAs("IsPercent")] public bool isPercent;

        [FormerlySerializedAs("Duration")] public float duration = -1f;
    }

    // =========================
    // HAPPINESS BUILDING DATA
    // =========================

    [System.Serializable]
    public class HappinessBuildingData
    {
        [FormerlySerializedAs("BuildingID")] public int buildingID;

        [FormerlySerializedAs("BaseHappiness")] [Range(0, 100)]
        public float baseHappiness = 50f;

        [FormerlySerializedAs("CurrentHappiness")] [Range(0, 100)]
        public float currentHappiness = 50f;

        [FormerlySerializedAs("Modifiers")] public List<HappinessModifier> modifiers =
            new List<HappinessModifier>();

        [FormerlySerializedAs("IsDirty")] [HideInInspector]
        public bool isDirty = true;

        [FormerlySerializedAs("HappinessSlider")] public Slider happinessSlider;
    }

    // =========================
    // BUILDINGS
    // =========================

    [FormerlySerializedAs("Buildings")] public List<HappinessBuildingData> buildings =
        new List<HappinessBuildingData>();

    // =========================
    // UPDATE LOOP
    // =========================

    private void Update()
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            HappinessBuildingData building = buildings[i];

            UpdateModifierDurations(building);

            if (building.isDirty)
            {
                RecalculateHappiness(building);

                building.isDirty = false;
            }

            UpdateUI(building);
        }
    }

    // =========================
    // RECALCULATE
    // =========================

    private void RecalculateHappiness(HappinessBuildingData building)
    {
        float happiness = building.baseHappiness;

        float percentMultiplier = 1f;

        foreach (var mod in building.modifiers)
        {
            if (mod.isPercent)
            {
                percentMultiplier += mod.value;
            }
            else
            {
                happiness += mod.value;
            }
        }

        happiness *= percentMultiplier;

        building.currentHappiness =
            Mathf.Clamp(happiness, 0f, 100f);
    }

    // =========================
    // UPDATE MODIFIER TIMERS
    // =========================

    private void UpdateModifierDurations(HappinessBuildingData building)
    {
        for (int i = building.modifiers.Count - 1; i >= 0; i--)
        {
            HappinessModifier mod = building.modifiers[i];

            if (mod.duration > 0)
            {
                mod.duration -= Time.deltaTime;

                if (mod.duration <= 0)
                {
                    building.modifiers.RemoveAt(i);

                    building.isDirty = true;
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
        float duration = -1f)
    {
        HappinessBuildingData building =
            buildings.Find(b => b.buildingID == buildingID);

        if (building == null)
            return;

        building.modifiers.Add(new HappinessModifier
        {
            type = type,
            value = value,
            isPercent = isPercent,
            duration = duration
        });

        building.isDirty = true;
    }

    // =========================
    // REMOVE MODIFIER
    // =========================

    public void RemoveModifier(
        int buildingID,
        HappinessType type)
    {
        HappinessBuildingData building =
            buildings.Find(b => b.buildingID == buildingID);

        if (building == null)
            return;

        building.modifiers.RemoveAll(
            m => m.type == type);

        building.isDirty = true;
    }

    // =========================
    // GET HAPPINESS
    // =========================

    public float GetHappiness(int buildingID)
    {
        HappinessBuildingData building =
            buildings.Find(b => b.buildingID == buildingID);

        if (building == null)
            return 0f;

        return building.currentHappiness;
    }

    // =========================
    // UI
    // =========================

    private void UpdateUI(HappinessBuildingData building)
    {
        if (building.happinessSlider != null)
        {
            building.happinessSlider.value =
                building.currentHappiness / 100f;
        }
    }

    // =========================
    // DEBUG TESTS
    // =========================

    [ContextMenu("Test Add Park Bonus")]
    private void TestParkBonus()
    {
        if (buildings.Count == 0)
            return;

        AddModifier(
            buildings[0].buildingID,
            HappinessType.Parks,
            15f);
    }

    [ContextMenu("Test Pollution")]
    private void TestPollution()
    {
        if (buildings.Count == 0)
            return;

        AddModifier(
            buildings[0].buildingID,
            HappinessType.Pollution,
            -25f);
    }
}