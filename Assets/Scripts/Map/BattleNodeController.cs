using UnityEngine;
using UnityEngine.SceneManagement;
using AirshipsAndAirIslands.Events;

public class BattleNodeController : MonoBehaviour
{
    [SerializeField] private BattleEncounterType encounterType = BattleEncounterType.Encounter1;
    [SerializeField] private string battleSceneName = "Battle";
    private MapHUD mapHUD;

    private void Start()
    {
        mapHUD = FindFirstObjectByType<MapHUD>();
    }

    private void OnMouseDown()
    {
        // Use the same fuel and path validation as travel nodes
        if (GameState.Instance == null || !GameState.Instance.IsMovementPossible())
        {
            return;
        }

        // Set encounter type
        GameState.Instance.CurrentEncounterType = encounterType;

        // Load with event system (TravelNodeController handles fuel deduction)
        if (mapHUD != null)
        {
            mapHUD.LoadBattleWithEvents();
        }
        else
        {
            // Fallback: load directly
            if (Application.CanStreamedLevelBeLoaded(battleSceneName))
            {
                SceneManager.LoadScene(battleSceneName);
            }
        }
    }
}

