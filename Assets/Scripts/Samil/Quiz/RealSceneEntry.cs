using TMPro;
using UnityEngine;

namespace Samil.Quiz
{
    public class RealSceneEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text welcomeText;

        private void Start()
        {
            Debug.Log("Quiz bestanden - echte Szene geladen.");

            if (welcomeText != null)
                welcomeText.text = "Willkommen! Das Quiz wurde bestanden.";
        }
    }
}
