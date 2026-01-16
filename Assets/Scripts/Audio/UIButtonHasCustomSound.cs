using UnityEngine;

namespace AirshipsAndAirIslands.Audio
{
    /// <summary>
    /// Marker component to indicate a Button already has a custom sound and should not be assigned the default click SFX.
    /// Attach this at runtime from components that play their own sounds (e.g. CityLocation, build presenters).
    /// </summary>
    public class UIButtonHasCustomSound : MonoBehaviour { }
}
