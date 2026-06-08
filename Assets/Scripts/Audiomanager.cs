using UnityEngine;
using Dennis.Manager;
using Random = UnityEngine.Random;
//=== Can Özbal ===//

public class Audiomanager : Singleton<Audiomanager>
{
    public AudioSource menuMusic;
    public AudioSource[] bgm;
    public AudioSource[] sfx;

    private int _currentBGM;
    private bool _playingBGM;

    #region Lifecycle
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
        if (sfx == null || sfxToPlay < 0 || sfxToPlay >= sfx.Length) return; 
        sfx[sfxToPlay].Play();
    }

    #endregion
}