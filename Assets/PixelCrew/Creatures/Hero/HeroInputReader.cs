using PixelCrew.UI.Hud;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelCrew.Creatures.Hero
{
    public class HeroInputReader : MonoBehaviour
    {
        [SerializeField] private Hero _hero;
        private HudController _hud;

        private void Start()
        {
            _hud = FindObjectOfType<HudController>();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>();
            _hero.SetDirection(direction);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            var jump = context.ReadValue<float>();
            _hero.SetJump(jump);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _hero.Interact();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _hero.Dash();
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _hero.Attack();
            }
        }

        public void OnUseQuickItem(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _hero.StartThrowing();
            }

            if (context.canceled)
            {
                _hero.UseQuickItem();
            }
        }

        public void OnSwitchItem(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _hero.SwitchItem();
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (!_hud.PauseIsAvailable) return;
                else
                {
                    _hud.OnSettings();
                }
            }
        }
    }
}