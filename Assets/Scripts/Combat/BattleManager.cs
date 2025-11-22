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

        public event Action<BattleState> StateChanged;
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

            if (_activeSubsystems.Count == 0)
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

            if (hull <= 0)
            {
                HandleBattleFinished(BattleState.Defeat, defeatPenalties);
            }

            return hull;
        }
    }
}
