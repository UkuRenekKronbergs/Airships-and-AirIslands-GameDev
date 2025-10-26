using System;
using System.Collections.Generic;
using AirshipsAndAirIslands.Events;
using AirshipsAndAirIslands.Ship;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Coordinates combat flow in the battle scene and keeps the shared game state in sync.
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

        [Header("Runtime Debug")]
        [SerializeField] private BattleState currentState = BattleState.Inactive;
        [SerializeField] private EnemyAIController currentTarget;

        private readonly List<EnemyAIController> _activeEnemies = new();
        private float _stateTimer;
        private int _cachedHull = int.MinValue;
        private int _cachedAmmo = int.MinValue;

        public BattleState CurrentState => currentState;
        public EnemyAIController CurrentTarget => currentTarget;

        public event Action<BattleState> StateChanged;
        public event Action<BattleState> BattleEnded;
        public event Action<int> PlayerHullChanged;
        public event Action<int> PlayerAmmoChanged;
        public event Action<EnemyAIController> EnemyRegistered;
        public event Action<EnemyAIController> EnemyRemoved;
    public event Action<int, EnemyAIController> PlayerDamaged;

        private void Awake()
        {
            if (gameState == null)
            {
                gameState = FindFirstObjectByType<GameState>();
            }

            if (shipSystems == null)
            {
                shipSystems = FindFirstObjectByType<ShipSystemsController>();
            }

            if (playerCombat == null)
            {
                playerCombat = FindFirstObjectByType<PlayerCombatController>();
            }

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

            _activeEnemies.Clear();
        }

        public void BeginBattle()
        {
            if (currentState != BattleState.Inactive)
            {
                return;
            }

            UpdateResourceCaches(forceNotify: true);
            TransitionState(BattleState.Intro);
            _stateTimer = introDurationSeconds;
        }

        public void RegisterEnemy(EnemyAIController enemy)
        {
            if (enemy == null || _activeEnemies.Contains(enemy))
            {
                return;
            }

            _activeEnemies.Add(enemy);
            enemy.Destroyed += HandleEnemyDestroyed;
            EnemyRegistered?.Invoke(enemy);

            if (currentTarget == null)
            {
                SetCurrentTarget(enemy);
            }
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

            if (currentTarget == enemy)
            {
                SelectNextTarget();
            }
        }

        public void SetCurrentTarget(EnemyAIController enemy)
        {
            if (enemy != null && !_activeEnemies.Contains(enemy))
            {
                RegisterEnemy(enemy);
            }

            currentTarget = enemy;
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

            if (currentTarget == null && _activeEnemies.Count > 0)
            {
                currentTarget = _activeEnemies[0];
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

        private void SelectNextTarget()
        {
            currentTarget = _activeEnemies.Count > 0 ? _activeEnemies[0] : null;
        }

        private void TransitionState(BattleState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;

            if (currentState == BattleState.Running && currentTarget == null && _activeEnemies.Count > 0)
            {
                currentTarget = _activeEnemies[0];
            }

            StateChanged?.Invoke(currentState);
        }

        private void HandleBattleFinished(BattleState terminalState, IReadOnlyList<ResourceDelta> resourceChanges)
        {
            if (currentState == BattleState.Victory || currentState == BattleState.Defeat)
            {
                return;
            }

            if (gameState != null && resourceChanges != null)
            {
                gameState.ApplyResourceChanges(resourceChanges);
                UpdateResourceCaches(forceNotify: true);
            }

            TransitionState(terminalState);
            BattleEnded?.Invoke(terminalState);
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
