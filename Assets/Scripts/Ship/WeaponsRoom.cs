using System.Collections;
using UnityEngine;

namespace AirshipsAndAirIslands.Ship
{
    /// <summary>
    /// Enhances ship weapon output and reload speed. Supports a temporary charged volley ability.
    /// </summary>
    public class WeaponsRoom : ShipRoom
    {
        [Header("Weapon Buffs")]
        [SerializeField, Min(0f)] private float baseDamageBonus = 2f;
        [SerializeField, Min(0f)] private float damageBonusPerLevel = 1.5f;
        [SerializeField, Range(-0.5f, 0f)] private float baseReloadModifier = -0.1f;
        [SerializeField, Range(-0.2f, 0f)] private float reloadModifierPerLevel = -0.05f;

        [Header("Charged Volley Ability")]
        [SerializeField] private float chargedVolleyMultiplier = 1.75f;
        [SerializeField] private float chargedVolleyCooldown = 12f;

        private bool _chargedVolleyReady;
        private float _cooldownTimer;

        protected override void ApplyActiveEffects(ref ShipSystemsState state)
        {
            var level = Level;
            var damageBonus = baseDamageBonus + damageBonusPerLevel * (level - 1);
            var reloadModifier = baseReloadModifier + reloadModifierPerLevel * (level - 1);

            if (_chargedVolleyReady)
            {
                damageBonus *= chargedVolleyMultiplier;
            }

            state.WeaponDamageBonus += damageBonus;
            state.WeaponReloadModifier += reloadModifier;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer = Mathf.Max(0f, _cooldownTimer - Time.deltaTime);
                if (_cooldownTimer <= 0f)
                {
                    _chargedVolleyReady = true;
                    NotifyRoomChanged();
                }
            }
        }

        /// <summary>
        /// Consumes the charged volley to provide a momentary burst of damage.
        /// </summary>
        public bool ConsumeChargedVolley()
        {
            if (!_chargedVolleyReady)
            {
                return false;
            }

            _chargedVolleyReady = false;
            _cooldownTimer = chargedVolleyCooldown;
            NotifyRoomChanged();
            return true;
        }

        /// <summary>
        /// Forces the charged volley to become available immediately, bypassing the cooldown.
        /// </summary>
        public void ForceReadyVolley()
        {
            _chargedVolleyReady = true;
            _cooldownTimer = 0f;
            NotifyRoomChanged();
        }
    }
}
