using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Dennis.Placement;
//=== Andy ===//

namespace Andy.Manager
{
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance { get; private set; }

        [Header("Panel")]
        public RectTransform tooltipPanel;

        [Header("Texte")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI residentsText;
        public TextMeshProUGUI incomeText;
        public TextMeshProUGUI energyText;
        public TextMeshProUGUI pollutionText;

        [Header("Icon")]
        public Image buildingIcon;

        [Header("Animation")]
        public float slideSpeed = 8f;

        private float _openY = 430f;        // Position wenn sichtbar - im Inspector anpassen
        private float _closedY = -400f;     // Position wenn versteckt - im Inspector anpassen
        private bool _isOpen = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            tooltipPanel.anchoredPosition = new Vector2(tooltipPanel.anchoredPosition.x, _closedY);
        }

        private void Update()
        {
            // Animation
            float targetY = _isOpen ? _openY : _closedY;
            tooltipPanel.anchoredPosition = Vector2.Lerp(
                tooltipPanel.anchoredPosition,
                new Vector2(tooltipPanel.anchoredPosition.x, targetY),
                Time.deltaTime * slideSpeed
            );
        }

        public void Show(BuildingData data)
        {
            _isOpen = true;

            nameText.text        = data.BuildingName;
            descriptionText.text = data.Description;
            residentsText.text   = "+" + data.Residents;
            incomeText.text      = "+" + data.IncomePerHour + "/h";
            energyText.text      = "-" + data.EnergyUsage + "%";
            pollutionText.text   = "+" + data.Pollution + "%/Tag";

            if (data.Icon != null)
                buildingIcon.sprite = data.Icon;
        }

        public void Hide()
        {
            _isOpen = false;
        }
    }
}