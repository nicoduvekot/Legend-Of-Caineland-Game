using UnityEngine;

namespace PlayerMovementSystem
{
    /// <summary>
    /// INPUT LAYER:
    ///
    /// This layer is designed to receive movement intent from the user
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        private InputSystem_Actions input;
        
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool AttackPressed { get; private set; }
        
        private void Awake()
        {
            input = new InputSystem_Actions();
        }
        
        private void OnEnable()
        {
            input.Enable();

            // Move
            input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled  += ctx => MoveInput = Vector2.zero;

            // Jump
            input.Player.Jump.performed += ctx => JumpPressed = true;
            input.Player.Jump.started   += ctx => JumpHeld = true;
            input.Player.Jump.canceled  += ctx => JumpHeld = false;

            // Dash
            input.Player.Sprint.performed += ctx => DashPressed = true;

            // Crouch
            input.Player.Crouch.started  += ctx => CrouchHeld = true;
            input.Player.Crouch.canceled += ctx => CrouchHeld = false;

            // Attack
            input.Player.Attack.performed += ctx => AttackPressed = true;
        }
        
        private void LateUpdate()
        {
            JumpPressed = false;
            DashPressed = false;
            AttackPressed = false;
        }

        private void OnDisable()
        {
            input.Disable();
        }
    }
}