using UnityEngine;
using System;

public class CO2BudgetManager : MonoBehaviour
{
    public static CO2BudgetManager Instance { get; private set; }

    [Header("CO2 Budget Settings")]
    [Tooltip("Startwert des CO2-Fußabdrucks")]
    public float startFootprint = 0f;

    [Tooltip("Aktueller CO2-Fußabdruck")]
    public float currentFootprint;

    public event Action<float> OnBudgetChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        currentFootprint = startFootprint;
    }

    // Erhöhe den CO2-Fußabdruck je nach Stufe der schlechten Entscheidung
    // Stufe 1 = klein, Stufe 2 = mittel, Stufe 3 = groß
    public void AddBadDecision(int level)
    {
        float amount = 0f;
        switch (level)
        {
            case 1:
                amount = 10f; // Beispielwert für kleine schlechte Entscheidung
                break;
            case 2:
                amount = 50f; // Beispielwert für mittlere schlechte Entscheidung
                break;
            case 3:
                amount = 200f; // Beispielwert für große schlechte Entscheidung
                break;
            default:
                amount = 0f;
                break;
        }
        currentFootprint += amount;
        OnBudgetChanged?.Invoke(currentFootprint);
    }
    
    // Verringere den CO2-Fußabdruck je nach Stufe der guten Entscheidung
    // Stufe 1 = klein, Stufe 2 = mittel, Stufe 3 = groß
    public void AddGoodDecision(int level)
    {
        float amount = 0f;
        switch (level)
        {
            case 1:
                amount = 10f; // Beispielwert für kleine gute Entscheidung
                break;
            case 2:
                amount = 50f; // Beispielwert für mittlere gute Entscheidung
                break;
            case 3:
                amount = 200f; // Beispielwert für große gute Entscheidung
                break;
            default:
                amount = 0f;
                break;
        }

        // Fußabdruck verringern, aber nicht < 0
        currentFootprint = Mathf.Max(0f, currentFootprint - amount);
        OnBudgetChanged?.Invoke(currentFootprint);
    }

    public float GetCurrentFootprint()
    {
        return currentFootprint;
    }

    public void ResetFootprint()
    {
        currentFootprint = startFootprint;
        OnBudgetChanged?.Invoke(currentFootprint);
    }
}
