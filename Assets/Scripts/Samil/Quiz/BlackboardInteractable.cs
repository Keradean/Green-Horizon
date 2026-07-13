using UnityEngine;

namespace Samil.Quiz
{
    [RequireComponent(typeof(Collider))]
    public class BlackboardInteractable : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private GameObject interactPrompt;

        private bool _playerInRange;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _playerInRange = true;
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _playerInRange = false;
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        private void Update()
        {
            if (!_playerInRange) return;
            if (!Input.GetKeyDown(interactKey)) return;
            if (QuizManager.Instance == null) return;

            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            QuizManager.Instance.OpenIntro();
        }
    }
}
