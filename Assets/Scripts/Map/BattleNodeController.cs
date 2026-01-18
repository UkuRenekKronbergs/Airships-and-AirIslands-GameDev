using UnityEngine;
using UnityEngine.SceneManagement;
using AirshipsAndAirIslands.Events;

public class BattleNodeController : MonoBehaviour
{
    [SerializeField] private BattleEncounterType encounterType = BattleEncounterType.Encounter1;
    [SerializeField] private string battleSceneName = "Battle";

    private void OnMouseDown()
    {
        // Use the same fuel and path validation as travel nodes
        if (GameState.Instance == null || !GameState.Instance.IsMovementPossible())
        {
            return;
        }

        // Set encounter type (TravelNodeController will handle the movement and fuel deduction)
        GameState.Instance.CurrentEncounterType = encounterType;

        // Load battle scene
        if (Application.CanStreamedLevelBeLoaded(battleSceneName))
        {
            SceneManager.LoadScene(battleSceneName);
        }
        else
        {
            Debug.LogWarning($"BattleNodeController: Could not load battle scene '{battleSceneName}'");
        }
    }
}
