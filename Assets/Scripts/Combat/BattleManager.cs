using System;
using System.Collections.Generic;
using AirshipsAndAirIslands.Events;
using AirshipsAndAirIslands.Ship;
using AirshipsAndAirIslands.Audio;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Coordinates combat flow in the battle scene, enabling FTL-style subsystem targeting.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public enum BattleState
        {
            Inactive,
            Intro,
            Running,
            Victory,
            Defeat
        }

        [Header("References")]
        [SerializeField] private GameState gameState;
        [SerializeField] private PlayerCombatController playerCombat;
        [SerializeField] private ShipSystemsController shipSystems;

        [Header("Behaviour")]
        [SerializeField] private bool autoStart = true;
        [SerializeField, Min(0f)] private float introDurationSeconds = 1.25f;
        [SerializeField, Min(0f)] private float actionDelaySeconds = 0.6f;
        [SerializeField] private List<ResourceDelta> victoryRewards = new();
        [SerializeField] private List<ResourceDelta> defeatPenalties = new();
        [SerializeField, Min(1)] private int maxTrackedEnemies = 1;

        [Header("Runtime Debug")]
        [SerializeField] private BattleState currentState = BattleState.Inactive;
        [SerializeField] private EnemySubsystem currentSubsystem;

        private readonly List<EnemyAIController> _activeEnemies = new();
        private readonly List<EnemySubsystem> _activeSubsystems = new();
        private readonly Dictionary<EnemySubsystem, EnemyAIController> _subsystemOwners = new();
        private float _stateTimer;
        private bool _playerGoesFirst;
        private bool _allowEnemyAction;
        private bool _allowPlayerAction;
        private int _cachedHull = int.MinValue;
        private int _cachedAmmo = int.MinValue;
        private IReadOnlyList<ResourceDelta> _lastOutcomeChanges = Array.Empty<ResourceDelta>();
        private BattleState _lastOutcomeState = BattleState.Inactive;

        public BattleState CurrentState => currentState;
        public EnemySubsystem CurrentSubsystem => currentSubsystem;
        public IReadOnlyList<EnemyAIController> ActiveEnemies => _activeEnemies;
        public IReadOnlyList<EnemySubsystem> ActiveSubsystems => _activeSubsystems;
        public IReadOnlyList<ResourceDelta> LastOutcomeChanges => _lastOutcomeChanges;
        public BattleState LastOutcomeState => _lastOutcomeState;
        public IReadOnlyList<ResourceDelta> VictoryRewards => victoryRewards;
        public IReadOnlyList<ResourceDelta> DefeatPenalties => defeatPenalties;
        public bool PlayerGoesFirst => _playerGoesFirst;
        public bool AllowEnemyAction => _allowEnemyAction;
        public bool AllowPlayerAction => _allowPlayerAction;
        public float ActionDelaySeconds => actionDelaySeconds;

        public event Action<BattleState> StateChanged;
        public event Action<string> CombatLogMessage;

        /// <summary>
        /// Safe method for other classes to post messages into the combat log.
        /// Events cannot be invoked from outside the declaring type, so this helper
        /// provides a safe public entrypoint.
        /// </summary>
        public void PostCombatLog(string message)
        {
            CombatLogMessage?.Invoke(message);
        }
        public event Action<BattleState> BattleEnded;
        public event Action<int> PlayerHullChanged;
        public event Action<int> PlayerAmmoChanged;
        public event Action<EnemyAIController> EnemyRegistered;
        public event Action<EnemyAIController> EnemyRemoved;
        public event Action<EnemySubsystem> SubsystemRegistered;
        public event Action<EnemySubsystem> SubsystemRemoved;
        public event Action<EnemySubsystem> SubsystemChanged;
        public event Action<int, EnemyAIController> PlayerDamaged;
        public event Action<BattleState, IReadOnlyList<ResourceDelta>> BattleResult;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameState>();
            shipSystems ??= FindFirstObjectByType<ShipSystemsController>();
            playerCombat ??= FindFirstObjectByType<PlayerCombatController>();

            if (playerCombat != null)
            {
                playerCombat.SetBattleManager(this);
            }
        }

        private void OnEnable()
        {
            CollectEnemies();
            if (autoStart)
            {
                BeginBattle();
            }
        }

        private void OnDisable()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.Destroyed -= HandleEnemyDestroyed;
                }
            }

            foreach (var subsystem in _activeSubsystems)
            {
                if (subsystem != null)
                {
                    subsystem.SubsystemDestroyed -= HandleSubsystemDestroyed;
                    subsystem.IntegrityChanged -= HandleSubsystemIntegrityChanged;
                }
            }

            _activeEnemies.Clear();
            _activeSubsystems.Clear();
            _subsystemOwners.Clear();
            currentSubsystem = null;
        }

        public void BeginBattle()
        {
            if (currentState != BattleState.Inactive)
            {
                return;
            }

            _lastOutcomeChanges = Array.Empty<ResourceDelta>();
            _lastOutcomeState = BattleState.Inactive;
            UpdateResourceCaches(forceNotify: true);
            // Decide initiative: randomized for now. Player gets an initial advantage if true.
            _playerGoesFirst = UnityEngine.Random.value < 0.5f;
            // Start with both sides disabled; we will perform the initial action sequence
            // once the battle transitions to Running to avoid races.
            _allowEnemyAction = false;
            _allowPlayerAction = false;

            TransitionState(BattleState.Intro);
            _stateTimer = introDurationSeconds;
            AudioManager.Instance?.PlayEnemyEncounter();
        }

        public void RegisterEnemy(EnemyAIController enemy)
        {
            if (enemy == null || _activeEnemies.Contains(enemy))
            {
                return;
            }

            if (_activeEnemies.Count >= maxTrackedEnemies)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"BattleManager: Ignoring enemy '{enemy.name}' because maxTrackedEnemies is {maxTrackedEnemies}.");
