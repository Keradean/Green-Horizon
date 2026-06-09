using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//=== Andy ===//

namespace Andy.Manager
{
    public class AnnouncementManager : MonoBehaviour
    {
        public static AnnouncementManager Instance { get; private set; }

        [Header("Referenzen")]
        public CanvasGroup canvasGroup;         // Panel - Announcement
        public TextMeshProUGUI messageText;     // Text - Message
        public Image iconImage;                 // Image - Icon
        public GameObject iconHolder;           // Image - Icon GameObject

        [Header("Einstellungen")]
        public float fadeSpeed = 2f;

        private Coroutine _currentRoutine;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            canvasGroup.alpha = 0f;
        }

        public void Show(AnnouncementData data)
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(ShowRoutine(data));
        }

        // Direkt mit Text aufrufen ohne SO
        public void Show(string message, float duration = 5f, Sprite icon = null)
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);

            var data = ScriptableObject.CreateInstance<AnnouncementData>();
            data.message = message;
            data.duration = duration;
            data.icon = icon;

            _currentRoutine = StartCoroutine(ShowRoutine(data));
        }

        private IEnumerator ShowRoutine(AnnouncementData data)
        {
            // Inhalt setzen
            messageText.text = data.message;

            if (data.icon != null)
            {
                iconHolder.SetActive(true);
                iconImage.sprite = data.icon;
            }
            else
            {
                iconHolder.SetActive(false);
            }

            // Einblenden
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Warten
            yield return new WaitForSeconds(data.duration);

            // Ausblenden
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
}