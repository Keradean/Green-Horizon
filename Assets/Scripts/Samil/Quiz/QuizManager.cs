using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Samil.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("Fragen-Datei")]
        [SerializeField] private TextAsset questionsFile;

        [Header("Intro / Tafel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TMP_Text infoText;
        [TextArea(4, 12)]
        [SerializeField] private string infoMessage =
            "Willkommen zum Green Horizon Quiz!\n\n" +
            "Merke dir:\n" +
            "- Nachhaltiges Handeln spart Ressourcen und schützt Ökosysteme.\n" +
            "- Erneuerbare Energien stehen langfristig zur Verfügung.\n" +
            "- Recycling und Reparatur reduzieren Müll und Rohstoffverbrauch.\n\n" +
            "Beantworte anschließend alle Fragen richtig, um zu bestehen.";
        [SerializeField] private Button infoContinueButton;

        [Header("Quiz UI")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Ergebnis")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;

        [Header("Einstellungen")]
        [SerializeField] private bool shuffleQuestions = true;
        [SerializeField] private string realSceneName = "RealScene";
        [SerializeField] private float feedbackDuration = 1.2f;

        private List<QuizQuestion> _allQuestions;
        private List<QuizQuestion> _round;
        private int _currentIndex;
        private int _correctCount;
        private bool _inputLocked;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            infoContinueButton.onClick.AddListener(OnInfoContinueClicked);
            retryButton.onClick.AddListener(StartQuiz);
            continueButton.onClick.AddListener(OnContinueToRealScene);

            infoPanel.SetActive(false);
            quizPanel.SetActive(false);
            resultPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            infoContinueButton.onClick.RemoveListener(OnInfoContinueClicked);
            retryButton.onClick.RemoveListener(StartQuiz);
            continueButton.onClick.RemoveListener(OnContinueToRealScene);
        }

        public void OpenIntro()
        {
            if (_allQuestions == null)
                _allQuestions = QuizTextParser.Parse(questionsFile.text);

            resultPanel.SetActive(false);
            quizPanel.SetActive(false);

            infoText.text = infoMessage;
            infoPanel.SetActive(true);
        }

        private void OnInfoContinueClicked()
        {
            infoPanel.SetActive(false);
            StartQuiz();
        }

        private void StartQuiz()
        {
            resultPanel.SetActive(false);

            _round = new List<QuizQuestion>(_allQuestions);
            if (shuffleQuestions)
                Shuffle(_round);

            _currentIndex = 0;
            _correctCount = 0;

            quizPanel.SetActive(true);
            ShowQuestion(_currentIndex);
        }

        private void ShowQuestion(int index)
        {
            _inputLocked = false;
            feedbackText.text = "";

            var question = _round[index];
            questionText.text = question.Text;
            progressText.text = $"Frage {index + 1} / {_round.Count}";

            for (var i = 0; i < answerButtons.Length; i++)
            {
                var button = answerButtons[i];
                var hasAnswer = i < question.Answers.Count;
                button.gameObject.SetActive(hasAnswer);
                if (!hasAnswer) continue;

                button.interactable = true;
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = question.Answers[i];

                var answerIndex = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnAnswerSelected(answerIndex));
            }
        }

        private void OnAnswerSelected(int answerIndex)
        {
            if (_inputLocked) return;
            _inputLocked = true;

            foreach (var button in answerButtons)
                button.interactable = false;

            var question = _round[_currentIndex];
            var isCorrect = answerIndex == question.CorrectAnswerIndex;
            if (isCorrect)
                _correctCount++;

            feedbackText.text = isCorrect
                ? "Richtig!"
                : $"Leider falsch. Richtig war: {question.Answers[question.CorrectAnswerIndex]}";

            StartCoroutine(NextAfterDelay());
        }

        private IEnumerator NextAfterDelay()
        {
            yield return new WaitForSeconds(feedbackDuration);

            _currentIndex++;
            if (_currentIndex < _round.Count)
                ShowQuestion(_currentIndex);
            else
                ShowResult();
        }

        private void ShowResult()
        {
            quizPanel.SetActive(false);
            resultPanel.SetActive(true);

            var passed = _correctCount == _round.Count;
            resultText.text = passed
                ? $"Bestanden! {_correctCount} / {_round.Count} richtig."
                : $"Nicht bestanden: {_correctCount} / {_round.Count} richtig. Versuch es erneut!";

            retryButton.gameObject.SetActive(!passed);
            continueButton.gameObject.SetActive(passed);
        }

        private void OnContinueToRealScene()
        {
            SceneManager.LoadScene(realSceneName);
        }

        private static void Shuffle(List<QuizQuestion> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