#endif
                enemy.gameObject.SetActive(false);
                return;
            }

            _activeEnemies.Add(enemy);
            enemy.Destroyed += HandleEnemyDestroyed;
            Debug.Log($"BattleManager: Registered enemy {enemy.name}. ActiveEnemies={_activeEnemies.Count}");
            EnemyRegistered?.Invoke(enemy);

            RegisterEnemySubsystems(enemy);
        }

        public void UnregisterEnemy(EnemyAIController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            if (_activeEnemies.Remove(enemy))
            {
                enemy.Destroyed -= HandleEnemyDestroyed;
                Debug.Log($"BattleManager: Unregistered enemy {enemy.name}. ActiveEnemies={_activeEnemies.Count}");
                EnemyRemoved?.Invoke(enemy);
            }

            RemoveSubsystemsOwnedBy(enemy);
        }

        public void SetCurrentSubsystem(EnemySubsystem subsystem)
        {
            if (subsystem != null && !_activeSubsystems.Contains(subsystem))
            {
                if (_subsystemOwners.TryGetValue(subsystem, out var owner))
                {
                    RegisterEnemy(owner);
                }
                else
                {
                    return;
                }
            }

            if (currentSubsystem == subsystem)
            {
                return;
            }

            currentSubsystem = subsystem;
            SubsystemChanged?.Invoke(currentSubsystem);
        }

        public void CycleSubsystem(int step)
        {
            if (step == 0 || _activeSubsystems.Count == 0)
            {
                return;
            }

            var currentIndex = currentSubsystem != null ? _activeSubsystems.IndexOf(currentSubsystem) : -1;
            if (currentIndex < 0)
            {
                currentIndex = step > 0 ? -1 : 0;
            }

            var nextIndex = currentIndex + step;
            nextIndex %= _activeSubsystems.Count;
            if (nextIndex < 0)
            {
                nextIndex += _activeSubsystems.Count;
            }

            SetCurrentSubsystem(_activeSubsystems[nextIndex]);
        }

        public bool ApplyDamageToSubsystem(EnemySubsystem subsystem, int amount)
        {
            if (subsystem == null || !_subsystemOwners.TryGetValue(subsystem, out var owner))
            {
                return false;
            }

            owner.ApplyDamageToSubsystem(subsystem, amount);
            if (subsystem == currentSubsystem)
            {
                SubsystemChanged?.Invoke(currentSubsystem);
            }
            return true;
        }

        public EnemyAIController GetSubsystemOwner(EnemySubsystem subsystem)
        {
            return subsystem != null && _subsystemOwners.TryGetValue(subsystem, out var owner) ? owner : null;
        }

        private void Update()
        {
            switch (currentState)
            {
                case BattleState.Intro:
                    RunIntroState();
                    break;
                case BattleState.Running:
                    RunActiveState();
                    break;
            }
        }

        private void RunIntroState()
        {
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                TransitionState(BattleState.Running);
            }
        }

        private void RunActiveState()
        {
            CullMissingEnemies();
            UpdateResourceCaches();

            // Consider the battle finished when there are no active enemies.
            // Previously this checked for active subsystems, which could cause
            // an immediate victory if enemies did not expose subsystems. Use
            // the tracked enemy list to determine terminal victory instead.
            if (_activeEnemies.Count == 0)
            {
                HandleBattleFinished(BattleState.Victory, victoryRewards);
                return;
            }

            if (gameState != null && gameState.GetResource(ResourceType.Hull) <= 0)
            {
                HandleBattleFinished(BattleState.Defeat, defeatPenalties);
            }
        }

        private void HandleEnemyDestroyed(EnemyAIController enemy)
        {
            UnregisterEnemy(enemy);
        }

        private void HandleSubsystemDestroyed(EnemySubsystem subsystem)
        {
            RemoveSubsystem(subsystem);

            if (currentState == BattleState.Running && _activeSubsystems.Count == 0)
            {
                HandleBattleFinished(BattleState.Victory, victoryRewards);
            }
        }

        private void RegisterEnemySubsystems(EnemyAIController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var subsystems = enemy.GetSubsystems();
            if (subsystems == null)
            {
                return;
            }

            foreach (var subsystem in subsystems)
            {
                RegisterSubsystem(subsystem, enemy);
            }

            if (currentSubsystem == null && _activeSubsystems.Count > 0)
            {
                SetCurrentSubsystem(_activeSubsystems[0]);
            }
        }

        private void RegisterSubsystem(EnemySubsystem subsystem, EnemyAIController owner)
        {
            if (subsystem == null || _activeSubsystems.Contains(subsystem))
            {
                return;
            }

            if (subsystem.IsDestroyed)
            {
                return;
            }

            _activeSubsystems.Add(subsystem);
            _subsystemOwners[subsystem] = owner;
            subsystem.SubsystemDestroyed += HandleSubsystemDestroyed;
            subsystem.IntegrityChanged += HandleSubsystemIntegrityChanged;
            SubsystemRegistered?.Invoke(subsystem);
        }

        private void RemoveSubsystemsOwnedBy(EnemyAIController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var toRemove = new List<EnemySubsystem>();
            foreach (var kvp in _subsystemOwners)
            {
                if (kvp.Value == enemy)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var subsystem in toRemove)
            {
                RemoveSubsystem(subsystem);
            }
        }

        private void RemoveSubsystem(EnemySubsystem subsystem)
        {
            if (subsystem == null)
            {
                return;
            }

            if (_activeSubsystems.Remove(subsystem))
            {
                subsystem.SubsystemDestroyed -= HandleSubsystemDestroyed;
                subsystem.IntegrityChanged -= HandleSubsystemIntegrityChanged;
                SubsystemRemoved?.Invoke(subsystem);
            }

            _subsystemOwners.Remove(subsystem);

            if (currentSubsystem == subsystem)
            {
                SetCurrentSubsystem(_activeSubsystems.Count > 0 ? _activeSubsystems[0] : null);
            }
        }

        private void HandleSubsystemIntegrityChanged(EnemySubsystem subsystem)
        {
            if (subsystem == currentSubsystem)
            {
                SubsystemChanged?.Invoke(subsystem);
            }
        }

        /// <summary>
        /// Called when the player performs an action (fires). This will allow enemies to start
        /// firing if initiative granted the player the first action.
        /// </summary>
        public void NotifyPlayerActed()
        {
            // Player has used their action this turn; enable the enemy's action.
            _allowPlayerAction = false;
            _allowEnemyAction = true;
            Debug.Log($"BattleManager: NotifyPlayerActed() -> allowPlayer={_allowPlayerAction} allowEnemy={_allowEnemyAction}");
        }

        /// <summary>
        /// Called by enemies after they perform their action to transfer the turn
        /// back to the player.
        /// </summary>
        public void NotifyEnemyActed()
        {
            _allowEnemyAction = false;
            _allowPlayerAction = true;
            Debug.Log($"BattleManager: NotifyEnemyActed() -> allowPlayer={_allowPlayerAction} allowEnemy={_allowEnemyAction}");
        }

        private void CollectEnemies()
        {
#if UNITY_2023_1_OR_NEWER
            var enemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);
