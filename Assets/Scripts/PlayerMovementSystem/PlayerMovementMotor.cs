using System.Collections;
using GameState;
using GameState.Core;
using PlayerRespawnSystem;
using UnityEngine;

namespace PlayerMovementSystem
{
    /// <summary>
    /// MOTOR LAYER:
    ///
    /// This is designed convert intent into a movement
    ///
    /// Works kind of like a state machine
    /// Given the player's input, and our state, what should movement look like
    /// </summary>
    [RequireComponent(typeof(PlayerInputController), (typeof(PlayerCollisionResolver)))]
    public class PlayerMovementMotor : MonoBehaviour
    {
        [Header("References")]
        private PlayerInputController _input;
        private PlayerCollisionResolver _collisionResolver;
        [SerializeField] private Transform visual;
        [SerializeField] private PlayerAnimationController animatorController;

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
        
        private float _dashTimer;
        private float _dashCooldownTimer;
        private bool _hasGroundedSinceLastDash = true;
        private DashState _state = DashState.Normal;
        
        [Header("Dash Unlock")]
        public bool dashUnlocked = true;
        public bool airDashUnlocked;
        
        [Header("Crouch Fall Settings")]
        public float maxCrouchFallTime = 1.0f;
        public float crouchFallStaggerTime = 0.25f;
        public float bounceJumpMultiplier = 20f;
        
        private float _crouchFallPower;
        
        private bool _isCrouchFalling;
        private bool _isStaggered;
        
        private float _crouchFallTimer;
        private float _staggeredTimer;

        private bool _crouchFallLocked;
        private bool _attackLocked;

        [Header("Attack Settings")] 
        public float attackCooldown = 1.0f;
        public float attackRange = 1.25f;
        public int attackDamage = 1;
        public LayerMask enemyMask;
        
        private bool _canAttack;
        private float _attackCooldownTimer;

        [Header("Grounded Info")] 
        private bool IsGrounded { get; set; }
        private bool WasGrounded { get; set; }

        private Vector2 Velocity { get; set; }

        private bool InputLocked { get; set; }
        private bool _nextGroundingUnlocksPlayer;
        
        [Header("Invincibility Settings")]
        [SerializeField] private float flashInterval = 0.1f;
        private SpriteRenderer[] _allSpriteRenderers;
        private MaterialPropertyBlock _mpb;
        private Coroutine _flashRoutine;
        
        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _collisionResolver = GetComponent<PlayerCollisionResolver>();
            
            _allSpriteRenderers = visual.GetComponentsInChildren<SpriteRenderer>(true);
            
            // Register this player as the player to respawn
            PlayerRespawnManager.Instance.RegisterPlayer(transform);
            PlayerControlManager.Instance.RegisterMotor(this);
            
            GameStateManager.Instance.OnPlayerDamaged += HandleDamage;
            GameStateManager.Instance.OnPlayerInvincibilityStarted += HandleInvincibilityStart;
            GameStateManager.Instance.OnPlayerInvincibilityEnded += HandleInvincibilityEnd;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnPlayerDamaged -= HandleDamage;
                GameStateManager.Instance.OnPlayerInvincibilityStarted -= HandleInvincibilityStart;
                GameStateManager.Instance.OnPlayerInvincibilityEnded -= HandleInvincibilityEnd;
            }

