using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapHUD : MonoBehaviour
{
    [Header("Navigation Buttons")]
    [SerializeField] private Button shipRoomsButton;
    [SerializeField] private Button cityButton;
    [SerializeField] private Button battleButton;

    [Header("Scene Names")]
    [SerializeField] private string shipRoomsSceneName = "ShipRooms";
    [SerializeField] private string citySceneName = "City";
    [SerializeField] private string battleSceneName = "Battle";

    private void Awake()
    {
        WireButton(shipRoomsButton, LoadShipRoomsScene);
        WireButton(cityButton, LoadCityScene);
    WireButton(battleButton, LoadBattleScene);
    }

    private void WireButton(Button button, UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }

    private void LoadShipRoomsScene()
    {
        TryLoadScene(shipRoomsSceneName);
    }

    private void LoadCityScene()
    {
        TryLoadScene(citySceneName);
    }

    private void LoadBattleScene()
    {
        TryLoadScene(battleSceneName);
    }

    private void TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("MapHUD scene name is empty.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"MapHUD could not load scene '{sceneName}'. Ensure it is added to Build Settings.");
        }
    }
}
