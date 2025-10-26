using UnityEngine;
using UnityEngine.InputSystem;

namespace AirshipsAndAirIslands.Combat
{
    /// <summary>
    /// Routes input action callbacks to the player combat controller.
    /// </summary>
    public class BattleInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController playerCombat;
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private bool autoFireWhileHeld;

        private InputAction _fire;

        private void Awake()
        {
            playerCombat ??= FindFirstObjectByType<PlayerCombatController>();
        }

        private void OnEnable()
        {
            if (fireAction == null)
            {
                Debug.LogWarning("BattleInputHandler: No input action reference assigned.");
                return;
            }

            _fire = fireAction.action;
            if (_fire == null)
            {
                Debug.LogWarning("BattleInputHandler: Input action reference does not contain an action instance.");
                return;
            }

            _fire.Enable();
            _fire.performed += HandleFirePerformed;
            _fire.canceled += HandleFireCanceled;
        }

        private void OnDisable()
        {
            if (_fire == null)
            {
                return;
            }

            _fire.performed -= HandleFirePerformed;
            _fire.canceled -= HandleFireCanceled;
            _fire.Disable();
            _fire = null;
        }

        private void Update()
        {
            if (!autoFireWhileHeld || _fire == null || playerCombat == null)
            {
                return;
            }

            if (_fire.IsPressed())
            {
                playerCombat.TryFire();
            }
        }

        private void HandleFirePerformed(InputAction.CallbackContext _)
        {
            if (autoFireWhileHeld)
            {
                return;
            }

            playerCombat?.TryFire();
        }

        private void HandleFireCanceled(InputAction.CallbackContext _)
        {
            // Intentionally left blank. Hook retained so Update knows whether the binding exists.
        }
    }
}
