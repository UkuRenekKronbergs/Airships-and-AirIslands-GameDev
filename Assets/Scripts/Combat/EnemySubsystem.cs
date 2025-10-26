using System;
using UnityEngine;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Represents a targetable subsystem on an enemy vessel, similar to FTL room components.
    /// </summary>
    public class EnemySubsystem : MonoBehaviour
    {
        [SerializeField] private string subsystemName = "Weapons";
        [SerializeField, Min(1)] private int maxIntegrity = 10;
        [SerializeField] private bool criticalSystem;
        [SerializeField] private int currentIntegrity;

        public string SubsystemName => subsystemName;
        public int MaxIntegrity => maxIntegrity;
        public int CurrentIntegrity => currentIntegrity;
        public bool IsCritical => criticalSystem;
        public bool IsDestroyed => currentIntegrity <= 0;

        public event Action<EnemySubsystem> IntegrityChanged;
        public event Action<EnemySubsystem> SubsystemDestroyed;

        private void Awake()
        {
            if (currentIntegrity <= 0)
            {
                currentIntegrity = maxIntegrity;
            }
            else
            {
                currentIntegrity = Mathf.Clamp(currentIntegrity, 0, maxIntegrity);
            }
        }

        public void ApplyDamage(int amount)
        {
            if (IsDestroyed)
            {
                return;
            }

            var damage = Mathf.Max(0, amount);
            if (damage == 0)
            {
                return;
            }

            var previous = currentIntegrity;
            currentIntegrity = Mathf.Max(0, currentIntegrity - damage);
            if (currentIntegrity == previous)
            {
                return;
            }

            IntegrityChanged?.Invoke(this);
            if (currentIntegrity == 0)
            {
                SubsystemDestroyed?.Invoke(this);
            }
        }

        public void Repair(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var previous = currentIntegrity;
            currentIntegrity = Mathf.Clamp(currentIntegrity + amount, 0, maxIntegrity);
            if (currentIntegrity != previous)
            {
                IntegrityChanged?.Invoke(this);
            }
        }
    }
}
