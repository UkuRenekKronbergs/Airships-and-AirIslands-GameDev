using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button creditsButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button creditsBackButton;

    [SerializeField]
    private Button settingsBackButton;

    [Header("Panels")]
    [SerializeField]
    private GameObject settingsPanel;

    [SerializeField]
    private GameObject creditsPanel;

    [SerializeField]
    private string playSceneName = "Map";

    private void Awake()
    {
        WireButton(playButton, OnPlayClicked);
        WireButton(settingsButton, OnSettingsClicked);
        WireButton(creditsButton, OnCreditsClicked);
        WireButton(exitButton, OnExitClicked);
    WireButton(creditsBackButton, OnCreditsBackClicked);
    WireButton(settingsBackButton, OnSettingsBackClicked);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        LoadScene(playSceneName);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("MainMenuController: Tried to load a scene, but the scene name was empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowPanel(GameObject panel)
    {
        TogglePanel(panel, true);
    }

    public void HidePanel(GameObject panel)
    {
        TogglePanel(panel, false);
    }

    private static void TogglePanel(GameObject panel, bool enabled)
    {
        if (panel == null)
        {
            Debug.LogWarning("MainMenuController: Tried to toggle a panel, but no panel was provided.");
            return;
        }

        panel.SetActive(enabled);
    }

    private void OnPlayClicked()
    {
        PlayGame();
    }

    private void OnSettingsClicked()
    {
        TogglePanel(settingsPanel);
    }

    private void OnCreditsClicked()
    {
        TogglePanel(creditsPanel);
    }

    private void OnExitClicked()
    {
        QuitGame();
    }

    private void OnCreditsBackClicked()
    {
        HidePanel(creditsPanel);
    }

    private void OnSettingsBackClicked()
    {
        HidePanel(settingsPanel);
    }

    private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    private static void TogglePanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("MainMenuController: Tried to toggle a panel, but no panel was provided.");
            return;
        }

        panel.SetActive(!panel.activeSelf);
    }
}
