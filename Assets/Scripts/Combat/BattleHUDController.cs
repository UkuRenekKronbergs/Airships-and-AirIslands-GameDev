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
        [SerializeField] private TMP_Text combatLogText;
        [SerializeField] private ScrollRect combatLogScrollRect;
        [SerializeField, Min(0f)] private float logMessageDelay = 0.8f;
        [SerializeField, Min(10)] private int maxLogLines = 200;
        [SerializeField] private TMP_Text subsystemNameText;
        [SerializeField] private TMP_Text damageIndicatorText;
        [SerializeField] private TMP_Text reloadStatusText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultDetailsText;
        [SerializeField] private Slider reloadProgress;
        [SerializeField, Min(0f)] private float damageIndicatorDuration = 1.5f;

        private float _damageIndicatorTimer;
        private readonly Queue<string> _logQueue = new();
        private readonly List<string> _logLines = new();
        private bool _processingLog;

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
                battleManager.CombatLogMessage += HandleCombatLogMessage;
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
                battleManager.CombatLogMessage -= HandleCombatLogMessage;
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
            // Note: initiative messages are sent via BattleManager.CombatLogMessage to avoid duplication.
        }

        private void HandlePlayerDamaged(int damage, EnemyAIController source)
        {
            if (damageIndicatorText != null)
            {
                damageIndicatorText.text = damage > 0 ? $"Hit: -{damage}" : string.Empty;
                _damageIndicatorTimer = damageIndicatorDuration;
            }

            // If no damage indicator is wired, fall back to the combat log so
            // the player still receives feedback about hits.
            if (damageIndicatorText == null)
            {
                var attacker = source != null && source.Stats != null ? source.Stats.EnemyName : "Enemy";
                EnqueueLogLine($"{attacker} hits you for {damage} damage.");
            }
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

            // Also log a concise summary to the combat log for clarity.
            if (state == BattleManager.BattleState.Victory)
            {
                EnqueueLogLine($"Victory! Rewards:\n{FormatResourceChanges(deltas)}");
            }
            else if (state == BattleManager.BattleState.Defeat)
            {
                EnqueueLogLine($"Defeat. Penalties:\n{FormatResourceChanges(deltas)}");
            }
        }

        private void HandleWeaponFired(int damage)
        {
            UpdateReloadWidgets();
            EnqueueLogLine($"You fire for {damage} damage.");
        }

        private void EnqueueLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            _logQueue.Enqueue(line);
            if (!_processingLog)
            {
                StartCoroutine(ProcessLogQueue());
            }
        }

        private System.Collections.IEnumerator ProcessLogQueue()
        {
            _processingLog = true;
            while (_logQueue.Count > 0)
            {
                var line = _logQueue.Dequeue();

                // Maintain a bounded history for scrolling/backlog.
                _logLines.Add(line);
                if (_logLines.Count > maxLogLines)
                {
                    _logLines.RemoveAt(0);
                }

                if (combatLogText != null)
                {
                    combatLogText.text = string.Join("\n", _logLines);
                    // If a ScrollRect is provided, scroll to bottom.
                    if (combatLogScrollRect != null)
                    {
                        Canvas.ForceUpdateCanvases();
                        combatLogScrollRect.verticalNormalizedPosition = 0f;
                    }
                }
                else if (damageIndicatorText != null)
                {
                    damageIndicatorText.text = line;
                    _damageIndicatorTimer = damageIndicatorDuration;
                }
                else
                {
                    Debug.Log(line);
                }

                yield return new WaitForSeconds(logMessageDelay);
            }

            _processingLog = false;
        }

        private void HandleCombatLogMessage(string msg)
        {
            EnqueueLogLine(msg);
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
