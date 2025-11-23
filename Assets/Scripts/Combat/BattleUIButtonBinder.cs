using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Binds simple battle UI buttons (fire, subsystem cycling, abilities) to gameplay logic.
    /// </summary>
    public class BattleUIButtonBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private PlayerCombatController playerCombat;

    [Header("Flow")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Buttons")]
        [SerializeField] private Button fireButton;
        [SerializeField] private Button nextSubsystemButton;
        [SerializeField] private Button previousSubsystemButton;
    [SerializeField] private Button exitButton;

        [Header("Labels")]
        [SerializeField] private TMP_Text subsystemCountText;

        private void Awake()
        {
            battleManager ??= FindFirstObjectByType<BattleManager>();
            playerCombat ??= FindFirstObjectByType<PlayerCombatController>();
        }

        private void OnEnable()
        {
            WireButton(fireButton, HandleFireClicked);
            WireButton(nextSubsystemButton, HandleNextSubsystemClicked);
            WireButton(previousSubsystemButton, HandlePreviousSubsystemClicked);
            WireButton(exitButton, HandleExitClicked);

            if (battleManager != null)
            {
                battleManager.StateChanged += HandleStateChanged;
                battleManager.SubsystemChanged += HandleSubsystemChanged;
                battleManager.SubsystemRegistered += HandleSubsystemChanged;
                battleManager.SubsystemRemoved += HandleSubsystemChanged;
            }

            UpdateButtonStates();
        }

        private void OnDisable()
        {
            UnwireButton(fireButton, HandleFireClicked);
            UnwireButton(nextSubsystemButton, HandleNextSubsystemClicked);
            UnwireButton(previousSubsystemButton, HandlePreviousSubsystemClicked);
            UnwireButton(exitButton, HandleExitClicked);

            if (battleManager != null)
            {
                battleManager.StateChanged -= HandleStateChanged;
                battleManager.SubsystemChanged -= HandleSubsystemChanged;
                battleManager.SubsystemRegistered -= HandleSubsystemChanged;
                battleManager.SubsystemRemoved -= HandleSubsystemChanged;
            }
        }

        private void Update()
        {
            UpdateButtonStates();
        }

        private void HandleFireClicked()
        {
            playerCombat?.TryFire();
        }

        private void HandleNextSubsystemClicked()
        {
            battleManager?.CycleSubsystem(1);
        }

        private void HandlePreviousSubsystemClicked()
        {
            battleManager?.CycleSubsystem(-1);
        }

        private void HandleExitClicked()
        {
            if (string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                Debug.LogWarning("BattleUIButtonBinder: Main menu scene name not configured.");
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void HandleStateChanged(BattleManager.BattleState _)
        {
            UpdateButtonStates();
        }

        private void HandleSubsystemChanged(EnemySubsystem _)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var isRunning = battleManager != null && battleManager.CurrentState == BattleManager.BattleState.Running;
            var subsystemList = battleManager?.ActiveSubsystems;
            var subsystemCount = subsystemList?.Count ?? 0;

            if (fireButton != null)
            {
                // Allow firing when:
                // - battle is running
                // - playerCombat exists and is not reloading
                // - and either a subsystem is selected OR there is at least one active enemy (fallback to hull)
                var hasFallbackTarget = (battleManager?.ActiveEnemies != null && battleManager.ActiveEnemies.Count > 0);
                var hasSubsystemTarget = battleManager?.CurrentSubsystem != null;
                fireButton.interactable = isRunning && playerCombat != null && !playerCombat.IsReloading && (hasSubsystemTarget || hasFallbackTarget);
            }

            if (nextSubsystemButton != null)
            {
                nextSubsystemButton.interactable = isRunning && subsystemCount > 1;
            }

            if (previousSubsystemButton != null)
            {
                previousSubsystemButton.interactable = isRunning && subsystemCount > 1;
            }

            if (exitButton != null)
            {
                exitButton.interactable = true;
            }

            if (subsystemCountText != null)
            {
                var currentIndex = 0;
                if (battleManager != null && battleManager.CurrentSubsystem != null && subsystemList != null)
                {
                    for (var i = 0; i < subsystemCount; i++)
                    {
                        if (subsystemList[i] == battleManager.CurrentSubsystem)
                        {
                            currentIndex = i;
                            break;
                        }
                    }
                }

                subsystemCountText.text = subsystemCount > 0
                    ? $"{currentIndex + 1}/{subsystemCount}"
                    : "0/0";
            }
        }

        private static void WireButton(Button button, UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(action);
            // Ensure battle UI buttons play click SFX unless explicitly marked otherwise
            if (button.GetComponent<AirshipsAndAirIslands.Audio.UIButtonSound>() == null && button.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
            {
                button.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonSound>();
            }
        }

        private static void UnwireButton(Button button, UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }
    }
}
