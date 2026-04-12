using UnityEngine;

namespace PlayerMovementSystem
{
    /// <summary>
    /// MOTOR LAYER:
    ///
    /// This is designed convert intent into a movement
    /// </summary>
    [RequireComponent(typeof(PlayerInputController), (typeof(PlayerCollisionResolver)))]
    public class PlayerMovementMotor : MonoBehaviour
    {
        [Header("References")]
        private PlayerInputController _input;
        private PlayerCollisionResolver _collisionResolver;
        [SerializeField] private Transform visual;
        
        [Header("Horizontal Movement")]
        public float maxRunSpeed = 8f;
        public float runAcceleration = 60f;
        public float runDeceleration = 50f;
        public float groundFriction = 8f;

        public float airAcceleration = 40f;
        public float airDeceleration = 30f;
        
        [Header("Vertical Movement")]
        public float gravity = -50f;
        public float maxFallSpeed = -20f;
        
        [Header("Jump Settings")]
        public float jumpForce = 15f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.1f;
        public float jumpCutMultiplier = 0.5f;
        public float apexGravityMultiplier = 0.5f;
        
        private float _coyoteCounter;
        private float _jumpBufferCounter;
        
        [Header("Dash Settings")]
        public float dashSpeed = 20f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 0.2f;
        
        [Header("Dash Unlock")]
        public bool dashUnlocked = true;
        public bool airDashUnlocked;
        
        private float _dashTimer;
        private float _dashCooldownTimer;
        private bool _hasGroundedSinceLastDash = true;
        private DashState _state = DashState.Normal;
        
        [Header("Crouch Settings")]
        public float maxCrouchFallTime = 1.0f;
        
        private bool _isCrouchFalling;
        private float _crouchFallTimer;
        private float _crouchFallPower;
        
        [Header("Attack Settings")]
        public float maxAttackChargeTime = 1.0f;
        
        private bool _isChargingAttack;
        private float _attackChargeTimer;

        [Header("Grounded Info")]
        private bool IsGrounded { get; set; }
        private bool WasGrounded { get; set; }

        public Vector2 Velocity { get; private set; }
        
        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _collisionResolver = GetComponent<PlayerCollisionResolver>();
        }
        
        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. update input-based timers
            UpdateJumpTimers();
            CheckGrounded();
            
            // 2. Compute desired velocity from intended movement
            HandleJump();
            HandleVerticalMovement(dt);
            HandleHorizontalMovement(dt);
            HandleDash(dt);
            HandleCrouchFall(dt);
            
            // 3. send desired to collision resolver
            Vector2 desiredVelocity = Velocity * dt;
            CollisionInfo info = _collisionResolver.Move(desiredVelocity);
            
            // 4. update grounded state from collision info
            WasGrounded = IsGrounded;
            IsGrounded = info.Below;
            
            // 5. Apply corrected velocity
            Velocity = info.Velocity / dt;
            transform.position += (Vector3)info.Velocity;
            
