using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//*** De Col ***//
namespace Dennis.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button nextButton;

        [Header("Steps")]
        [SerializeField] private TutorialStep[] steps;

        private int  _currentStep;
        private bool _isActive;

        /////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            StartTutorial();
        }

        private void OnDestroy()
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        public void StartTutorial()
        {
            _currentStep = 0;
            _isActive    = true;
            ShowStep(_currentStep);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void ShowStep(int index)
        {
            if (index >= steps.Length)
            {
                EndTutorial();
                return;
            }

            var step             = steps[index];
            tutorialPanel.SetActive(true);
            messageText.text     = step.message;

            // Show Next button only for Button trigger
            nextButton.gameObject.SetActive(step.trigger == TutorialTrigger.Button);

            // Auto trigger: advance after 2 seconds
            if (step.trigger == TutorialTrigger.Auto)
                StartCoroutine(AutoAdvance());
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void OnNextButtonClicked()
        {
            if (!_isActive) return;
            if (steps[_currentStep].trigger != TutorialTrigger.Button) return;
            AdvanceStep();
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public void NotifyEvent(TutorialTrigger trigger)
        {
            if (!_isActive) return;
            if (_currentStep >= steps.Length) return;
            if (steps[_currentStep].trigger != trigger) return;
            AdvanceStep();
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private void AdvanceStep()
        {
            _currentStep++;
            ShowStep(_currentStep);
        }

        private void EndTutorial()
        {
            _isActive = false;
            tutorialPanel.SetActive(false);
            Debug.Log("Tutorial completed!");
        }

        /////////////////////////////////////////////////////////////////////////////////////
        private IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(2f);
            AdvanceStep();
        }
    }
}