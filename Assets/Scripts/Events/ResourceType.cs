using System;

namespace AirshipsAndAirIslands.Events
{
    /// <summary>
    /// Logical resource buckets the event system can modify.
    /// </summary>
    [Serializable]
    public enum ResourceType
    {
    Fuel,
    Food,
    Ammo,
    Gold,
        Hull,
        CrewMorale,
        CrewFatigue
    }
}