            // 6. post movement actions
            HandleSpriteFlip();
            HandleAttack();
        }
        
        private void UpdateJumpTimers()
        {
            // Jump buffer
            if (_input.JumpPressed)
                _jumpBufferCounter = jumpBufferTime;
            else
                _jumpBufferCounter -= Time.deltaTime;

            // Coyote time
            if (IsGrounded) // Note this is LAST frame's IsGrounded value
                _coyoteCounter = coyoteTime;
            else
                _coyoteCounter -= Time.deltaTime;
        }
        
        private void CheckGrounded()
        {
            if (IsGrounded && !WasGrounded)
            {
                _hasGroundedSinceLastDash = true;
                
                if (_isCrouchFalling)
                {
                    float duration = _crouchFallTimer;
                    _crouchFallPower = Mathf.Clamp01(duration / maxCrouchFallTime);
                    
                    Debug.Log($"CROUCH FALL LANDED | Duration={duration:F2}s | Power={_crouchFallPower:F2}");
                    
                    _isCrouchFalling = false;
                }
            }

            if (IsGrounded && Velocity.y < 0)
                Velocity = new Vector2(Velocity.x, 0f);
        }
        
        private void HandleJump()
        {
            // Jump trigger (buffer + coyote)
            if (_jumpBufferCounter > 0 && _coyoteCounter > 0)
            {
                Velocity = new Vector2(Velocity.x, jumpForce);

                // Consume timers
                _jumpBufferCounter = 0;
                _coyoteCounter = 0;
            }

            // Variable jump height (jump cut)
            if (!_input.JumpHeld && Velocity.y > 0)
            {
                Velocity = new Vector2(Velocity.x, Velocity.y * jumpCutMultiplier);
            }
        }
        
        private void HandleVerticalMovement(float dt)
        {
            // dash is overriding vertical movement
            if (_state == DashState.Dashing)
                return;
            
            if (IsGrounded) return;
            
            // Apex gravity reduction
            float apexPoint = Mathf.InverseLerp(0f, maxFallSpeed, Velocity.y);
            float gravityMultiplier = Mathf.Lerp(apexGravityMultiplier, 1f, apexPoint);

            float newY = Velocity.y + gravity * gravityMultiplier * dt;
            newY = Mathf.Max(newY, maxFallSpeed);

            Velocity = new Vector2(Velocity.x, newY);
        }
        
        private void HandleHorizontalMovement(float dt)
        {
            // active dash is overriding horizontal movement
            if (_state == DashState.Dashing)
                return;
            
            float inputX = _input.MoveInput.x;
            float speed = Velocity.x;

            if (Mathf.Abs(inputX) > 0.01f)
            {
                float targetSpeed = inputX * maxRunSpeed;
                float acceleration = IsGrounded ? runAcceleration : airAcceleration;

                speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * dt);
            }
            else
            {
                float deceleration = IsGrounded ? runDeceleration : airDeceleration;
                speed = Mathf.MoveTowards(speed, 0f, deceleration * dt);

                if (IsGrounded)
                    speed *= (1f - groundFriction * dt);
            }

            Velocity = new Vector2(speed, Velocity.y);
        }
        
        private void HandleSpriteFlip()
        {
            float speedX = Velocity.x;

            // Only flip when speed is meaningful
            if (Mathf.Abs(speedX) > 0.01f)
            {
                // Face right if moving right, left if moving left
                visual.localScale = new Vector3(
                    speedX > 0 ? 1 : -1,
                    visual.localScale.y,
                    visual.localScale.z
                );
            }
        }

        private void HandleDash(float dt)
        {
            // input check
            if (_state == DashState.Normal && dashUnlocked && _input.DashPressed)
            {
                if (CanDash())
                {
                    StartDash();
                }
            }
            
            // active
            if (_state == DashState.Dashing)
            {
                _dashTimer -= dt;
                if (_dashTimer <= 0f)
                {
                    EndDash();
                }

                return; // Skip normal movement while dashing
            }
            
            // cooldown
            if (_state == DashState.DashCooldown)
            {
                _dashCooldownTimer -= dt;
                if (_dashCooldownTimer <= 0f)
                {
                    _state = DashState.Normal;
                }
            }
        }

        private bool CanDash()
        {
            if (!airDashUnlocked)
                return IsGrounded && _dashCooldownTimer <= 0f;
            
            // air dash unlocked
            if (!IsGrounded && !_hasGroundedSinceLastDash)
                return false;
            
            return _dashCooldownTimer <= 0f;
        }

        private void StartDash()
        {
            _state = DashState.Dashing;
            _dashTimer = dashDuration;
            
            float dir = Mathf.Sign(visual.localScale.x);
            Velocity = new Vector2(dir * dashSpeed, 0f);
            
            // if air dash, mark that we must re-ground
            if (!IsGrounded)
                _hasGroundedSinceLastDash = false;
        }

        private void EndDash()
        {
            _state = DashState.DashCooldown;
            _dashCooldownTimer = dashCooldown;
        }

        private enum DashState
        {
            Normal,
            Dashing,
            DashCooldown
        }

        private void HandleCrouchFall(float dt)
        {
            if (IsGrounded)
                return;
            
            // If in air and crouch is held, begin or continue crouch-fall
            if (_input.CrouchHeld)
            {
                if (!_isCrouchFalling)
                {
                    _isCrouchFalling = true;
                    _crouchFallTimer = 0f;
                }
                
                _crouchFallTimer += dt;

                // create heavy fall feel
                float boostedGravity = gravity * 1.75f;
                float newY = Velocity.y + boostedGravity * dt;
                newY = Mathf.Max(newY, maxFallSpeed * 1.5f);

                Velocity = new Vector2(Velocity.x, newY);
            }
        }

        private void HandleAttack()
        {
            // no attack during crouch fall (crouch fall is an attack)
            if (_isCrouchFalling)
                return;
            
            // attack start
            if (_input.AttackPressed)
            {
                _isChargingAttack = true;
                _attackChargeTimer = 0f;
            }
            
            // charging state
            if (_isChargingAttack && _input.AttackHeld)
            {
                _attackChargeTimer += Time.deltaTime;
                return;
            }

            // release state
            if (_isChargingAttack && !_input.AttackHeld)
            {
                _isChargingAttack = false;
                
                float chargePower = Mathf.Clamp01(_attackChargeTimer / maxAttackChargeTime);
                string debugString;
                
// TODO : Commented out because I couldn't deal with my IDE telling it wasn't being used lmao
                //Vector2 attackDir;
            
                if (_input.UpHeld)
                {
                    //attackDir = Vector2.up;
                    debugString = "Up";
                }
                else if (_input.DownHeld)
                {
                    float facing = Mathf.Sign(visual.localScale.x);
                    //attackDir = new Vector2(facing, -1).normalized;
                    debugString = facing > 0 ? "Down-Right" : "Down-Left";
                }
                else
                {
                    float facing = Mathf.Sign(visual.localScale.x);
                    //attackDir = new Vector2(facing, 0);
                    debugString = facing > 0 ? "Right" : "Left";
                }
                
                Debug.Log($"ATTACK | Dir={debugString} | Power={chargePower:F2}");
            }
        }
    }
}