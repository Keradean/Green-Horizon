using UnityEngine;
using System.Collections.Generic;

public class Ereignisse : MonoBehaviour
{
    public static Ereignisse Instance { get; private set; }

    private Queue<string> eventPool = new Queue<string>();
    private List<string> allEvents = new List<string>
    {
        "Dürren",
        "Überflutungen",
        "Hitzewellen",
        "Politische Unruhen"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        InitializePool();
    }

    private void InitializePool()
    {
        // Fülle den Pool mit allen Ereignis-Namen
        foreach (string eventName in allEvents)
        {
            eventPool.Enqueue(eventName);
        }
        Debug.Log($"Ereignis-Pool initialisiert mit {eventPool.Count} Ereignissen");
    }

    /// <summary>
    /// Holt das nächste Ereignis aus dem Pool
    /// </summary>
    public string Get()
    {
        if (eventPool.Count > 0)
        {
            string eventName = eventPool.Dequeue();
            Debug.Log($"[Pool] Ereignis abgerufen: {eventName}. Verbleibend: {eventPool.Count}");
            return eventName;
        }
        else
        {
            Debug.LogWarning("Ereignis-Pool ist leer!");
            return null;
        }
    }

    /// <summary>
    /// Gibt ein Ereignis an den Pool zurück
    /// </summary>
    public void Release(string eventName)
    {
        if (!string.IsNullOrEmpty(eventName))
        {
            eventPool.Enqueue(eventName);
            Debug.Log($"[Pool] Ereignis freigegeben: {eventName}. Verfügbar: {eventPool.Count}");
        }
        else
        {
            Debug.LogWarning("Versuch, null an den Pool zurückzugeben");
        }
    }

    /// <summary>
    /// Gibt die Anzahl der verfügbaren Ereignisse zurück
    /// </summary>
    public int GetAvailableEventCount()
    {
        return eventPool.Count;
    }

    /// <summary>
    /// Gibt alle Ereignis-Namen aus
    /// </summary>
    public List<string> GetAllEventNames()
    {
        return allEvents;
    }
}
