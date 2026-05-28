using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Andy.Manager;

//*** De Col ***\\
namespace Dennis.Placement
{
    [RequireComponent(typeof(Button))]
    public class BuildingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        /////////////////////////////////////////////////////////////////////////////////////
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buildingData == null || isRoadButton || isDemolishButton) return;
            TooltipManager.Instance.Show(buildingData);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.Hide();
        }
    }
}