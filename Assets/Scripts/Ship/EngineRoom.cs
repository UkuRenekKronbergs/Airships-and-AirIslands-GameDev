using System.Collections;
using UnityEngine;

namespace AirshipsAndAirIslands.Ship
{
    /// <summary>
    /// Provides thrust and fuel efficiency bonuses to the ship. Can trigger temporary overdrive bursts.
    /// </summary>
    public class EngineRoom : ShipRoom
    {
        [Header("Engine Performance")]
        [SerializeField, Min(0f)] private float baseThrust = 8f;
        [SerializeField, Min(0f)] private float thrustPerLevel = 4f;
        [SerializeField, Range(0f, 0.5f)] private float fuelEfficiencyPerLevel = 0.05f;

        [Header("Overdrive Ability")]
        [SerializeField] private float overdriveDurationSeconds = 5f;
        [SerializeField] private float overdriveThrustMultiplier = 1.5f;
        [SerializeField] private float overdriveCooldownSeconds = 15f;

        private float _overdriveTimer;
        private float _cooldownTimer;
        private Coroutine _overdriveRoutine;

        protected override void ApplyActiveEffects(ref ShipSystemsState state)
        {
            var level = Level;
            var thrust = baseThrust + thrustPerLevel * (level - 1);
            var efficiencyBonus = fuelEfficiencyPerLevel * level;

            if (_overdriveTimer > 0f)
            {
                thrust *= overdriveThrustMultiplier;
            }

            state.EngineThrust += thrust;
            state.FuelEfficiencyBonus += efficiencyBonus;
        }

        private void Update()
        {
            if (_overdriveTimer > 0f)
            {
                _overdriveTimer = Mathf.Max(0f, _overdriveTimer - Time.deltaTime);
                if (_overdriveTimer <= 0f)
                {
                    NotifyRoomChanged();
                }
            }

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer = Mathf.Max(0f, _cooldownTimer - Time.deltaTime);
            }
        }

        public bool TryActivateOverdrive()
        {
            if (_overdriveTimer > 0f || _cooldownTimer > 0f)
            {
                return false;
            }

            if (_overdriveRoutine != null)
            {
                StopCoroutine(_overdriveRoutine);
            }

            _overdriveRoutine = StartCoroutine(OverdriveRoutine());
            return true;
        }

        private IEnumerator OverdriveRoutine()
        {
            _overdriveTimer = overdriveDurationSeconds;
            _cooldownTimer = overdriveCooldownSeconds + overdriveDurationSeconds;
            NotifyRoomChanged();

            while (_overdriveTimer > 0f)
            {
                yield return null;
            }

            NotifyRoomChanged();
            _overdriveRoutine = null;
        }
    }
}
