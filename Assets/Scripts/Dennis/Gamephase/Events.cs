using System.Collections.Generic;
using UnityEngine;
//*** De Col ***//
namespace Dennis.Gamephase
{
    public class Events : MonoBehaviour
    {
        public static Events Instance { get; private set; }

        private readonly Queue<string> _eventPool = new();
        private readonly List<string> _allEvents = new()
        {
            "Drought",
            "Flood",
            "Heat Wave",
            "Political Unrest"
        };

        private float _timer;

        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= GetEventInterval())
            {
                _timer = 0f;
                TriggerNextEvent();
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private float GetEventInterval()
        {
            if (PhaseManager.Instance == null) return 60f;

            return PhaseManager.Instance.CurrentPhase switch
            {
                GamePhase.Start  => 120f,   // every 2 minutes
                GamePhase.Growth =>  60f,   // every 1 minute
                GamePhase.Crisis =>  20f,   // every 20 seconds
                GamePhase.Change =>  90f,   // calmer again
                _                =>  60f
            };
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void TriggerNextEvent()
        {
            string ev = Get();
            if (ev == null) return;

            Debug.Log($"[Event] {ev} triggered! (Phase: {PhaseManager.Instance?.CurrentPhase})");

            // TODO: show UI popup, apply CO2 impact etc.

            Release(ev);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void InitializePool()
        {
            foreach (string ev in _allEvents)
                _eventPool.Enqueue(ev);
        }

        public string Get()
        {
            if (_eventPool.Count > 0) return _eventPool.Dequeue();
            Debug.LogWarning("Event pool is empty!");
            return null;
        }

        public void Release(string ev)
        {
            if (!string.IsNullOrEmpty(ev))
                _eventPool.Enqueue(ev);
        }

        public int GetAvailableEventCount()    => _eventPool.Count;
        public List<string> GetAllEventNames() => _allEvents;
    }
}