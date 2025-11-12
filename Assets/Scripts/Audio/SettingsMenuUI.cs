using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuUI : MonoBehaviour
{
    // --- Music ---
    public Slider musicSlider;
    public TextMeshProUGUI musicValueText;
    private const string MUSIC_VOL_KEY = "MusicVolume";

    // --- SFX ---
    public Slider sfxSlider;
    public TextMeshProUGUI sfxValueText;
    private const string SFX_VOL_KEY = "SFXVolume";

    void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1.0f);
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }
        UpdateMusicText(musicVolume);

        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1.0f);
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
        UpdateSFXText(sfxVolume);
    }


    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        UpdateMusicText(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        UpdateSFXText(value);
    }


    private void UpdateMusicText(float value)
    {
        if (musicValueText != null)
        {
            musicValueText.text = "Music: " + Mathf.RoundToInt(value * 100) + "%";
        }
    }

    private void UpdateSFXText(float value)
    {
        if (sfxValueText != null)
        {
            sfxValueText.text = "SFX: " + Mathf.RoundToInt(value * 100) + "%";
        }
    }
}