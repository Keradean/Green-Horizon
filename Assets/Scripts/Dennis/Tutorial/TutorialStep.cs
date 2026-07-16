using UnityEngine;
//*** De Col ***//
namespace Dennis.Tutorial
{
    public enum TutorialTrigger
    {
        Button,          // Player clicks Next
        BuildingPlaced,  // Player placed a building
        RoadPlaced,      // Player placed a road
        Auto,            // Advances automatically after 2 seconds
        Tab,             // Player clicked on a tab
        numbers,          // Player pressed a number key
        CO2Increased,    // CO2 went up
        PauseOpened,     // Player opened pause menu
        CoinsSpent       // Player spent coins
    }

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(2, 6)]
        public string message;

        public TutorialTrigger trigger;
    }
}