using UnityEngine;

namespace PlayerController
{
    /// <summary>
    /// Player Max Speed value : should equal the player's max speed
    ///     Higher : camera lead scales slower
    ///     Lower : camera reaches full lead at lower speeds
    /// 
    /// Base Offset : camera resting position offset
    ///     X value is base x offset
    ///     Y value is base y offset
    ///     Z value is base z offset
    ///
    /// Direction Offset values : horizontal offset based on player facing direction
    ///     Direction offset amount : how much in the direction of facing the camera offsets
    ///     Direction offset smooth : how fast the camera smooths when direction changes
    ///         Higher : snappier
    ///         Lower : smoother
    ///
    /// Follow Smooth : Controls how quickly the camera returns to home when player stops
    ///     Higher : faster return
    ///     Lower : slower return
    /// 
    /// Lead Distance Values : How far the lead is allowed to go
    ///     Higher : See further forward / up / down
    ///     Lower : Camera view stays closer to player center
    /// 
    /// Lead Smooth values : How quickly the lead reacts
    ///     Higher: snappier reaction
    ///     Lower: Smoother reaction
    /// 
    /// Lead Return Values : How quickly the lead returns to center
    ///     Higher: Faster return
    ///     Lower : Slower return
    ///
    /// Lead Activation Speed : Speed at which leading will start to occur
    ///     Higher : player needs to move faster for a lead to occur
    ///     Lower : lead can start at lower speeds
    ///
    /// DeadZone Value : threshold for which movement value is ignored
    ///     Higher : micro movements are ignored more
    ///     Lower : camera reacts to every nudge
    ///
    /// DeadZoneDampen Value : How much lead is reduced when in the "dead zone"
    ///     Higher : stronger dampen
    ///     Lower : weaker dampen
    ///
    /// CurvePower : vertical lead curve
    ///     Higher : more exponential response
    ///     Lower : linear response
    ///
    /// Downward lead : minimum downward lead when falling
    ///     Higher : stronger downward pull during falls
    ///     Lower : softer downward motion during falls
    ///
    /// Fall Multiplier : multiplier for falling
    ///     Higher : camera moves more "aggressively" downwards during falls
    ///     Lower :
    ///
    /// Camera Softness : Global multiplier that softens all movement
    ///     Higher : Stronger lead influence
    ///     Lower : gentler movements
    ///
    /// Gizmo notes:
    ///     Cyan is home position. The base offset plus direction of facing for camera to be at.
    ///     Green is the camera focus center, this is center of screen,
    ///         it pushed out in direction of movement by factor of speed so visually the camera leads the player.
    ///     Magenta is camera lead. Where the camera center is trying to go, but Green is smoothing to that position
    /// </summary>
    public class CameraMovement2D : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        public float playerMaxSpeed = 8f;

        [Header("Directional Offset")] 
        public float directionalOffsetAmount = 0.5f;
        public float directionalOffsetSmooth = 4f;
        
        [Header("Follow Settings")]
        public float followSmooth = 4.2f;
        public Vector3 baseOffset = new(0f, 1f, -10f);
        
        [Header("Horizontal Lead Settings")]
        public float horizontalLeadDistance = 2.2f;
        public float horizontalLeadSmooth = 1.4f;
        public float horizontalLeadReturnSpeed = 1.2f;
        public float horizontalLeadActivationSpeed = 1.5f;
        public float horizontalDeadZone = 0.1f;
        public float horizontalDeadZoneDampen = 0.25f;
        
        [Header("Vertical Lead Settings")]
        public float verticalLeadDistance = 1.3f;
        public float verticalLeadSmooth = 2.2f;
        public float fallMultiplier = 1.4f;
        public float minDownwardLead = 0.4f;
        public float upwardLeadMultiplier = 0.6f;
        public float verticalDeadZone = 0.15f;
        public float verticalDeadZoneDampen = 0.25f;
        public float verticalCurvePower = 1.5f;
        
        [Header("Camera Feel")]
        [Range(0.1f, 1f)]
        public float cameraSoftness = 0.55f;
        
        private float _currentDirectionalOffset;
        private Vector3 _curLead;
        private Vector3 _leadTarget;
        private Vector3 _curVertLead;
        private Vector3 _vertLeadTarget;
        private Vector3 _lastTargetPos;
        
        [Header("Debug Settings")]
        private Vector3 _debugHomePos;
        private Vector3 _debugLeadPos;
        
        // helpers
        private Transform _playerVisual;

        private void Start()
        {
            if (target == null) return;
            
            _lastTargetPos = target.position;
            
            _playerVisual = target.Find("Visual");
            if (_playerVisual == null)
                Debug.LogError("CameraFollow2D: Could not find 'Visual' child under target!");
        }

