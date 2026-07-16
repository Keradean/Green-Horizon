using Dennis.DayAndNight;
using Dennis.Manager;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
//=== De Col ===//
namespace Can.Manager
{
    public class Audiomanager : Singleton<Audiomanager>
    {
        [Header("Menu")]
        public AudioSource menuMusic;

        [Header("BGM")]
        public AudioSource[] bgm;

        [Header("SFX")]
        public AudioSource[] sfx;

        [Header("Ambience")]
        public AudioSource dayAmbience;
        public AudioSource nightAmbience;

        private int _currentBGM;
        private bool _playingBGM;
        private float _dayAmbienceVolume;
        private float _nightAmbienceVolume;
        private LightManager _lightManager;

        #region Lifecycle
        private void Start()
        {
            if (dayAmbience != null)
            {
                _dayAmbienceVolume = dayAmbience.volume;
                // Clip vorab laden
                dayAmbience.PlayOneShot(dayAmbience.clip, 0f);
            }
            if (nightAmbience != null)
            {
                _nightAmbienceVolume = nightAmbience.volume;
                // Clip vorab laden
                nightAmbience.PlayOneShot(nightAmbience.clip, 0f);
            }
            _lightManager = FindAnyObjectByType<LightManager>();
        }

        private void OnEnable()
        {
            LightManager.OnDayNightChanged += OnDayNightChanged;
        }

        private void OnDisable()
        {
            LightManager.OnDayNightChanged -= OnDayNightChanged;
        }

        private void Update()
        {
            IsBGMPlaying();
        }
        #endregion

        #region Ambience
        public void StartGameAmbience()
        {
            if (dayAmbience != null)
                dayAmbience.Play();
        }

        private float GetCrossfadeDuration()
        {
            if (_lightManager == null)
                _lightManager = FindAnyObjectByType<LightManager>();
            if (_lightManager == null) return 3f;

            var daySeconds = _lightManager.DayDuration * 60f;
            var nightSeconds = _lightManager.NightDuration * 60f;
            return Mathf.Min(daySeconds, nightSeconds) * 0.1f;
        }

        private void OnDayNightChanged(bool isDay)
        {
            StartCoroutine(CrossfadeAmbience(isDay));
        }

        private IEnumerator CrossfadeAmbience(bool isDay)
        {
            var fadeIn = isDay ? dayAmbience : nightAmbience;
            var fadeOut = isDay ? nightAmbience : dayAmbience;
            var targetVolume = isDay ? _dayAmbienceVolume : _nightAmbienceVolume;
            var crossfadeDuration = GetCrossfadeDuration();

            if (!fadeIn.isPlaying)
            {
                fadeIn.volume = 0f;
                fadeIn.Play();
            }

            var timer = 0f;
            var startVolumeOut = fadeOut.volume;

            while (timer < crossfadeDuration)
            {
                timer += Time.deltaTime;
                var t = timer / crossfadeDuration;
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                yield return null;
            }

            fadeIn.volume = targetVolume;
            fadeOut.volume = 0f;
            fadeOut.Stop();
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

        private void StopMusic()
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
            _currentBGM++;
            if (_currentBGM >= bgm.Length)
                _currentBGM = 0;
            bgm[_currentBGM].Play();
        }
        #endregion

        #region SFX
        public void PlaySfx(int sfxToPlay)
        {
            if (sfx == null || sfxToPlay < 0 || sfxToPlay >= sfx.Length) return;
            sfx[sfxToPlay].Play();
        }
        #endregion
    }
}