using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public void Start() {
        AudioClip mainTheme = Resources.Load<AudioClip>(StageManager.Instance.gameVersion + "/audio/" + "mainTheme");
        if (mainTheme is null) mainTheme = Resources.Load<AudioClip>("defaultAssets/audio/"+ "mainTheme");
        if (mainTheme is not null) audioSource.clip = mainTheme;

        foreach (GameClip gameClip in Enum.GetValues(typeof(GameClip))) {
            AudioClip clip = Resources.Load<AudioClip>(StageManager.Instance.gameVersion + "/audio/" + gameClip.ToString());
            if (clip is null) clip = Resources.Load<AudioClip>("defaultAssets/audio/" + gameClip.ToString());
            if (clip is not null) clips[gameClip] = clip;
        }
        audioSource.Play();
    }

    public void PlayClip(GameClip clipToPlay) {
        if (!clips.ContainsKey(clipToPlay)) return;
        if (clipToPlay == GameClip.win || clipToPlay == GameClip.highScore) {
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
    enterShop
}
