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
        private InputSystem_Actions _input;
        
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        
        // other
        public bool PausePressed { get; private set; }
        
        // Direction helpers (used for attack direction calculation)
        public bool UpHeld => MoveInput.y > 0.5f;
        public bool DownHeld => MoveInput.y < -0.5f;
        public bool LeftHeld => MoveInput.x < -0.5f;
        public bool RightHeld => MoveInput.x > 0.5f;
        
        private void Awake()
        {
            _input = new InputSystem_Actions();
        }
        
        private void OnEnable()
        {
            _input.Enable();
            
            // Pause
            _input.Player.Pause.performed += ctx => PausePressed = true;

            // Move
            _input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _input.Player.Move.canceled  += ctx => MoveInput = Vector2.zero;

            // Jump
            _input.Player.Jump.performed += ctx => JumpPressed = true;
            _input.Player.Jump.started   += ctx => JumpHeld = true;
            _input.Player.Jump.canceled  += ctx => JumpHeld = false;

            // Dash
            _input.Player.Sprint.performed += ctx => DashPressed = true;

            // Crouch
            _input.Player.Crouch.started  += ctx => CrouchHeld = true;
            _input.Player.Crouch.canceled += ctx => CrouchHeld = false;

            // Attack
            _input.Player.Attack.performed += ctx => AttackPressed = true;
            _input.Player.Attack.started  += ctx => AttackHeld = true;
            _input.Player.Attack.canceled += ctx => AttackHeld = false;
        }
        
        private void LateUpdate()
        {
            PausePressed = false;
            JumpPressed = false;
            DashPressed = false;
            AttackPressed = false;
        }

        private void OnDisable()
        {
            _input.Disable();
        }
    }
}