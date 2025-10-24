using System;
using UnityEngine;

namespace AirshipsAndAirIslands.Ship
{
    /// <summary>
    /// Base behaviour for rooms installed on the player's ship. Handles level management, power usage,
    /// and change notifications to the ship systems controller.
    /// </summary>
    public abstract class ShipRoom : MonoBehaviour
    {
        public enum RoomType
        {
            Engine,
            Weapons,
            Storage,
            Utility
        }

        [Header("Room Identity")]
        [SerializeField] private RoomType roomType;
        [SerializeField] private string displayName = "Unnamed Room";
        [SerializeField, TextArea] private string description;

        [Header("Operations")]
        [SerializeField, Min(0)] private int powerDraw = 1;
        [SerializeField, Range(1, 5)] private int maxLevel = 3;
        [SerializeField, Range(1, 5)] private int level = 1;
        [SerializeField] private bool isOnline = true;

        public event Action<ShipRoom> RoomChanged;

        public RoomType Type => roomType;
        public string DisplayName => displayName;
        public string Description => description;
        public int PowerDraw => powerDraw;
        public int Level => level;
        public int MaxLevel => maxLevel;
        public bool IsOnline => isOnline;

        /// <summary>
        /// Called by the ship systems controller whenever statistics need to be aggregated.
        /// </summary>
        public void Contribute(ref ShipSystemsState state)
        {
            if (!isOnline)
            {
                return;
            }

            state.TotalPowerDraw += powerDraw;
            ApplyActiveEffects(ref state);
        }

        /// <summary>
        /// Upgrades the room if possible and notifies listeners.
        /// </summary>
        public bool Upgrade()
        {
            if (level >= maxLevel)
            {
                return false;
            }

            level++;
            NotifyRoomChanged();
            return true;
        }

        public void SetOnline(bool value)
        {
            if (isOnline == value)
            {
                return;
            }

            isOnline = value;
            NotifyRoomChanged();
        }

        public void SetLevel(int newLevel)
        {
            var clampedLevel = Mathf.Clamp(newLevel, 1, maxLevel);
            if (clampedLevel == level)
            {
                return;
            }

            level = clampedLevel;
            NotifyRoomChanged();
        }

        protected abstract void ApplyActiveEffects(ref ShipSystemsState state);

        protected void NotifyRoomChanged()
        {
            RoomChanged?.Invoke(this);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            level = Mathf.Clamp(level, 1, maxLevel);
            NotifyRoomChanged();
        }
#endif
    }
}
