using UnityEngine;
using UnityEngine.UI;

//=== Andy ===//

namespace Andy.Manager
{
    public class TabManager : MonoBehaviour
    {
        [Header("Tab Panels")]
        public GameObject[] buildingPanels;             // alle 4 Contents
    
        [Header("Tab Buttons")]
        public Button[] tabButtons;                     // alle 4 Tab-Buttons
        public Color activeColor = Color.white;     // Farbe wenn aktiv
        public Color inactiveColor = Color.gray;    // Farbe wenn inaktiv
    
        [Header("Scroll View")]
        public ScrollRect scrollRect;                   // Scroll View - Buildings

        private void Start()
        {
            ShowPanel(0);
        }

        public void ShowPanel(int index)
        {
            if (index < 0 || index >= buildingPanels.Length) return;
            for (var i = 0; i < buildingPanels.Length; i++)
            {
                buildingPanels[i].SetActive(i == index);
            }
        
            // Tab-Button Farben updaten
            for (var i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].image.color = i == index ? activeColor : inactiveColor;
            }
        
            scrollRect.content = buildingPanels[index].GetComponent<RectTransform>();   // Aktives Panel umschalten
            scrollRect.horizontalNormalizedPosition = 0;                                // Reset Scroll auf Anfang
            Dennis.Tutorial.TutorialManager.Instance?.NotifyEvent(Dennis.Tutorial.TutorialTrigger.Numbers);
            
        }
    }
}