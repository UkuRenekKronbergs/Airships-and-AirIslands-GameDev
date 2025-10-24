using System;
using System.Collections.Generic;
using UnityEngine;

namespace AirshipsAndAirIslands.Ship
{
    /// <summary>
    /// Aggregates ship room contributions and exposes the derived ship capability snapshot to other systems.
    /// </summary>
    public class ShipSystemsController : MonoBehaviour
    {
        [SerializeField] private ShipSystemsState currentState = ShipSystemsState.CreateBaseline();

        private readonly List<ShipRoom> _rooms = new();

        public ShipSystemsState CurrentState => currentState;
        public event Action<ShipSystemsState> SystemsUpdated;

        private void OnEnable()
        {
            RecalculateSystems();
        }

        private void OnDisable()
        {
            foreach (var room in _rooms)
            {
                if (room != null)
                {
                    room.RoomChanged -= HandleRoomChanged;
                }
            }

            _rooms.Clear();
        }

        public void RecalculateSystems()
        {
            CollectRooms();

            var state = ShipSystemsState.CreateBaseline();
            foreach (var room in _rooms)
            {
                if (room == null)
                {
                    continue;
                }

                room.Contribute(ref state);
            }

            currentState = state;
            SystemsUpdated?.Invoke(currentState);
        }

        private void CollectRooms()
        {
            foreach (var room in _rooms)
            {
                if (room != null)
                {
                    room.RoomChanged -= HandleRoomChanged;
                }
            }

            _rooms.Clear();

            var rooms = GetComponentsInChildren<ShipRoom>(includeInactive: true);
            foreach (var room in rooms)
            {
                if (room == null)
                {
                    continue;
                }

                _rooms.Add(room);
                room.RoomChanged += HandleRoomChanged;
            }
        }

        private void HandleRoomChanged(ShipRoom room)
        {
            RecalculateSystems();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            RecalculateSystems();
        }
#endif
    }

    /// <summary>
    /// Snapshot of the ship's current systemic capabilities based on room configuration.
    /// </summary>
    [Serializable]
    public struct ShipSystemsState
    {
        public float EngineThrust;
        public float FuelEfficiencyBonus;
        public float WeaponDamageBonus;
        public float WeaponReloadModifier;
        public int StorageCapacityBonus;
        public int TotalPowerDraw;

        public static ShipSystemsState CreateBaseline()
        {
            return new ShipSystemsState
            {
                EngineThrust = 0f,
                FuelEfficiencyBonus = 0f,
                WeaponDamageBonus = 0f,
                WeaponReloadModifier = 0f,
                StorageCapacityBonus = 0,
                TotalPowerDraw = 0
            };
        }
    }
}
