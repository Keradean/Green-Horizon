using UnityEngine;
using UnityEngine.UI;

//*** De Col ***\\
namespace Dennis.Placement
{
    [RequireComponent(typeof(Button))]
    public class BuildingButton : MonoBehaviour
    {
        [Header("Gebäude")]
        [Tooltip("Das ScriptableObject des Gebäudes das platziert werden soll")]
        [SerializeField] private BuildingData buildingData;

        [Header("Sondermodi")]
        [SerializeField] private bool isRoadButton    = false;
        [SerializeField] private bool isDemolishButton = false;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClick);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        private void OnClick()
        {
            if (isRoadButton)
            {
                BuildingSystem.Instance.StartRoadMode();
                return;
            }

            if (isDemolishButton)
            {
                BuildingSystem.Instance.StartDemolishMode();
                return;
            }
        
            if (buildingData != null)
                BuildingSystem.Instance.StartPlacing(buildingData);
        }
    }
}