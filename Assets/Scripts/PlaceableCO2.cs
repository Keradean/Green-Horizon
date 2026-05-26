using UnityEngine;

public class PlaceableCO2 : MonoBehaviour
{
    [Header("CO2-Stufe dieses Prefabs (1=klein, 2=mittel, 3=groß)")]
    [Range(1, 3)]
    public int co2Level = 1;

    private bool hasBeenPlaced = false;

    // Diese Methode sollte aufgerufen werden, wenn das Prefab platziert wird
    public void OnPlaced()
    {
        if (!hasBeenPlaced)
        {
            CO2BudgetManager.Instance.AddBadDecision(co2Level);
            hasBeenPlaced = true;
        }
    }
}
