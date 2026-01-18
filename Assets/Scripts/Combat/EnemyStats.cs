using System;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Data container describing how tough an enemy unit is and how it behaves in combat.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Airships/Combat/Enemy Stats", order = 0)]
    public class EnemyStats : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyName = "Sky Marauder";
        [SerializeField, TextArea] private string description = "Light raider craft armed with chain guns.";

        [Header("Attributes")]
        [SerializeField, Min(1)] private int maxHull = 100;
        [SerializeField, Min(0)] private int armor = 2;
        [SerializeField, Min(1)] private int attackDamage = 4;
        [SerializeField, Min(0)] private float attackIntervalSeconds = 2.5f;


        [Header("Behavior")]
        [SerializeField] private float engagementRange = 12f;
        [SerializeField] private float pursuitSpeed = 4f;
        [SerializeField] private float disengageThreshold = 0.25f;

        [Header("Abilities")]
        [SerializeField] private bool hasShieldOverload = true;
        [SerializeField, Tooltip("Extra damage dealt when firing from optimal range.")]
        private int optimalRangeBonus = 2;

        public string EnemyName => enemyName;
        public string Description => description;
        public int MaxHull => maxHull;
        public int Armor => armor;
        public int AttackDamage => attackDamage;
        public float AttackIntervalSeconds => attackIntervalSeconds;
        public float EngagementRange => engagementRange;
        public float PursuitSpeed => pursuitSpeed;
        public float DisengageThreshold => disengageThreshold;
        public bool HasShieldOverload => hasShieldOverload;
        public int OptimalRangeBonus => optimalRangeBonus;
    }
}
