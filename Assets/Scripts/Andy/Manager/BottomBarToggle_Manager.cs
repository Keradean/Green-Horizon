using UnityEngine;
using UnityEngine.InputSystem;
//=== Andy ===//

public class BottomBarToggle : MonoBehaviour
{
    [Header("Bottom Bar")]
    public RectTransform bottomBar;         // Panel - BottomBar

    [Header("Tab Manager")]
    public TabManager tabManager;

    [Header("Einstellungen")]
    public KeyCode toggleKey = KeyCode.Tab; // Taste zum Öffnen/Schließen
    public float slideSpeed = 8f;           // Geschwindigkeit der Animation

    private bool isOpen = false;
    private float closedY;                  // Position wenn geschlossen (unten versteckt)
    private float openY = 150f;             // Position wenn offen

    void Start()
    {
        closedY = -bottomBar.rect.height;
        bottomBar.anchoredPosition = new Vector2(0, closedY);
    }

    void Update()
    {
        // Bottom Bar togglen
        if (Keyboard.current[Key.Tab].wasPressedThisFrame)
        {
            isOpen = !isOpen;
        }

        // Tabs per Zahlentasten auswählen
        if (Keyboard.current[Key.Digit1].wasPressedThisFrame) tabManager.ShowPanel(0);
        if (Keyboard.current[Key.Digit2].wasPressedThisFrame) tabManager.ShowPanel(1);
        if (Keyboard.current[Key.Digit3].wasPressedThisFrame) tabManager.ShowPanel(2);
        if (Keyboard.current[Key.Digit4].wasPressedThisFrame) tabManager.ShowPanel(3);
        if (Keyboard.current[Key.Digit5].wasPressedThisFrame) tabManager.ShowPanel(4);

        // Sanft zur Zielposition gleiten
        float targetY = isOpen ? openY : closedY;
        Vector2 target = new Vector2(bottomBar.anchoredPosition.x, targetY);
        bottomBar.anchoredPosition = Vector2.Lerp(bottomBar.anchoredPosition, target, Time.deltaTime * slideSpeed);
    }
}