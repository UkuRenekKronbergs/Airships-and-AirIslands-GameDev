using UnityEngine;
using UnityEngine.UI;
using AirshipsAndAirIslands.Audio;

namespace AirshipsAndAirIslands.UI
{
    /// <summary>
    /// Handles volume control UI for music and sound effects.
    /// Attach this script to the SettingsPanel in the MainMenu scene.
    /// </summary>
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Text musicVolumeLabel;
        [SerializeField] private Text sfxVolumeLabel;

        private void OnEnable()
        {
            // Initialize sliders with current values when panel is shown
            if (AudioManager.Instance != null)
            {
                if (musicVolumeSlider != null)
                {
                    musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                }
                if (sfxVolumeSlider != null)
                {
                    sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                }
            }
        }

        private void Start()
        {
            // Subscribe to slider changes
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                // Update label immediately
                OnMusicVolumeChanged(musicVolumeSlider.value);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                // Update label immediately
                OnSFXVolumeChanged(sfxVolumeSlider.value);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from slider changes
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
                
                if (musicVolumeLabel != null)
                {
                    musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100)}%";
                }
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
                
                if (sfxVolumeLabel != null)
                {
                    sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%";
                }
            }
        }
    }
}
