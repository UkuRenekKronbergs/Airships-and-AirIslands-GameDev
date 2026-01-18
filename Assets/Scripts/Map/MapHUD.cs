using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AirshipsAndAirIslands.Events;

public class MapHUD : MonoBehaviour
{
    [Header("Navigation Buttons")]
    [SerializeField] private Button shipRoomsButton;
    [SerializeField] private Button[] cityButtons;
    [SerializeField] private Button battleButton;

    [Header("Scene Names")]
    [SerializeField] private string shipRoomsSceneName = "ShipRooms";
    [SerializeField] private string citySceneName = "City";
    [SerializeField] private string battleSceneName = "Battle";
    [Header("Events")]
    [SerializeField] private GameEventManager gameEventManager;
    [SerializeField] private EventUI eventUI;

    private void Awake()
    {
        // Auto-find and wire event manager + UI if they weren't assigned in the inspector
        if (gameEventManager == null)
        {
            gameEventManager = GameState.Instance?.GetComponent<GameEventManager>() ?? UnityEngine.Object.FindFirstObjectByType<GameEventManager>();
        }

        if (eventUI == null)
        {
            eventUI = UnityEngine.Object.FindFirstObjectByType<EventUI>();
        }

        if (eventUI != null && gameEventManager != null)
        {
            eventUI.SetGameEventManager(gameEventManager);
            SetEventReferences(gameEventManager, eventUI);
        }

        WireButton(shipRoomsButton, LoadShipRoomsScene);
        foreach (Button cityButton in cityButtons)
        {
            WireButton(cityButton, LoadCityScene);
        }
        WireButton(battleButton, LoadBattleScene);
        // Map should only have the Ship button play the click SFX. Ensure shipRoomsButton has it and remove from others.
        if (shipRoomsButton != null && shipRoomsButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
        {
            if (shipRoomsButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonSound>() == null)
            {
                shipRoomsButton.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonSound>();
            }
        }

        if (cityButtons != null)
        {
            foreach (Button cityButton in cityButtons)
            {
                var comp = cityButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonSound>();
                if (comp != null) Destroy(comp);
            }
        }

        if (battleButton != null)
        {
            var comp = battleButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonSound>();
            if (comp != null) Destroy(comp);
        }
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
        if (!GameState.Instance.IsHoveredMovementPossible() && !GameState.Instance.IsPlayerOnHoveredLocation()) return;

        StartCoroutine(HandleEventThenLoad(citySceneName));
    }

    private void LoadBattleScene()
    {
        if (!GameState.Instance.IsHoveredMovementPossible()) return;

        StartCoroutine(HandleEventThenLoad(battleSceneName));
    }

    private System.Collections.IEnumerator HandleEventThenLoad(string sceneName)
    {
        if (gameEventManager == null || eventUI == null)
        {
            TryLoadScene(sceneName);
            yield break;
        }

        // Get a random event and show it. If player skips, just load the scene.
        GameEvent gameEvent = null;
        try
        {
            gameEvent = gameEventManager.GetRandomEvent();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to get random event: {ex.Message}");
        }

        if (gameEvent == null)
        {
            TryLoadScene(sceneName);
            yield break;
        }

        bool finished = false;
        EventResult resolvedResult = null;
        bool usedEvent = false;

        yield return StartCoroutine(eventUI.ShowEventCoroutine(gameEvent, (result, applied) =>
        {
            resolvedResult = result;
            usedEvent = applied;
            finished = true;
        }));

        // If the event triggered combat, prefer loading the Battle scene.
        if (resolvedResult != null && resolvedResult.TriggersCombat)
        {
            TryLoadScene(battleSceneName);
            yield break;
        }

        // Otherwise proceed to the requested scene.
        TryLoadScene(sceneName);
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

    /// <summary>
    /// Allows runtime wiring of the event manager and UI when scenes are set up programmatically.
    /// </summary>
    public void SetEventReferences(GameEventManager manager, AirshipsAndAirIslands.Events.EventUI ui)
    {
        gameEventManager = manager;
        eventUI = ui;
    }
}