        private void LateUpdate()
        {
            if (target == null)
                return;
            
            Vector3 targetDelta = target.position - _lastTargetPos;
            float hSpeed = targetDelta.x / Time.deltaTime;
            float vSpeed = targetDelta.y / Time.deltaTime;
            
            float absH = Mathf.Abs(hSpeed);
            float absV = Mathf.Abs(vSpeed);
            
            // 0 = precision camera, 1 = flow/speed camera
            float flowFactor = Mathf.Clamp01(absH / (playerMaxSpeed * 0.6f));
            flowFactor = Mathf.Pow(flowFactor, 2f);
            
            // HOME (CYAN)
            float facing = _playerVisual.right.x > 0 ? 1f : -1f;
            float targetDirectionalOffset = facing * directionalOffsetAmount;
            float dT = 1f - Mathf.Exp(-(directionalOffsetSmooth * Time.deltaTime));
            _currentDirectionalOffset = Mathf.Lerp(_currentDirectionalOffset, targetDirectionalOffset, dT);
    
            Vector3 directionalOffset = new(_currentDirectionalOffset, 0f, 0f);
            Vector3 homePos = target.position + baseOffset + directionalOffset; // CYAN
            _debugHomePos = homePos;
            
            // horizontal component
            if (absH > horizontalLeadActivationSpeed)
            {
                // cubic scaling = very gentle at low speeds, stronger at higher
                float speedPercent = Mathf.Pow(Mathf.Clamp01(absH / playerMaxSpeed), 3f);
                float scaledLead = horizontalLeadDistance * speedPercent;

                _leadTarget = Vector3.right * (Mathf.Sign(hSpeed) * scaledLead);
            }
            else
            {
                _leadTarget = Vector3.zero;
            }
            
            if (absH < horizontalDeadZone)
                _leadTarget *= horizontalDeadZoneDampen;
            
            float hT = 1f - Mathf.Exp(-((absH > horizontalLeadActivationSpeed ? horizontalLeadSmooth : horizontalLeadReturnSpeed) * Time.deltaTime));
            _curLead = Vector3.Lerp(_curLead, _leadTarget, hT);
            
            // vertical component
            float normalizedV = Mathf.Clamp(vSpeed / playerMaxSpeed, -1f, 1f);
            float vCurve = Mathf.Sign(normalizedV) * Mathf.Pow(Mathf.Abs(normalizedV), verticalCurvePower);
            float vLead = verticalLeadDistance * vCurve;
            
            if (vSpeed < 0)
            {
                vLead = Mathf.Min(vLead, -minDownwardLead);
                vLead *= fallMultiplier;
            }
            else
            {
                vLead *= upwardLeadMultiplier;
            }

            if (absV < verticalDeadZone)
                vLead *= verticalDeadZoneDampen;

            _vertLeadTarget = new Vector3(0f, vLead, 0f);

            // smooth the lead
            float vT = 1f - Mathf.Exp(-(verticalLeadSmooth * Time.deltaTime));
            _curVertLead = Vector3.Lerp(_curVertLead, _vertLeadTarget, vT);
            
            // apply the global softness
            float dynamicSoftness = Mathf.Lerp(0.4f, cameraSoftness, flowFactor);
            Vector3 softenedLead = (_curLead + _curVertLead) * dynamicSoftness;
            
            // direction offset
            Vector3 desiredPos = homePos + softenedLead;
            desiredPos.z = transform.position.z;

            _debugLeadPos = desiredPos;
            
            // movement state checks
            bool movingHoriz = absH > 0.05f;
            bool movingVert  = absV > 0.05f;
            bool isMoving = movingHoriz || movingVert;
            
            float intentSmooth = Mathf.Lerp(6f, 12f, flowFactor);
            float intentT = 1f - Mathf.Exp(-(intentSmooth * Time.deltaTime));
            
            float returnT = 1f - Mathf.Exp(-(followSmooth * Time.deltaTime));
            
            // apply movement
            Vector3 current = transform.position;
            Vector3 next = current;

            if (isMoving)
            {
                next += targetDelta;
                next.x = Mathf.Lerp(next.x, desiredPos.x, intentT);
                
                float verticalMultiplier = vSpeed < 0f ? 1.35f : 1.1f;
                next.y = Mathf.Lerp(next.y, desiredPos.y, intentT * verticalMultiplier);
            }
            else
            {
                // Idle / slowing: return to home
                next.x = Mathf.Lerp(current.x, homePos.x, returnT);
                next.y = Mathf.Lerp(current.y, homePos.y, returnT);
            }
            next.z = current.z;

            transform.position = next;

            _lastTargetPos = target.position;
        }
        
        private void OnDrawGizmos()
        {
            if (target == null) return;
            
            // GREEN = actual camera center
            Vector3 camXYPlayerZ = new(
                transform.position.x,
                transform.position.y,
                target.position.z
            );
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(camXYPlayerZ, 0.15f);

            // CYAN = home position (rest state)
            Vector3 homeXYPlayerZ = new(
                _debugHomePos.x,
                _debugHomePos.y,
                target.position.z
            );
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(homeXYPlayerZ, 0.2f);
            
            // MAGENTA = lead offset from home
            Vector3 leadXYPlayerZ = new(
                _debugLeadPos.x,
                _debugLeadPos.y,
                target.position.z
            );
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(leadXYPlayerZ, 0.15f);
            
            Gizmos.DrawLine(homeXYPlayerZ, leadXYPlayerZ);
        }
    }
}