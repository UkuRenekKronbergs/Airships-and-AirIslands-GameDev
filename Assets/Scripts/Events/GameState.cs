using System;
using System.Collections.Generic;
using UnityEngine;

namespace AirshipsAndAirIslands.Events
{
    /// <summary>
    /// Centralised mutable game state used by the event system. Designed to be referenced
    /// by other systems (UI, combat, economy) without tightly coupling their implementations.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        private static GameState _instance;

        /// <summary>
        /// Global singleton instance. Use `GameState.Instance` to access from other code.
        /// The instance created in the Main Menu will persist across scene loads.
        /// </summary>
        public static GameState Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }

            if (_instance == this)
            {
                // already the singleton
                return;
            }

            // A persistent instance already exists — destroy this duplicate GameObject
            Debug.Log("GameState: duplicate instance detected, destroying duplicate.");
            Destroy(gameObject);
        }
        [Header("Core Resources")]
        [SerializeField] private int gold = 20;
        [SerializeField] private int fuel = 10;
        [SerializeField] private int food = 15;
        [SerializeField] private int ammo = 10;
        [SerializeField, Min(1)] private int maxHull = 100;
        [SerializeField, Min(0)] private int hull = 10;

        [Header("Crew Stats (0-100)")]
        [Range(0, 100)][SerializeField] private int crewMorale = 60;
        [Range(0, 100)][SerializeField] private int crewFatigue = 40;

        [SerializeField] private string playerLocation;

        private readonly List<QuestInfo> _activeQuests = new();
        public IReadOnlyList<QuestInfo> ActiveQuests => _activeQuests;
        public int MaxHull => maxHull;

        private NodePair selectedPath;
        public GameObject ShipObject;
        public int GetGold() { 
            return gold;
        }

        public int GetResource(ResourceType type)
        {
            return type switch
            {
                ResourceType.Gold => gold,
                ResourceType.Fuel => fuel,
                ResourceType.Food => food,
                ResourceType.Ammo => ammo,
                ResourceType.Hull => hull,
                ResourceType.CrewMorale => crewMorale,
                ResourceType.CrewFatigue => crewFatigue,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public bool HasResource(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }

        public int ModifyResource(ResourceType type, int delta)
        {
            switch (type)
            {
                case ResourceType.Gold:
                    gold = Mathf.Max(0, gold + delta);
                    return gold;
                case ResourceType.Fuel:
                    fuel = Mathf.Max(0, fuel + delta);
                    return fuel;
                case ResourceType.Food:
                    food = Mathf.Max(0, food + delta);
                    return food;
                case ResourceType.Ammo:
                    ammo = Mathf.Max(0, ammo + delta);
                    return ammo;
                case ResourceType.Hull:
                    hull = Mathf.Clamp(hull + delta, 0, maxHull);
                    return hull;
                case ResourceType.CrewMorale:
                    crewMorale = Mathf.Clamp(crewMorale + delta, 0, 100);
                    return crewMorale;
                case ResourceType.CrewFatigue:
                    crewFatigue = Mathf.Clamp(crewFatigue + delta, 0, 100);
                    return crewFatigue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ApplyResourceChanges(IReadOnlyList<ResourceDelta> deltas)
        {
            if (deltas == null)
            {
                return;
            }

            foreach (var delta in deltas)
            {
                ModifyResource(delta.Type, delta.Amount);
            }
        }

        public void SetSelectedPath(NodePair path)
        {
            selectedPath = path;
        }

        public bool TryAddQuest(QuestInfo quest)
        {
            if (quest == null)
            {
                return false;
            }

            if (_activeQuests.Exists(q => q.QuestId == quest.QuestId))
            {
                return false;
            }

            _activeQuests.Add(quest);
            return true;
        }

        public bool CompleteQuest(string questId, out QuestInfo completedQuest)
        {
            completedQuest = null;
            if (string.IsNullOrWhiteSpace(questId))
            {
                return false;
            }

            var index = _activeQuests.FindIndex(q => q.QuestId == questId);
            if (index < 0)
            {
                return false;
            }

            completedQuest = _activeQuests[index];
            _activeQuests.RemoveAt(index);
            ApplyResourceChanges(completedQuest.Rewards);
            return true;
        }

        public bool IsHoveredMovementPossible()
        {
            if (selectedPath.a.name != playerLocation && selectedPath.b.name != playerLocation) return false;
            if (!checkMovementFuelRequirement()) return false;
            if (!checkMovementFoodRequirement()) return false;

            return true;
        }

        public bool checkMovementFuelRequirement()
        {
            if (selectedPath.a.name != playerLocation && selectedPath.b.name != playerLocation) return false;
            return true;
        }

        public bool checkMovementFoodRequirement()
        {
            if (selectedPath.distance + (int) (crewFatigue*0.1) > food) return false;
            return true;
        }

        public void MovePlayerLocation(string newLocation)
        {
            if (!IsHoveredMovementPossible()) return;

            playerLocation = newLocation;
            ModifyResource(ResourceType.Fuel, -selectedPath.distance);
            ModifyResource(ResourceType.Food, -(selectedPath.distance + (int) (crewFatigue*0.1)));
        }

        public string GetPlayerLocation()
        {
            return playerLocation;
        }
    }
}
