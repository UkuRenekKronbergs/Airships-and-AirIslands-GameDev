using UnityEngine;
using UnityEngine.UI;

namespace AirshipsAndAirIslands.Audio
{
    /// <summary>
    /// Attach to a GameObject with a Button to play the configured click SFX when pressed.
    /// This keeps UI scripts unchanged and lets designers opt-in per-button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            AudioManager.Instance?.PlayClick();
        }
    }
}
