using System;
using System.Collections;
using System.Collections.Generic;
using AirshipsAndAirIslands.Events;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Simple state-based AI for an enemy raider. Moves toward the player, attacks at intervals,
    /// and disengages when heavily damaged.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAIController : MonoBehaviour
    {
        private enum EnemyState
        {
            Approaching,
            Attacking,
            Disengaging
        }

        [Header("Setup")]
        [SerializeField] private EnemyStats stats;
        [SerializeField] private Transform target;
        [SerializeField] private GameState gameState;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField, Min(0)] private int shieldOverloadBonusArmor = 3;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private EnemySubsystem[] subsystemOverrides;

    [Header("Runtime Debug")]
    [SerializeField] private int currentHull;
    [SerializeField] private EnemyState currentState;

    public event Action<EnemyAIController> Destroyed;

        private Rigidbody2D _rigidbody;
        private Coroutine _attackRoutine;
        private float _shieldOverloadTimer;
        private readonly List<EnemySubsystem> _subsystems = new();
    private bool _warnedMissingBattleManager;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            if (stats != null)
            {
                currentHull = stats.MaxHull;
            }

            CollectSubsystems();
        }

        private void Start()
        {
            EnsureGameStateReference();
            EnsureBattleManagerReference();

            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            currentState = EnemyState.Approaching;
            _attackRoutine = StartCoroutine(AttackLoop());
        }

        private void Update()
        {
            if (_shieldOverloadTimer > 0f)
            {
                _shieldOverloadTimer = Mathf.Max(0f, _shieldOverloadTimer - Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (stats == null || target == null)
            {
                return;
            }

            switch (currentState)
            {
                case EnemyState.Approaching:
                    ApproachTarget();
                    break;
                case EnemyState.Attacking:
                    MaintainOptimalRange();
                    break;
                case EnemyState.Disengaging:
                    Disengage();
                    break;
            }

            EvaluateStateTransitions();
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                if (currentState == EnemyState.Attacking)
                {
                    FireWeapons();
                    yield return new WaitForSeconds(stats.AttackIntervalSeconds);
                }
                else
                {
                    yield return null;
                }
            }
        }

        public EnemyStats Stats => stats;
        public int CurrentHull => currentHull;
    public IReadOnlyList<EnemySubsystem> GetSubsystems() => _subsystems;

        private void FireWeapons()
        {
            var baseDamage = stats.AttackDamage;
            var bonus = IsAtOptimalRange() ? stats.OptimalRangeBonus : 0;
            var critRoll = UnityEngine.Random.value;
            var damage = baseDamage + bonus;
            if (critRoll <= stats.CriticalChance)
            {
                damage = Mathf.CeilToInt(damage * stats.CriticalMultiplier);
            }

            // TODO: Integrate with player's combat system.
            Debug.Log($"{stats.EnemyName} fires for {damage} damage (bonus {bonus}).");

            EnsureBattleManagerReference();
            EnsureGameStateReference();

            if (battleManager != null)
            {
                battleManager.ApplyDamageToPlayer(damage, this);
            }
            else if (gameState != null)
            {
                gameState.ModifyResource(ResourceType.Hull, -damage);

                if (!_warnedMissingBattleManager)
                {
                    Debug.LogWarning("EnemyAIController could not locate a BattleManager; applying damage directly to GameState will bypass damage indicators.", this);
                    _warnedMissingBattleManager = true;
                }
            }

            if (muzzleFlashPrefab != null)
            {
                Instantiate(muzzleFlashPrefab, transform.position, Quaternion.identity);
            }
        }

        private void ApproachTarget()
        {
            var direction = (target.position - transform.position).normalized;
            _rigidbody.linearVelocity = direction * stats.PursuitSpeed;
        }

        private void MaintainOptimalRange()
        {
            var distance = Vector2.Distance(transform.position, target.position);
            if (distance < stats.EngagementRange * 0.9f)
            {
                _rigidbody.linearVelocity = (transform.position - target.position).normalized * stats.PursuitSpeed * 0.5f;
            }
            else if (distance > stats.EngagementRange * 1.1f)
            {
                ApproachTarget();
            }
            else
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void Disengage()
        {
            _rigidbody.linearVelocity = (transform.position - target.position).normalized * (stats.PursuitSpeed * 1.2f);
        }

        private void EvaluateStateTransitions()
        {
            if (stats == null || target == null)
            {
                return;
            }

            var distance = Vector2.Distance(transform.position, target.position);
            var hullPercent = stats.MaxHull > 0 ? (float)currentHull / stats.MaxHull : 0f;

            if (hullPercent <= stats.DisengageThreshold)
            {
                currentState = EnemyState.Disengaging;
                return;
            }

            if (distance <= stats.EngagementRange)
            {
                currentState = EnemyState.Attacking;
            }
            else
            {
                currentState = EnemyState.Approaching;
            }
        }

        private bool IsAtOptimalRange()
        {
            if (stats == null || target == null)
            {
                return false;
            }

            var distance = Vector2.Distance(transform.position, target.position);
            return Mathf.Abs(distance - stats.EngagementRange) <= 1.5f;
        }

        public void ApplyDamage(int amount)
        {
            if (stats == null)
            {
                return;
            }

            var mitigated = Mathf.Max(1, amount - GetEffectiveArmor());
            currentHull = Mathf.Max(0, currentHull - mitigated);
            Debug.Log($"{stats.EnemyName} took {mitigated} damage (remaining hull {currentHull}).");

            if (currentHull <= 0)
            {
                HandleDestroyed();
            }
        }

        public void ApplyDamageToSubsystem(EnemySubsystem subsystem, int amount)
        {
            if (subsystem == null || !_subsystems.Contains(subsystem))
            {
                return;
            }

            subsystem.ApplyDamage(amount);
            ApplyDamage(amount);

            if (subsystem.IsCritical && subsystem.IsDestroyed)
            {
                HandleDestroyed();
            }
        }

        private int GetEffectiveArmor()
        {
            if (_shieldOverloadTimer > 0f)
            {
                return stats.Armor + shieldOverloadBonusArmor;
            }

            return stats.Armor;
        }

        private void HandleDestroyed()
        {
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
            }

            // TODO: Grant loot / update game state.
            Destroyed?.Invoke(this);
            Destroy(gameObject);
        }


        private void CollectSubsystems()
        {
            _subsystems.Clear();

            if (subsystemOverrides != null && subsystemOverrides.Length > 0)
            {
                foreach (var subsystem in subsystemOverrides)
                {
                    if (subsystem != null && !_subsystems.Contains(subsystem))
                    {
                        _subsystems.Add(subsystem);
                    }
                }
            }

            if (_subsystems.Count == 0)
            {
                var discovered = GetComponentsInChildren<EnemySubsystem>(includeInactive: true);
                foreach (var subsystem in discovered)
                {
                    if (subsystem != null && !_subsystems.Contains(subsystem))
                    {
                        _subsystems.Add(subsystem);
                    }
                }
            }
        }
        public void ApplyStatusEffectShieldOverload(float durationSeconds)
        {
            if (stats == null || !stats.HasShieldOverload)
            {
                return;
            }

            _shieldOverloadTimer = Mathf.Max(_shieldOverloadTimer, durationSeconds);
            Debug.Log($"{stats.EnemyName} activates Shield Overload for {durationSeconds:0.0}s.");
        }

        private void EnsureBattleManagerReference()
        {
            if (battleManager != null)
            {
                return;
            }

            battleManager = FindFirstObjectByType<BattleManager>();
        }

        private void EnsureGameStateReference()
        {
            if (gameState != null)
            {
                return;
            }

            gameState = FindFirstObjectByType<GameState>();
        }
    }
}

