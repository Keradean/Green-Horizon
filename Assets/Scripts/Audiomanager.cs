using UnityEngine;
using Random = UnityEngine.Random;

    public class Audiomanager : MonoBehaviour
    {   
        [SerializeField] public AudioSource menuMusic;
        [SerializeField] public AudioSource[] bgm;
        public AudioSource[] sfx;

        private int _currentBGM;
        private bool _playingBGM;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            IsBGMPlaying();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void PlaymainMenu()
        {
            StopMusic();
            menuMusic.Play();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void PlayBGM()
        {
            StopMusic();
            _currentBGM = Random.Range(0, bgm.Length);
            bgm[_currentBGM].Play();
            _playingBGM = true;
        }        
        ////////////////////////////////////////////////////////////////////////////////////////////////
        private void IsBGMPlaying()
        {
            if (!_playingBGM) return;
            // Wenn die Musik nicht mehr spielt, gehe zum nächsten element
            if (bgm[_currentBGM].isPlaying) return;
            _currentBGM++;
            // ist der Array durschgelaufen fange von vorne an
            if (_currentBGM >= bgm.Length)
            {
                _currentBGM = 0;
            }
            bgm[_currentBGM].Play();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        
        public void StopMusic()
        {
            menuMusic.Stop();
          
            foreach (var track in bgm)
            {
                track.Stop();
            }
            _playingBGM = false;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void PlaySfx(int sfxToPlay)
        {
            sfx[sfxToPlay].PlayOneShot(sfx[sfxToPlay].clip);
        }
} 
