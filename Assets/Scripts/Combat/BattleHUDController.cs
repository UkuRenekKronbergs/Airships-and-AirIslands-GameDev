using System.Collections.Generic;
using System.Text;
using AirshipsAndAirIslands.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Binds shared combat state to simple HUD widgets.
    /// </summary>
    public class BattleHUDController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private PlayerCombatController playerCombat;
        [SerializeField] private GameState gameState;

        [Header("HUD Widgets")]
        [SerializeField] private TMP_Text hullValueText;
        [SerializeField] private TMP_Text ammoValueText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text subsystemNameText;
        [SerializeField] private TMP_Text damageIndicatorText;
        [SerializeField] private TMP_Text reloadStatusText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultDetailsText;
        [SerializeField] private Slider reloadProgress;
        [SerializeField, Min(0f)] private float damageIndicatorDuration = 1.5f;

        private float _damageIndicatorTimer;

        private void Awake()
        {
            battleManager ??= FindFirstObjectByType<BattleManager>();
            playerCombat ??= FindFirstObjectByType<PlayerCombatController>();
            gameState ??= FindFirstObjectByType<GameState>();

            HideResultPanel();
        }

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.PlayerHullChanged += HandleHullChanged;
                battleManager.PlayerAmmoChanged += HandleAmmoChanged;
                battleManager.StateChanged += HandleStateChanged;
                battleManager.PlayerDamaged += HandlePlayerDamaged;
                battleManager.SubsystemChanged += HandleSubsystemChanged;
                battleManager.BattleResult += HandleBattleResult;
            }

            if (playerCombat != null)
            {
                playerCombat.WeaponFired += HandleWeaponFired;
                playerCombat.ReloadStarted += HandleReloadStarted;
                playerCombat.ReloadFinished += HandleReloadFinished;
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.PlayerHullChanged -= HandleHullChanged;
                battleManager.PlayerAmmoChanged -= HandleAmmoChanged;
                battleManager.StateChanged -= HandleStateChanged;
                battleManager.PlayerDamaged -= HandlePlayerDamaged;
                battleManager.SubsystemChanged -= HandleSubsystemChanged;
                battleManager.BattleResult -= HandleBattleResult;
            }

            if (playerCombat != null)
            {
                playerCombat.WeaponFired -= HandleWeaponFired;
                playerCombat.ReloadStarted -= HandleReloadStarted;
                playerCombat.ReloadFinished -= HandleReloadFinished;
            }
        }

        private void Update()
        {
            UpdateReloadWidgets();
            UpdateDamageIndicator();
        }

        private void RefreshAll()
        {
            if (gameState != null)
            {
                HandleHullChanged(gameState.GetResource(ResourceType.Hull));
                HandleAmmoChanged(gameState.GetResource(ResourceType.Ammo));
            }

            if (battleManager != null)
            {
                HandleStateChanged(battleManager.CurrentState);
                HandleSubsystemChanged(battleManager.CurrentSubsystem);
                HideResultPanel();
            }
        }

        private void UpdateReloadWidgets()
        {
            if (playerCombat == null)
            {
                return;
            }

            if (reloadProgress == null && reloadStatusText == null)
            {
                return;
            }

            var reloadTime = playerCombat.GetReloadDuration();
            var remaining = playerCombat.ReloadTimer;
            var progress = reloadTime <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / reloadTime);

            if (reloadProgress != null)
            {
                reloadProgress.value = progress;
            }

            if (reloadStatusText != null)
            {
                reloadStatusText.text = remaining > 0f ? $"Reloading ({remaining:0.0}s)" : "Ready";
            }
        }

        private void UpdateDamageIndicator()
        {
            if (damageIndicatorText == null || _damageIndicatorTimer <= 0f)
            {
                return;
            }

            _damageIndicatorTimer = Mathf.Max(0f, _damageIndicatorTimer - Time.deltaTime);
            if (_damageIndicatorTimer <= 0f)
            {
                damageIndicatorText.text = string.Empty;
            }
        }

        private void HandleHullChanged(int hull)
        {
            if (hullValueText != null)
            {
                hullValueText.text = $"Hull: {hull}";
            }
        }

        private void HandleAmmoChanged(int ammo)
        {
            if (ammoValueText != null)
            {
                ammoValueText.text = $"Ammo: {ammo}";
            }
        }

        private void HandleStateChanged(BattleManager.BattleState state)
        {
            if (statusText != null)
            {
                statusText.text = state.ToString();
            }

            if (state == BattleManager.BattleState.Inactive ||
                state == BattleManager.BattleState.Intro ||
                state == BattleManager.BattleState.Running)
            {
                HideResultPanel();
            }
        }

        private void HandlePlayerDamaged(int damage, EnemyAIController _)
        {
            if (damageIndicatorText == null)
            {
                return;
            }

            damageIndicatorText.text = damage > 0 ? $"Hit: -{damage}" : string.Empty;
            _damageIndicatorTimer = damageIndicatorDuration;
        }

        private void HandleSubsystemChanged(EnemySubsystem subsystem)
        {
            if (subsystemNameText == null)
            {
                return;
            }

            if (subsystem == null)
            {
                subsystemNameText.text = "No Subsystem";
                return;
            }

            var label = subsystem.SubsystemName;
            subsystemNameText.text = string.IsNullOrWhiteSpace(label)
                ? $"Subsystem ({subsystem.CurrentIntegrity}/{subsystem.MaxIntegrity})"
                : $"{label} ({subsystem.CurrentIntegrity}/{subsystem.MaxIntegrity})";
        }

        private void HandleBattleResult(BattleManager.BattleState state, IReadOnlyList<ResourceDelta> deltas)
        {
            if (resultPanel == null)
            {
                return;
            }

            resultPanel.SetActive(true);

            if (resultTitleText != null)
            {
                resultTitleText.text = state switch
                {
                    BattleManager.BattleState.Victory => "Victory",
                    BattleManager.BattleState.Defeat => "Defeat",
                    _ => state.ToString()
                };
            }

            if (resultDetailsText != null)
            {
                resultDetailsText.text = FormatResourceChanges(deltas);
            }
        }

        private void HandleWeaponFired(int _)
        {
            UpdateReloadWidgets();
        }

        private void HandleReloadStarted()
        {
            if (reloadStatusText != null)
            {
                reloadStatusText.text = "Reloading";
            }
        }

        private void HandleReloadFinished()
        {
            if (reloadStatusText != null)
            {
                reloadStatusText.text = "Ready";
            }
        }

        private string FormatResourceChanges(IReadOnlyList<ResourceDelta> deltas)
        {
            if (deltas == null || deltas.Count == 0)
            {
                return "No resource changes.";
            }

            var builder = new StringBuilder();
            var appended = false;

            for (var i = 0; i < deltas.Count; i++)
            {
                var delta = deltas[i];
                if (delta.Amount == 0)
                {
                    continue;
                }

                var sign = delta.Amount > 0 ? "+" : "-";
                var amount = Mathf.Abs(delta.Amount);
                if (appended)
                {
                    builder.AppendLine();
                }

                builder.Append(sign);
                builder.Append(amount);
                builder.Append(' ');
                builder.Append(delta.Type);
                appended = true;
            }

            var result = builder.ToString();
            return string.IsNullOrWhiteSpace(result) ? "No significant changes." : result;
        }

        private void HideResultPanel()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (resultDetailsText != null)
            {
                resultDetailsText.text = string.Empty;
            }
        }
    }
}
