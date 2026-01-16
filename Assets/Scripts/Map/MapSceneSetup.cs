using System;
using UnityEngine;
using AirshipsAndAirIslands.Events;

/// <summary>
/// Ensures the Map scene has a GameEventManager and EventUI at runtime and wires them into MapHUD.
/// Attach this to a GameObject in the Map scene (or leave it absent — it will still run if present on any active object).
/// </summary>
public class MapSceneSetup : MonoBehaviour
{
    [Tooltip("If left empty the setup will create and configure a GameEventManager automatically.")]
    [SerializeField] private GameEventManager gameEventManagerPrefabInstance;

    [Tooltip("If left empty the setup will create and configure an EventUI automatically.")]
    [SerializeField] private AirshipsAndAirIslands.Events.EventUI eventUIPrefabInstance;

    private void Awake()
    {
        // Find existing MapHUD
        var mapHud = UnityEngine.Object.FindFirstObjectByType<MapHUD>();
        if (mapHud == null)
        {
            Debug.LogWarning("MapSceneSetup: MapHUD not found in scene. Skipping event wiring.");
            return;
        }

        // Ensure a GameEventManager exists. Prefer the one attached to the persistent GameState if available.
        var manager = gameEventManagerPrefabInstance ?? GameState.Instance?.GetComponent<GameEventManager>() ?? UnityEngine.Object.FindFirstObjectByType<GameEventManager>();
        if (manager == null)
        {
            var go = new GameObject("GameEventManager");
            manager = go.AddComponent<GameEventManager>();
            // Try to parent under a logical root if present
            var root = GameObject.Find("Managers");
            if (root != null) go.transform.SetParent(root.transform, false);
            Debug.Log("MapSceneSetup: Created GameEventManager at runtime.");
        }

        // Ensure an EventUI exists
        var eventUI = eventUIPrefabInstance ?? UnityEngine.Object.FindFirstObjectByType<AirshipsAndAirIslands.Events.EventUI>();
        if (eventUI == null)
        {
            var go = new GameObject("EventUI");
            eventUI = go.AddComponent<AirshipsAndAirIslands.Events.EventUI>();
            // Assign manager reference on EventUI if possible
            var eu = eventUI as AirshipsAndAirIslands.Events.EventUI;
            var emField = typeof(AirshipsAndAirIslands.Events.EventUI).GetField("gameEventManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (emField != null)
            {
                emField.SetValue(eventUI, manager);
            }

            Debug.Log("MapSceneSetup: Created EventUI at runtime and wired GameEventManager reference.");
        }
        else
        {
            // If EventUI exists but its gameEventManager field is null, try to set it.
            var emField = typeof(AirshipsAndAirIslands.Events.EventUI).GetField("gameEventManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (emField != null && emField.GetValue(eventUI) == null)
            {
                emField.SetValue(eventUI, manager);
            }
        }

        // Wire into MapHUD so MapHUD uses the created instances
        try
        {
            mapHud.SetEventReferences(manager, eventUI);
            Debug.Log("MapSceneSetup: Wired GameEventManager and EventUI into MapHUD.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"MapSceneSetup: Failed to set event references on MapHUD: {ex.Message}");
        }
    }
}
