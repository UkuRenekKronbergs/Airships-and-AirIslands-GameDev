using AirshipsAndAirIslands.Events;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Manages which enemy is active in the battle based on the encounter type set in GameState.
    /// Runs before BattleManager so the correct enemy is found during CollectEnemies().
    /// </summary>
    public class EncounterSetup : MonoBehaviour
    {
        [SerializeField] private GameObject enemy0; // PirateCrew
        [SerializeField] private GameObject enemy1; // Encounter2
        [SerializeField] private GameObject enemy2; // Encounter3

        private void Awake()
        {
            // Disable all enemies first
            if (enemy0 != null) enemy0.SetActive(false);
            if (enemy1 != null) enemy1.SetActive(false);
            if (enemy2 != null) enemy2.SetActive(false);

            // Enable the correct enemy based on encounter type
            if (GameState.Instance != null)
            {
                switch (GameState.Instance.CurrentEncounterType)
                {
                    case BattleEncounterType.Encounter1:
                        if (enemy0 != null) enemy0.SetActive(true);
                        break;
                    case BattleEncounterType.Encounter2:
                        if (enemy1 != null) enemy1.SetActive(true);
                        break;
                    case BattleEncounterType.Encounter3:
                        if (enemy2 != null) enemy2.SetActive(true);
                        break;
                }
            }
            else
            {
                Debug.LogWarning("EncounterSetup: GameState not found!");
                // Fallback: enable first enemy
                if (enemy0 != null) enemy0.SetActive(true);
            }
        }
    }
}
