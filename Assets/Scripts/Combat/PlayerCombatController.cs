using System;
using AirshipsAndAirIslands.Events;
using AirshipsAndAirIslands.Ship;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Handles the player's weapon fire cadence, ammo consumption, and damage output.
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameState gameState;
        [SerializeField] private ShipSystemsController shipSystems;
        [SerializeField] private BattleManager battleManager;

        [Header("Weapon Stats")]
        [SerializeField, Min(1)] private int baseDamage = 6;
        [SerializeField, Min(0f)] private float baseReloadSeconds = 2.5f;
        [SerializeField, Range(0f, 1f)] private float criticalChance = 0.08f;
        [SerializeField, Range(1f, 3f)] private float criticalMultiplier = 1.75f;
        [SerializeField, Min(1)] private int ammoPerShot = 1;

        [Header("Effects")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private GameObject muzzleFlashPrefab;

    private ShipSystemsState _systemsState = ShipSystemsState.CreateBaseline();
        private float _reloadTimer;

        public bool IsReloading => _reloadTimer > 0f;
        public float ReloadTimer => Mathf.Max(0f, _reloadTimer);

        public event Action<int> WeaponFired;
        public event Action ReloadStarted;
        public event Action ReloadFinished;

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
        }

        private void OnEnable()
        {
            if (shipSystems != null)
            {
                shipSystems.SystemsUpdated += HandleSystemsUpdated;
                _systemsState = shipSystems.CurrentState;
            }
        }

        private void OnDisable()
        {
            if (shipSystems != null)
            {
                shipSystems.SystemsUpdated -= HandleSystemsUpdated;
            }
        }

        private void Update()
        {
            if (_reloadTimer <= 0f)
            {
                return;
            }

            var previousTimer = _reloadTimer;
            _reloadTimer = Mathf.Max(0f, _reloadTimer - Time.deltaTime);
            if (_reloadTimer <= 0f && previousTimer > 0f)
            {
                ReloadFinished?.Invoke();
            }
        }

        public void SetBattleManager(BattleManager manager)
        {
            battleManager = manager;
        }

        public bool TryFire()
        {
            if (battleManager == null || battleManager.CurrentState != BattleManager.BattleState.Running)
            {
                return false;
            }

            return TryFireAt(battleManager.CurrentTarget);
        }

        public bool TryFireAt(EnemyAIController target)
        {
            if (target == null)
            {
                return false;
            }

            if (_reloadTimer > 0f)
            {
                return false;
            }

            if (gameState == null || !gameState.HasResource(ResourceType.Ammo, ammoPerShot))
            {
                return false;
            }

            gameState.ModifyResource(ResourceType.Ammo, -ammoPerShot);
            var damage = CalculateDamageRoll();
            target.ApplyDamage(damage);
            SpawnMuzzleEffect();
            BeginReload();
            WeaponFired?.Invoke(damage);
            return true;
        }

        public int ApplyIncomingDamage(int amount)
        {
            if (battleManager != null)
            {
                return battleManager.ApplyDamageToPlayer(amount);
            }

            if (gameState == null)
            {
                return 0;
            }

            var mitigated = Mathf.Max(0, amount);
            return gameState.ModifyResource(ResourceType.Hull, -mitigated);
        }

        public float GetReloadDuration()
        {
            var reloadModifier = Mathf.Clamp(1f + _systemsState.WeaponReloadModifier, 0.25f, 2f);
            return Mathf.Max(0.15f, baseReloadSeconds * reloadModifier);
        }

        private int CalculateDamageRoll()
        {
            var bonus = Mathf.RoundToInt(_systemsState.WeaponDamageBonus);
            var damage = Mathf.Max(1, baseDamage + bonus);
            var critRoll = UnityEngine.Random.value;
            if (critRoll <= criticalChance)
            {
                damage = Mathf.CeilToInt(damage * criticalMultiplier);
            }

            return damage;
        }

        private void BeginReload()
        {
            _reloadTimer = GetReloadDuration();
            if (_reloadTimer > 0f)
            {
                ReloadStarted?.Invoke();
            }
        }

        private void SpawnMuzzleEffect()
        {
            if (muzzleFlashPrefab == null)
            {
                return;
            }

            var position = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Instantiate(muzzleFlashPrefab, position, Quaternion.identity);
        }

        private void HandleSystemsUpdated(ShipSystemsState state)
        {
            _systemsState = state;
        }
    }
}