            if (PlayerControlManager.Instance != null)
                PlayerControlManager.Instance.UnregisterMotor(this);
        }

        private void Update()
        {
            if (_input.PausePressed)
                PauseManager.Instance.TogglePause();

            if (PauseManager.IsPaused)
                return;
            
            float dt = Time.deltaTime;
            // if the pausing happened to keep dt at 0, skip frame
            if (dt <= 0f) return;

            HandleStagger(dt);
            if (_isStaggered) return;
            
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
            Vector2 desiredDisplacement = Velocity * dt;
            CollisionInfo info = _collisionResolver.Move(desiredDisplacement);
            
            // 4. update grounded state from collision info
            WasGrounded = IsGrounded;
            IsGrounded = info.Below;
            
            // 5. Apply corrected velocity
            transform.position += (Vector3)(info.Displacement);
            Velocity = info.Displacement / dt;
            
            // 6. post movement actions
            HandleSpriteFlip();
            HandleAttack();
            
            // This is commented out for a reason : Ask Nico
            //animatorController.SetMove(IsGrounded && Mathf.Abs(Velocity.x) > Mathf.Epsilon);
        }

        private void HandleStagger(float dt)
        {
            if (!_isStaggered) return;
            
            _staggeredTimer -= dt;
            
            Velocity = new Vector2(0f, Velocity.y);

            if (_staggeredTimer > 0f) return;

            _isStaggered = false;
            Debug.Log("Staggered Removed");

            _attackLocked = false;
            _crouchFallLocked = false;
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
                _attackLocked = false;

                if (_nextGroundingUnlocksPlayer)
                {
                    _nextGroundingUnlocksPlayer = false;
                    InputLocked = false;
                }

                if (_isCrouchFalling)
                {
                    float duration = _crouchFallTimer;
                    _crouchFallPower = Mathf.Clamp01(duration / maxCrouchFallTime);
                    
                    Debug.Log($"CROUCH FALL LANDED | Duration={duration:F2}s | Power={_crouchFallPower:F2}");
                    
                    _isCrouchFalling = false;
                    
                    _isStaggered = true;
                    Debug.Log("Staggered");
                    _staggeredTimer = crouchFallStaggerTime;
                }
            }

            if (IsGrounded && Velocity.y < 0)
                Velocity = new Vector2(Velocity.x, 0f);
        }
        
        /// <summary>
        /// Currently assigned to player/Jump input
        /// </summary>
        private void HandleJump()
        {
            if (InputLocked) return;
            
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
            if (InputLocked) return;
            
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
            if (Mathf.Abs(speedX) < Mathf.Epsilon) return;

            float targetYRotation = speedX > 0 ? 0f : 180f;
            Vector3 rotation = visual.localEulerAngles;
            rotation.y = targetYRotation;
            visual.localEulerAngles = rotation;
        }

        /// <summary>
        /// Currently assigned to player/Sprint input - press equals activate
        ///
        /// associated values:
        ///
        /// float dashSpeed
        /// float dashDuration
        /// float dashCooldown
        /// float _dashTimer
        /// float _dashCooldownTimer
        /// bool _hasGroundedSinceLastDash
        /// DashState _state
        ///
        /// currently a ground dash that is default (but lockable if we want)
        /// and an air dash with a mechanic allowing for unlocking the air dash
        ///
        /// bool dashUnlocked
        /// bool airDashUnlocked
        /// </summary>
        private void HandleDash(float dt)
        {
            if (InputLocked) return;
            
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
            
            float dir = visual.right.x > 0 ? 1f : -1f;
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

        /// <summary>
        /// Currently assigned to player/Crouch input - must hold input for true effect
        ///
        /// associated values:
        ///
        /// float maxCrouchFallTime
        /// bool _isCrouchFalling
        /// float _crouchFallTimer
        /// float _crouchFallPower
        ///
        /// handle crouch falls just deals with the input for the state.
        /// only allowed to initiate when in air
        /// upon this initiation, time is started
        /// <see cref="CheckGrounded"/>> for dealing with ending the crouch fall
        /// _crouchFallPower will end up being larger for how long crouch fall was utilized for
        /// </summary>
        /// <param name="dt"></param>
        private void HandleCrouchFall(float dt)
        {
            if (InputLocked) return;

            if (_crouchFallLocked) return;
            
            // only do crouch fall if user inputs intent for it
            if (!_input.CrouchHeld) return;
            
            // can't crouch fall from grounded state
            if (IsGrounded) return;
            
            // first frame of crouch fall input check
            if (!_isCrouchFalling)
            {
                _isCrouchFalling = true;
                _crouchFallTimer = 0f;
                _attackLocked = true;
            }
             
            // increment timer by delta time
            _crouchFallTimer += dt;

            // create heavier fall feel by increasing gravity value
            float boostedGravity = gravity * 1.75f;
            float newY = Velocity.y + boostedGravity * dt;
            newY = Mathf.Max(newY, maxFallSpeed * 1.5f);

            Velocity = new Vector2(Velocity.x, newY);
            
            CheckCrouchFallHit();
        }

        private void CheckCrouchFallHit()
        {
            if (_crouchFallLocked) return;
            
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, -0.5f);
            Vector2 size = new(0.9f, 0.4f);
            const float range = 0.3f;
            Vector2 direction = Vector2.down;

            RaycastHit2D hit = Physics2D.BoxCast(
                origin,
                size,
                0f,
                direction,
                range,
                enemyMask
            );
            Debug.DrawRay(origin, direction * range, Color.yellow, 0.1f);

            if (hit.collider == null) return;
            
            _crouchFallLocked = true;
            
            Debug.Log("Crouch Fall Hit!");
            
            IEnemy enemy = hit.collider.GetComponent<IEnemy>();
            enemy?.TakeDamage(attackDamage);
            
            DoBounceJump();
        }

        private void DoBounceJump()
        {
            float bounce = jumpForce * bounceJumpMultiplier;
            Velocity = new Vector2(Velocity.x, bounce);

            _jumpBufferCounter = 0f;
            _coyoteCounter = 0f;
        }

        /// <summary>
        /// Currently assigned to player/Attack input
        /// </summary>
        private void HandleAttack()
        {
            if (InputLocked) return;

            if (_attackLocked) return;
            
            // no attack during crouch fall (crouch fall is an attack)
            if (_isCrouchFalling) return;

            if (!_canAttack)
            {
                _attackCooldownTimer -= Time.deltaTime;
                if (_attackCooldownTimer <= 0f)
                    _canAttack = true;

                return;
            }

            // attack start
            if (!_input.AttackPressed) return;

            _canAttack = false;
            _attackCooldownTimer = attackCooldown;
            
            float facing = visual.right.x > 0 ? 1f : -1f;
            string debugString = facing > 0f ? "Right" : "Left";
            
            Debug.Log($"ATTACK | Dir={debugString}");
            
            animatorController.PlayAttack();
            PerformAttack();
        }

        private void PerformAttack()
        {
            float facing = visual.right.x > 0 ? 1f : -1f;
            
            Vector2 origin = (Vector2)transform.position + new Vector2(facing * 0.5f, 0.5f);
            Vector2 direction = new(facing, 0f);

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, attackRange, enemyMask);

            if (hit.collider != null)
            {
                IEnemy enemy = hit.collider.GetComponent<IEnemy>();
                enemy?.TakeDamage(attackDamage);
            }
            Debug.DrawRay(origin, direction * attackRange, Color.red, 0.2f);
        }

        public void PlayDeathAnimation()
        {
            LockInput();
            Velocity = Vector2.zero;
            
            animatorController.PlayDeath();
            animatorController.OnDeathAnimationComplete += HandleDeathAnimationComplete;
        }
        
        private void HandleDeathAnimationComplete()
        {
            animatorController.OnDeathAnimationComplete -= HandleDeathAnimationComplete;
            
            GameFlowManager.Instance.RespawnPlayer();
        }

        public void LockInput()
        {
            InputLocked = true;
            
            Velocity = new Vector2(0f, Velocity.y);
        }

        public void UnlockInput()
        {
            if (IsGrounded)
            {
                InputLocked = false;
                _nextGroundingUnlocksPlayer = false;
                return;
            }

            _nextGroundingUnlocksPlayer = true;
        }

        private void HandleDamage()
        {
            animatorController.PlayDamaged();
        }
        
        private void HandleInvincibilityStart()
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashSprite());
        }

        private void HandleInvincibilityEnd()
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            SetSpriteAlpha(1f);
        }
        
        private IEnumerator FlashSprite()
        {
            while (true)
            {
                SetSpriteAlpha(0.3f);
                yield return new WaitForSeconds(flashInterval);

                SetSpriteAlpha(1f);
                yield return new WaitForSeconds(flashInterval);
            }
        }

        private void SetSpriteAlpha(float alpha)
        {
            _mpb ??= new MaterialPropertyBlock();

            foreach (SpriteRenderer sr in _allSpriteRenderers)
            {
                sr.GetPropertyBlock(_mpb);
                Color c = sr.color;
                c.a = alpha;
                _mpb.SetColor("_Color", c);
                sr.SetPropertyBlock(_mpb);
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 origin = transform.position;
            Vector2 displacement = Velocity * Time.deltaTime;

            if (!(displacement.sqrMagnitude > Mathf.Epsilon)) return;
            
            Vector3 dir = ((Vector3)displacement).normalized;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(origin, origin + dir * 2f);
        }
    }
}