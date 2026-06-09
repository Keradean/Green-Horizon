using UnityEngine;
//=== Andy ===//

public class DestroyOnPlace : MonoBehaviour
{
    // Wird von BuildingSystem aufgerufen wenn das Gebäude geplaced wird
    public void OnPlaced()
    {
        Destroy(gameObject);
    }
}