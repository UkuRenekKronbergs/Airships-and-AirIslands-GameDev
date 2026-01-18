using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using AirshipsAndAirIslands.Audio;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;

    [Header("Volume Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeLabel;
    [SerializeField] private TextMeshProUGUI sfxVolumeLabel;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Persist across all scenes - must be called on root GameObject
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("PauseMenuManager initialized and marked as DontDestroyOnLoad");
    }

    private void Start()
    {
        // Hide panels initially
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Wire buttons
        WireButton(resumeButton, OnResumeClicked);
        WireButton(settingsButton, OnSettingsClicked);
        WireButton(quitButton, OnQuitClicked);
        WireButton(settingsBackButton, OnSettingsBackClicked);

        // Initialize volume controls
        InitializeVolumeControls();
    }

    private void Update()
    {
        // Check for ESC key press, but NOT in MainMenu
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != mainMenuSceneName)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
            Debug.Log("Pause toggled. isPaused: " + isPaused + ", pausePanel active: " + pausePanel.activeSelf);
        }

        // When pausing, make sure to show PausePanel and hide SettingsPanel
        if (isPaused)
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            
            GameObject pausePanelChild = pausePanel?.transform.Find("PausePanel")?.gameObject;
            if (pausePanelChild != null)
            {
                pausePanelChild.SetActive(true);
            }
        }

        // Pause/Resume game time
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void OnResumeClicked()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            RefreshVolumeSlidersFromAudioManager();
        }
        
        // Hide PausePanel so it doesn't show behind SettingsPanel
        GameObject pausePanelChild = pausePanel?.transform.Find("PausePanel")?.gameObject;
        if (pausePanelChild != null)
        {
            pausePanelChild.SetActive(false);
        }
    }

    private void OnSettingsBackClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Only if we're actually paused, show the pause panel
        if (isPaused)
        {
            GameObject pausePanelChild = pausePanel?.transform.Find("PausePanel")?.gameObject;
            if (pausePanelChild != null)
            {
                pausePanelChild.SetActive(true);
            }
        }
    }

    private void OnQuitClicked()
    {
        // Resume time and hide panels before loading new scene
        Time.timeScale = 1f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void RefreshVolumeSlidersFromAudioManager()
    {
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                OnMusicVolumeChanged(musicVolumeSlider.value);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                OnSFXVolumeChanged(sfxVolumeSlider.value);
            }
        }
    }

    private void InitializeVolumeControls()
    {
        if (AudioManager.Instance != null)
        {
            // Set initial slider values
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                OnMusicVolumeChanged(musicVolumeSlider.value);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                OnSFXVolumeChanged(sfxVolumeSlider.value);
            }
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

    private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(action);
        // Ensure pause menu buttons play the default click SFX
        if (button.GetComponent<AirshipsAndAirIslands.Audio.UIButtonSound>() == null && button.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
        {
            button.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonSound>();
        }
    }
}
