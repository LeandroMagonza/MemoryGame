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
        if (_instance == null) {
            _instance = this as AudioManager;
            audioSource = GetComponent<AudioSource>();
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
    public AudioSource audioSource;
    public Dictionary<GameClip, AudioClip> clips = new();
    public bool playMusic = true;
    public GameObject muteMusicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    public void Start() {
        AudioClip mainTheme = Resources.Load<AudioClip>(StageManager.Instance.gameVersion + "/audio/" + "mainTheme");
        if (mainTheme is null) mainTheme = Resources.Load<AudioClip>("defaultAssets/audio/"+ "mainTheme");
        if (mainTheme is not null) audioSource.clip = mainTheme;

        foreach (GameClip gameClip in Enum.GetValues(typeof(GameClip))) {
            AudioClip clip = Resources.Load<AudioClip>(StageManager.Instance.gameVersion + "/audio/" + gameClip.ToString());
            if (clip is null) clip = Resources.Load<AudioClip>("defaultAssets/audio/" + gameClip.ToString());
            if (clip is not null) clips[gameClip] = clip;
        }

        if (playMusic) audioSource.Play();
    }

    public void PlayClip(GameClip clipToPlay) {
        if (!clips.ContainsKey(clipToPlay)) return;
        if ((clipToPlay == GameClip.win || clipToPlay == GameClip.highScore)
            && playMusic) {
            audioSource.Stop();
            StartCoroutine(RestartMusicInDelay(clips[clipToPlay].length));
        }
        audioSource.PlayOneShot(clips[clipToPlay]);
    }

    public IEnumerator RestartMusicInDelay(float delay) {
        while (delay > 0) {
            delay -= Time.deltaTime;
            yield return null;
        }
        audioSource.Play();
    }

    public void ToggleMusic()
    {
        playMusic = !playMusic;

        if (playMusic)
        {
            audioSource.Play();
            muteMusicButtonImage.GetComponent<Image>().sprite = musicOnSprite;
        }
        else
        {
            audioSource.Stop ();
            muteMusicButtonImage.GetComponent<Image>().sprite = musicOffSprite;
        }
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
    getAchievementStar2
}
