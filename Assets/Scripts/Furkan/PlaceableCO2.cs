using UnityEngine;

public class PlaceableCO2 : MonoBehaviour
{
    [Header("CO2-Stufe dieses Prefabs (1=klein, 2=mittel, 3=groß)")]
    [Range(1, 3)]
    public int co2Level = 1;

    [Tooltip("Wenn true: gute Entscheidung (verringert CO2). Wenn false: schlechte Entscheidung (erhöht CO2).")]
    public bool isGoodDecision = false;

    private bool hasBeenPlaced = false;

    public void OnPlaced()
    {
        if (!hasBeenPlaced)
        {
            if (CO2BudgetManager.Instance == null)
            {
                Debug.LogWarning("CO2BudgetManager nicht gefunden. Bitte füge das Manager-GameObject zur Szene hinzu.");
                return;
            }

            if (isGoodDecision)
                CO2BudgetManager.Instance.AddGoodDecision(co2Level);
            else
                CO2BudgetManager.Instance.AddBadDecision(co2Level);

            hasBeenPlaced = true;
        }
    }
}
