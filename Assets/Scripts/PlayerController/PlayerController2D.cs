using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(CharacterController2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 8f;
        public float jumpForce = 14f;
        public float gravity = -40f;
        
        [Header("Coyote Time")]
        public float coyoteTime = 0.1f;
        private float coyoteCounter;

        private CharacterController2D controller;
        private Animator animator;
        private Transform visual;

        private InputSystem_Actions input;
        private Vector2 moveInput;
        private bool jumpPressed;
        
        private Vector2 velocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController2D>();
            animator = GetComponentInChildren<Animator>();
            visual = transform.Find("Visual");

            input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            input.Enable();
            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            input.Player.Jump.performed += ctx => jumpPressed = true;
        }

        private void OnDisable()
        {
            input.Disable();
        }

        void Update()
        {
            if (moveInput.x != 0)
                visual.localScale = new Vector3(moveInput.x < 0 ? 1 : -1, 1, 1);

            //animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            //animator.SetBool("IsGrounded", controller.collisions.below);
        }

        private void FixedUpdate()
        {
            // Horizontal movement
            velocity.x = moveInput.x * moveSpeed;

            // Apply gravity
            velocity.y += gravity * Time.fixedDeltaTime;
            
            // coyote time
            if (controller.Collisions.Below)
                coyoteCounter = coyoteTime;
            else
                coyoteCounter -= Time.fixedDeltaTime;
            
            // Jump
            if (jumpPressed && coyoteCounter > 0f)
            {
                velocity.y = jumpForce;
                coyoteCounter = 0f;
            }
            
            jumpPressed = false;
            
            // Move using the collision controller
            controller.Move(velocity * Time.fixedDeltaTime);
            
            // Reset vertical velocity if grounded
            if (controller.Collisions.Below && velocity.y < 0)
            {
                velocity.y = 0;
            }
        }
    }
}
