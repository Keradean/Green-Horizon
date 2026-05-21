using UnityEngine;
using Random = UnityEngine.Random;

namespace Manager
{
    public class Audiomanager : MonoBehaviour
    {
        public static Audiomanager Instance { get; private set; }

        [SerializeField] public AudioSource menuMusic;
        [SerializeField] public AudioSource[] bgm;
        [SerializeField] public AudioSource[] sfx;

        private int _currentBGM;
        private bool _playingBGM;

        #region Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            IsBGMPlaying();
        }

        #endregion

        #region Music

        public void PlayMainMenu()
        {
            StopMusic();
            menuMusic.Play();
        }

        public void PlayBGM()
        {
            StopMusic();
            _currentBGM = Random.Range(0, bgm.Length);
            bgm[_currentBGM].Play();
            _playingBGM = true;
        }

        public void StopMusic()
        {
            menuMusic.Stop();

            foreach (var track in bgm)
                track.Stop();

            _playingBGM = false;
        }

        private void IsBGMPlaying()
        {
            if (!_playingBGM) return;
            if (bgm[_currentBGM].isPlaying) return;

            // Wenn die Musik nicht mehr spielt, gehe zum nächsten Element
            _currentBGM++;

            // Ist der Array durchgelaufen, fange von vorne an
            if (_currentBGM >= bgm.Length)
                _currentBGM = 0;

            bgm[_currentBGM].Play();
        }

        #endregion

        #region SFX

        public void PlaySfx(int sfxToPlay)
        {
            sfx[sfxToPlay].PlayOneShot(sfx[sfxToPlay].clip);
        }

        #endregion
    }
}