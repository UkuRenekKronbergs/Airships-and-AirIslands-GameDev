using AirshipsAndAirIslands.Events;
using UnityEngine;

namespace AirshipsAndAirIslands.Ship
{
    /// <summary>
    /// Expands cargo capacity and offers a refining ability that can convert spare resources into gold.
    /// </summary>
    public class StorageRoom : ShipRoom
    {
        [Header("Storage Settings")]
        [SerializeField, Min(0)] private int baseCapacity = 30;
        [SerializeField, Min(0)] private int capacityPerLevel = 15;

        [Header("Auto Refinery Ability")]
        [SerializeField] private bool refineryUnlocked = true;
        [SerializeField, Min(1)] private int foodToGoldRate = 3;
        [SerializeField, Min(1)] private int ammoToGoldRate = 2;

        [Tooltip("Optional reference to the shared game state so refinery actions can modify resources directly.")]
        [SerializeField] private GameState gameState;

        protected override void ApplyActiveEffects(ref ShipSystemsState state)
        {
            state.StorageCapacityBonus += CalculateCapacity();
        }

        public int CalculateCapacity()
        {
            return baseCapacity + capacityPerLevel * (Level - 1);
        }

        public void AssignGameState(GameState state)
        {
            gameState = state;
        }

        /// <summary>
        /// Converts a portion of food and ammunition into gold if the refinery is available, returning the gold generated.
        /// </summary>
        public int RunAutoRefinery(int maxCycles = 1)
        {
            if (!refineryUnlocked || gameState == null || maxCycles <= 0)
            {
                return 0;
            }

            var cycles = 0;
            var goldProduced = 0;

            while (cycles < maxCycles)
            {
                if (!gameState.HasResource(ResourceType.Food, foodToGoldRate) || !gameState.HasResource(ResourceType.Ammo, ammoToGoldRate))
                {
                    break;
                }

                gameState.ModifyResource(ResourceType.Food, -foodToGoldRate);
                gameState.ModifyResource(ResourceType.Ammo, -ammoToGoldRate);
                gameState.ModifyResource(ResourceType.Gold, 1);

                cycles++;
                goldProduced++;
            }

            if (goldProduced > 0)
            {
                NotifyRoomChanged();
            }

            return goldProduced;
        }
    }
}
