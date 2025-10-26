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
        [SerializeField] private TMP_Text damageIndicatorText;
        [SerializeField] private TMP_Text reloadStatusText;
        [SerializeField] private Slider reloadProgress;
        [SerializeField, Min(0f)] private float damageIndicatorDuration = 1.5f;

        private float _damageIndicatorTimer;

        private void Awake()
        {
            battleManager ??= FindFirstObjectByType<BattleManager>();
            playerCombat ??= FindFirstObjectByType<PlayerCombatController>();
            gameState ??= FindFirstObjectByType<GameState>();
        }

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.PlayerHullChanged += HandleHullChanged;
                battleManager.PlayerAmmoChanged += HandleAmmoChanged;
                battleManager.StateChanged += HandleStateChanged;
                battleManager.PlayerDamaged += HandlePlayerDamaged;
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
                hullValueText.text = hull.ToString();
            }
        }

        private void HandleAmmoChanged(int ammo)
        {
            if (ammoValueText != null)
            {
                ammoValueText.text = ammo.ToString();
            }
        }

        private void HandleStateChanged(BattleManager.BattleState state)
        {
            if (statusText != null)
            {
                statusText.text = state.ToString();
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
    }
}