#else
            var enemies = FindObjectsOfType<EnemyAIController>();
#endif
            foreach (var enemy in enemies)
            {
                RegisterEnemy(enemy);
            }
        }

        private void CullMissingEnemies()
        {
            for (var i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null)
                {
                    _activeEnemies.RemoveAt(i);
                }
            }

            for (var i = _activeSubsystems.Count - 1; i >= 0; i--)
            {
                var subsystem = _activeSubsystems[i];
                if (subsystem == null)
                {
                    _activeSubsystems.RemoveAt(i);
                    _subsystemOwners.Remove(subsystem);
                }
            }

            if (currentSubsystem == null && _activeSubsystems.Count > 0)
            {
                SetCurrentSubsystem(_activeSubsystems[0]);
            }
        }

        private void UpdateResourceCaches(bool forceNotify = false)
        {
            if (gameState == null)
            {
                return;
            }

            var hull = gameState.GetResource(ResourceType.Hull);
            if (forceNotify || hull != _cachedHull)
            {
                _cachedHull = hull;
                PlayerHullChanged?.Invoke(hull);
            }

            var ammo = gameState.GetResource(ResourceType.Ammo);
            if (forceNotify || ammo != _cachedAmmo)
            {
                _cachedAmmo = ammo;
                PlayerAmmoChanged?.Invoke(ammo);
            }
        }

        private void TransitionState(BattleState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;

            if (currentState == BattleState.Running && currentSubsystem == null && _activeSubsystems.Count > 0)
            {
                SetCurrentSubsystem(_activeSubsystems[0]);
            }

            StateChanged?.Invoke(currentState);

            // When the battle becomes Running, perform the initial action sequence in a coroutine
            // to avoid race conditions between manual initial hits and enemy attack coroutines.
            if (currentState == BattleState.Running)
            {
                StartCoroutine(RunInitiativeSequence());
            }
        }

        private System.Collections.IEnumerator RunInitiativeSequence()
        {
            // Announce initiative
            CombatLogMessage?.Invoke(_playerGoesFirst ? "Player goes first." : "Enemy goes first.");

            // Enable the first actor's action and leave the other disabled. Actors
            // must call NotifyPlayerActed/NotifyEnemyActed to transfer the turn.
            if (_playerGoesFirst)
            {
                _allowPlayerAction = true;
                _allowEnemyAction = false;
            }
            else
            {
                _allowPlayerAction = false;
                _allowEnemyAction = true;
            }

            // Small delay to let HUD initialize before players can act.
            yield return new WaitForSeconds(actionDelaySeconds);
        }

        private void HandleBattleFinished(BattleState terminalState, IReadOnlyList<ResourceDelta> resourceChanges)
        {
            if (currentState == BattleState.Victory || currentState == BattleState.Defeat)
            {
                return;
            }

            IReadOnlyList<ResourceDelta> appliedChanges;

            if (resourceChanges != null)
            {
                if (gameState != null)
                {
                    gameState.ApplyResourceChanges(resourceChanges);
                    UpdateResourceCaches(forceNotify: true);
                }

                appliedChanges = new List<ResourceDelta>(resourceChanges).ToArray();
            }
            else
            {
                appliedChanges = Array.Empty<ResourceDelta>();
            }

            _lastOutcomeChanges = appliedChanges;
            _lastOutcomeState = terminalState;

            // When the battle finishes, ensure no further actions are possible and
            // stop enemy coroutines immediately.
            _allowEnemyAction = false;
            _allowPlayerAction = false;
            // Also stop attack coroutines on all active enemies to prevent any remaining
            // scheduled FireWeapons calls from running after the battle ends.
            foreach (var enemy in _activeEnemies)
            {
                try
                {
                    enemy?.StopAttacking();
                }
                catch (Exception)
                {
                    // ignore issues stopping coroutines on already-destroyed enemies
                }
            }

            TransitionState(terminalState);
            BattleEnded?.Invoke(terminalState);
            BattleResult?.Invoke(terminalState, appliedChanges);
        }

        public int ApplyDamageToPlayer(int amount, EnemyAIController source = null)
        {
            if (gameState == null || currentState != BattleState.Running)
            {
                return gameState != null ? gameState.GetResource(ResourceType.Hull) : 0;
            }

            var damage = Mathf.Max(0, amount);
            var hull = gameState.ModifyResource(ResourceType.Hull, -damage);
            UpdateResourceCaches(forceNotify: true);
            PlayerDamaged?.Invoke(damage, source);

            // Emit a concise combat-log message so the HUD and log show incoming damage.
            if (damage > 0)
            {
                var attacker = source != null && source.Stats != null ? source.Stats.EnemyName : "Enemy";
                PostCombatLog($"{attacker} hits you for {damage} damage.");
            }

            if (hull <= 0)
            {
                HandleBattleFinished(BattleState.Defeat, defeatPenalties);
            }

            return hull;
        }
    }
}
