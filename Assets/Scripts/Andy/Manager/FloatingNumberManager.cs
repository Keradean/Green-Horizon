using System.Collections;
using TMPro;
using UnityEngine;
using Dennis.Manager;
//=== Andy ===//

namespace Andy.Manager
{
    public class FloatingNumberManager : MonoBehaviour
    {
        public static FloatingNumberManager Instance { get; private set; }

        [Header("Referenzen")]
        [SerializeField] private GameObject floatingNumberPrefab;
        [SerializeField] private RectTransform moneyHolder;    // Holder - Money
        [SerializeField] private RectTransform residentHolder; // Holder - Einwohner

        [Header("Animation")]
        [SerializeField] private float floatDistance = 50f;
        [SerializeField] private float duration = 1.5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            GreenCoinManager.OnGoldChanged += ShowGoldChange;
        }

        private void OnDisable()
        {
            GreenCoinManager.OnGoldChanged -= ShowGoldChange;
        }

        private void ShowGoldChange(int amount)
        {
            if (amount == 0) return;
            var text = amount > 0 ? $"+{amount:N0} $" : $"{amount:N0} $";
            var color = amount > 0 ? Color.green : Color.red;
            SpawnFloatingNumber(moneyHolder, text, color, amount > 0);
        }

        public void ShowResidentChange(int amount)
        {
            if (amount == 0) return;
            var text = amount > 0 ? $"+{amount:N0}" : $"{amount:N0}";
            var color = amount > 0 ? Color.green : Color.red;
            SpawnFloatingNumber(residentHolder, text, color, amount > 0);
        }

        private void SpawnFloatingNumber(RectTransform parent, string text, Color color, bool goUp)
        {
            var obj = Instantiate(floatingNumberPrefab, parent);
            var tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            StartCoroutine(AnimateFloatingNumber(obj.GetComponent<RectTransform>(), color, goUp));
        }

        private IEnumerator AnimateFloatingNumber(RectTransform rect, Color color, bool goUp)
        {
            var tmp = rect.GetComponent<TextMeshProUGUI>();
            var startPos = rect.anchoredPosition;
            var targetPos = startPos + new Vector2(0, goUp ? floatDistance : -floatDistance);
            var timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                var t = timer / duration;
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                var alpha = Mathf.Lerp(1f, 0f, t);
                tmp.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            Destroy(rect.gameObject);
        }
    }
}