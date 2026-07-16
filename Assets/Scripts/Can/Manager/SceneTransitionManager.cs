using Dennis.Manager;
using Dennis.Placement.Building;
using Samil.Manager;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//=== Can Özbal ===//

namespace Can.Manager
{
    public class SceneTransitionManager : MonoBehaviour
    {
        private const float FadeOutDuration = 0.4f; // current scene -> black
        private const float FadeInDuration = 0.4f; // black -> new scene

        // Frames the new scene renders behind the black overlay before it is
        // revealed, so Awake/Start hitches and first-render cost stay hidden.
        private const int SettleFrameCount = 5;

        private static SceneTransitionManager _instance;

        private GameObject _fadeCanvasRoot;
        private CanvasGroup _fadeCanvasGroup;
        private bool _isTransitioning;

        private static SceneTransitionManager Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var go = new GameObject(nameof(SceneTransitionManager));
                _instance = go.AddComponent<SceneTransitionManager>();
                DontDestroyOnLoad(go);
                _instance.BuildFadeCanvas();
                return _instance;
            }
        }

        private void BuildFadeCanvas()
        {
            _fadeCanvasRoot = new GameObject("FadeCanvas");
            _fadeCanvasRoot.transform.SetParent(transform);

            var canvas = _fadeCanvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            _fadeCanvasRoot.AddComponent<CanvasScaler>();
            _fadeCanvasRoot.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(_fadeCanvasRoot.transform, false);
            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;

            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _fadeCanvasGroup = _fadeCanvasRoot.AddComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 0f;

            // The whole canvas only exists while a transition is running, so it
            // can never bleed a stray black frame into normal gameplay.
            _fadeCanvasRoot.SetActive(false);
        }

        public static void LoadScene(string sceneName)
        {
            if (Instance._isTransitioning) return;
            if (CityTickManager.Instance)
            {
                CityTickManager.Instance.Buildings.Clear();
            }
            if (GreenCoinManager.Instance)
            {
                GreenCoinManager.Instance.SetGold(3000);
            }
            Instance.StartCoroutine(Instance.TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _isTransitioning = true;

            // 1. End the current scene on black.
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasRoot.SetActive(true);
            yield return Fade(0f, 1f, FadeOutDuration);

            // 2. Load and activate the new scene while the screen is covered.
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 3. Let the new scene render a few frames behind the overlay so its
            // first visible frames (camera, lighting, post-processing) are stable.
            for (var i = 0; i < SettleFrameCount; i++)
            {
                yield return null;
            }

            // 4. Reveal the new scene, then remove the overlay completely.
            yield return Fade(1f, 0f, FadeInDuration);
            _fadeCanvasRoot.SetActive(false);

            _isTransitioning = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            _fadeCanvasGroup.alpha = from;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Cap the per-frame step so a single hitchy frame can't make the
                // fade jump instantly from one end to the other.
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            _fadeCanvasGroup.alpha = to;
        }
    }
}
