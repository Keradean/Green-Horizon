using UnityEngine;
using System;
using System.Collections.Generic;

namespace Furkan.Ereignisse
{
    public class Ereignisse : MonoBehaviour
    {
        public static Ereignisse Instance { get; private set; }

        private Dictionary<string, Action> events;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // Dictionary mit allen Event-Namen und deren Funktionen
            events = new Dictionary<string, Action>
            {
                { "Dürren", TriggerDrought },
                { "Überflutungen", TriggerFloods },
                { "Hitzewellen", TriggerHeatWave },
                { "Politische Unruhen", TriggerPoliticalUnrest }
            };

            Debug.Log($"Events initialisiert mit {events.Count} Einträgen");
        }

        // Event-Funktionen
        private void TriggerDrought()
        {
            Debug.Log("🌵 Dürren-Event ausgelöst!");
        }

        private void TriggerFloods()
        {
            Debug.Log("💧 Überflutungs-Event ausgelöst!");
        }

        private void TriggerHeatWave()
        {
            Debug.Log("Hitzewellen-Event ausgelöst!");
        }

        private void TriggerPoliticalUnrest()
        {
            Debug.Log(" Politische Unruhen-Event ausgelöst!");
        }

        /// Ruft ein Event anhand des Namens auf

        /// <param name="eventName">Name des Events: "Dürren", "Überflutungen", "Hitzewellen", "Politische Unruhen"</param>
        public void TriggerEvent(string eventName)
        {
            if (events.ContainsKey(eventName))
            {
                Debug.Log($"✓ Event '{eventName}' wird aufgerufen...");
                events[eventName]?.Invoke();
            }
            else
            {
                Debug.LogWarning($"✗ Event '{eventName}' nicht gefunden!");
            }
        }

        /// <summary>
        /// Gibt alle verfügbaren Event-Namen zurück
        /// </summary>
        public List<string> GetAllEventNames()
        {
            return new List<string>(events.Keys);
        }
    }
}
