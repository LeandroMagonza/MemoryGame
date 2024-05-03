using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfigCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public Slider musicSlider;
    [SerializeField] public Slider soundFXSlider;

    void Start()
    {
        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        soundFXSlider.onValueChanged.AddListener(UpdateSoundFXVolume);

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolumePercentage");
        soundFXSlider.value = PlayerPrefs.GetFloat("SoundFXVolumePercentage");
    }

    private void UpdateMusicVolume(float volumePercentage) {
        AudioManager.Instance.AdjustMusicVolume(volumePercentage);
    }
    private void UpdateSoundFXVolume(float volumePercentage) {
        AudioManager.Instance.AdjustSoundFXVolume(volumePercentage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
