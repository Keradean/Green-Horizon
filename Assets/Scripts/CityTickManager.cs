using Andy.Manager;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField]
    private float daysPerSecond = 1f; // Anzahl der Tage, die pro Sekunde vergehen

    public int daysPassed = 0; // Anzahl der Tage, die seit Beginn des Spiels vergangen sind

    private float tickPassed = 0f;

    private void Update()
    {
        // Berechne die Anzahl der Tage, die seit dem letzten Frame vergangen sind
        if (GameStateManager.Instance.CurrentGameState == GameState.Paused) return;

        this.tickPassed += Time.deltaTime * daysPerSecond;
        int newDaysPassed = Mathf.FloorToInt(this.tickPassed);
        if (newDaysPassed > this.daysPassed)
        {
            UpdateDaysPassed(newDaysPassed);
        }
        this.daysPassed = newDaysPassed;
    }

    private void UpdateDaysPassed(int newDaysPassed)
    {
        // Hier kannst du Logik hinzufügen, die ausgeführt wird, wenn eine neue Anzahl von Tagen erreicht wird
        // Zum Beispiel könntest du Ereignisse auslösen oder den Zustand der Stadt aktualisieren
        Debug.Log("Tag updated " + newDaysPassed);
        CO2BudgetManager.Instance.AddBadDecision(1); // Beispiel: Erhöhe den CO2-Fußabdruck um eine kleine Menge pro Tag
        renewable_energy.Instance.Investments.ForEach(investment =>
        {
            if (investment.IsBuilt)
            {
                CO2BudgetManager.Instance.AddGoodDecision(investment.CO2ReductionLevel); // Beispiel: Reduziere den CO2-Fußabdruck basierend auf den gebauten Investitionen
            }
        });
    }
}
