using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfigCanvas : MonoBehaviour
{
    [SerializeField] public Slider musicSlider;
    [SerializeField] public Slider soundFXSlider;
    [SerializeField] public Toggle vibrateToggle; // Nuevo Toggle para vibración

    void Start()
    {
        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        soundFXSlider.onValueChanged.AddListener(UpdateSoundFXVolume);
        vibrateToggle.onValueChanged.AddListener(UpdateVibrationSetting); // Escuchar cambios en el Toggle

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolumePercentage", 0.75f);
        soundFXSlider.value = PlayerPrefs.GetFloat("SoundFXVolumePercentage", 0.75f);
        vibrateToggle.isOn = PlayerPrefs.GetInt("HapticFeedbackEnabled", 1) == 1; // Inicializar Toggle
    }

    private void UpdateMusicVolume(float volumePercentage)
    {
        AudioManager.Instance.AdjustMusicVolume(volumePercentage);
    }

    private void UpdateSoundFXVolume(float volumePercentage)
    {
        AudioManager.Instance.AdjustSoundFXVolume(volumePercentage);
    }

    private void UpdateVibrationSetting(bool isEnabled) // Nuevo método para actualizar la vibración en PlayerPrefs
    {
        PlayerPrefs.SetInt("HapticFeedbackEnabled", isEnabled ? 1 : 0);
    }
}