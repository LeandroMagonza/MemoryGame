using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    #region Singleton

    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<AudioManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(AudioManager) + "> instance has been not found.");
            return _instance;
        }
    }

    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as AudioManager;
            audioSourceMusic = GetComponents<AudioSource>()[0];
            audioSourceFX = GetComponents<AudioSource>()[1];
        }
        else if (_instance != this)
            DestroySelf();
    }

    private void DestroySelf()
    {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }

    #endregion

    public AudioSource audioSourceMusic;
    public AudioSource audioSourceFX;
    public Dictionary<GameClip, AudioClip> clips = new();
    public bool playMusic = true;
    public GameObject muteMusicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private float maxMusicVolume;
    private float maxSoundFXVolume;

    public ConfigCanvas configCanvas;

    public void Start()
    {
        maxMusicVolume = audioSourceMusic.volume;
        maxSoundFXVolume = audioSourceFX.volume;

        audioSourceMusic.volume = PlayerPrefs.GetFloat("MusicVolumePercentage", 0.75f) * maxMusicVolume;
        audioSourceFX.volume = PlayerPrefs.GetFloat("SoundFXVolumePercentage", 0.75f) * maxSoundFXVolume;

        AudioClip mainTheme = Resources.Load<AudioClip>(StageManager.Instance.gameVersion + "/audio/" + "mainTheme");
        if (mainTheme is null) mainTheme = Resources.Load<AudioClip>("defaultAssets/audio/" + "mainTheme");
        if (mainTheme is not null) audioSourceMusic.clip = mainTheme;

        foreach (GameClip gameClip in Enum.GetValues(typeof(GameClip)))
        {
            AudioClip clip = Resources.Load<AudioClip>(StageManager.Instance.gameVersion + "/audio/" + gameClip.ToString());
            if (clip is null) clip = Resources.Load<AudioClip>("defaultAssets/audio/" + gameClip.ToString());
            if (clip is not null) clips[gameClip] = clip;
        }
        if (playMusic && !audioSourceMusic.isPlaying) audioSourceMusic.Play();
    }

    public float PlayClip(GameClip clipToPlay)
    {
        if (!clips.ContainsKey(clipToPlay)) return 0f;
        if ((clipToPlay == GameClip.win || clipToPlay == GameClip.highScore) && playMusic)
        {
            audioSourceMusic.Stop();
            StartCoroutine(RestartMusicInDelay(clips[clipToPlay].length));
        }

        audioSourceFX.PlayOneShot(clips[clipToPlay]);
        return clips[clipToPlay].length;
    }

    public float PlayClip(GameClip clipToPlay, float pitchShift)
    {
        audioSourceFX.pitch = pitchShift;
        return PlayClip(clipToPlay);
    }

    public IEnumerator RestartMusicInDelay(float delay)
    {
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        if (playMusic && !audioSourceMusic.isPlaying) audioSourceMusic.Play();
    }

    public void ToggleMusic()
    {
        playMusic = !playMusic;

        if (playMusic)
        {
            audioSourceMusic.Play();
            muteMusicButtonImage.GetComponent<Image>().sprite = musicOnSprite;
        }
        else
        {
            audioSourceMusic.Stop();
            muteMusicButtonImage.GetComponent<Image>().sprite = musicOffSprite;
        }
    }

    public void AdjustMusicVolume(float volumePercentage)
    {
        audioSourceMusic.volume = maxMusicVolume * volumePercentage;
        PlayerPrefs.SetFloat("MusicVolumePercentage", volumePercentage);
    }

    public void AdjustSoundFXVolume(float volumePercentage)
    {
        audioSourceFX.volume = maxSoundFXVolume * volumePercentage;
        PlayerPrefs.SetFloat("SoundFXVolumePercentage", volumePercentage);
    }
}

public enum GameClip {
    none = 0,
    correctGuess,
    incorrectGuess,
    endGame,
    bonus,
    win,
    highScore,
    playStage,
    enterStages,
    enterShop,
    getAchievementStar0,
    getAchievementStar1,
    getAchievementStar2,
    bombExplosion,
    beep1,
    beep2,
    beep3,
}
