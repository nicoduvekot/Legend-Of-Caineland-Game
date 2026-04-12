using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Collision")] public LayerMask collisionMask;

        [Header("Raycast Settings")] public float skinWidth = 0.02f;
        public float maxSlopeAngle = 30f;

        private readonly List<DebugRay> _debugRays = new();

        private const float EPSILON = 0.0001f;
        private const float ANGLE_EPSILON = 0.5f;
        private const float SLOPE_SEAM_SMOOTH = 0.35f;

        private BoxCollider2D _boxCollider;
        private RaycastOrigins _raycastOrigins;

        public CollisionInfo Collisions;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        public void Move(Vector2 velocity)
        {
            UpdateRaycastOrigins();

            _debugRays.Clear();

            bool wasGroundedLastFrame = Collisions.Below;
            Collisions.Reset(wasGroundedLastFrame);

            Collisions.VelocityOld = velocity;

            if (Mathf.Abs(velocity.x) > EPSILON)
                ResolveGroundMovement(ref velocity);


            if (Mathf.Abs(velocity.y) > EPSILON)
            {
                VerticalCollisions(ref velocity);
            }

            transform.Translate(velocity);
        }

        private void ResolveGroundMovement(ref Vector2 velocity)
        {
            // 1. probe forward
            // 2. handle head hit
            // 3. handle knee hit
            // 4. handle foot only
            // 5. handle free forward
            // 6. apply ground snapping
            // 7. return corrected velocity

            // 1. Probe RayCasts
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            Vector2 footForward = directionX < 0 ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;
            Vector2 headForward = directionX < 0 ? _raycastOrigins.TopLeft : _raycastOrigins.TopRight;
            Vector2 kneeForward = Vector2.Lerp(footForward, headForward, 0.25f);

            RaycastHit2D footHit = CastAndRecord(footForward, Vector2.right * directionX, rayLength);
            RaycastHit2D kneeHit = CastAndRecord(kneeForward, Vector2.right * directionX, rayLength);
            RaycastHit2D headHit = CastAndRecord(headForward, Vector2.right * directionX, rayLength);

            bool canUseGroundLogic = Collisions.WasGroundedLastFrame && Collisions.VelocityOld.y <= EPSILON;

            // 2. Handle Forward Head Hit => stop
            if (headHit)
            {
                Debug.Log("STATE: BLOCKED (head hit)");
                BlockHorizontalMovement(ref velocity, directionX, headHit);
                return;
            }

            // 3. Handle Knee Hit => probe slope
            if (kneeHit)
            {
                if (!footHit)
                {
                    Debug.LogError("Knee hit but foot did NOT hit — invalid geometry.");
                    BlockHorizontalMovement(ref velocity, directionX, kneeHit);
                    return;
                }
                
                Vector2 slopeVector = kneeHit.point - footHit.point;
                
                float slopeAngle = Vector2.Angle(slopeVector.normalized, Vector2.up);

                if (slopeAngle <= maxSlopeAngle + ANGLE_EPSILON)
                {
                    
                    Debug.Log($"STATE: ASCEND (step slope) — angle={AngleStr(slopeAngle)}");
                    ClimbSlope(ref velocity, slopeAngle);
                    
                    return;
                }
                
                Debug.Log($"STATE: BLOCKED (step too tall or steep) — angle={AngleStr(slopeAngle)}");
                BlockHorizontalMovement(ref velocity, directionX, footHit);
                return;
            }

            // 4. Handle Foot Hit => ascend
            if (footHit)
            {
                float stepHeight = footHit.point.y - footForward.y;
                stepHeight = Mathf.Max(0f, stepHeight);
                
                Debug.Log($"STATE: STEP UP (foot only) — step={stepHeight:F3}");
                
                velocity.y = Mathf.Max(velocity.y, stepHeight);
                
                return;
            }

            if (!canUseGroundLogic) return;

            // 5. Free forward motion => Descend check
            TryDescendSlope(ref velocity, directionX);
            // 6. Ground Snapping
            SnapToGround(ref velocity);
        }

        private void TryDescendSlope(ref Vector2 velocity, float directionX)
        {
            if (!Collisions.WasGroundedLastFrame)
                return;

            const float DESCEND_CHECK_DISTANCE = 0.5f;

            if (!SampleGroundBelow(DESCEND_CHECK_DISTANCE, out RaycastHit2D hit))
                return;

            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Slope must be meaningful and shallow
            if (slopeAngle < ANGLE_EPSILON || slopeAngle > maxSlopeAngle)
                return;

            // meaningful change in slope angle
            if (Mathf.Abs(slopeAngle - Collisions.SlopeAngleOld) < ANGLE_EPSILON)
                slopeAngle = Collisions.SlopeAngleOld;

            // Ensure we are not already inside the ground
            if (hit.distance <= skinWidth)
                return;

            float moveDistance = Mathf.Abs(velocity.x);
            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            // Only descend if we are not moving upward faster than the slope
            if (velocity.y > descendVelocityY)
                return;

            // Target vertical delta
            float targetDeltaY = -descendVelocityY;

            // Blend instead of snapping fully
            float smoothedDeltaY = Mathf.Lerp(0f, targetDeltaY, SLOPE_SEAM_SMOOTH);
            velocity.y += smoothedDeltaY;

            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

            Collisions.Below = true;
            Collisions.DescendingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
            Collisions.SlopeAngleOld = slopeAngle;
        }

        private void SnapToGround(ref Vector2 velocity)
        {
            if (!Collisions.WasGroundedLastFrame)
                return;

            // Only snap if moving horizontally and not moving upward
            if (Mathf.Abs(velocity.x) < EPSILON)
                return;
            if (velocity.y > EPSILON)
                return;
            if (Collisions.DescendingSlope)
                return;

            const float SNAP_DISTANCE = 0.1f;

            if (!SampleGroundBelow(SNAP_DISTANCE + skinWidth, out RaycastHit2D hit))
                return;

            // If the ground is slightly below us, snap down
            if (hit.distance <= skinWidth || hit.distance > SNAP_DISTANCE + skinWidth)
                return;

            float snapAmount = hit.distance - skinWidth;
            float smoothedSnap = Mathf.Lerp(0f, snapAmount, SLOPE_SEAM_SMOOTH);
            velocity.y -= smoothedSnap;

            Collisions.Below = true;
        }

        private bool TryHorizontalCornerCorrection(ref Vector2 velocity, float directionX)
        {
            // Only correct when grounded and not on slopes
            if (!Collisions.Below || Collisions.ClimbingSlope || Collisions.DescendingSlope)
                return false;

            // Forward ray origin
            Vector2 forwardOrigin = directionX < 0 ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;

            // Cast forward ray
            float rayLength = skinWidth * 2f;
            RaycastHit2D forwardHit =
                Physics2D.Raycast(forwardOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (!forwardHit)
                return false;

            // Only correct vertical faces (not slopes)
            if (Mathf.Abs(forwardHit.normal.x) > 0.01f)
                return false;

            // Sample ground below (MRDS)
            if (!SampleGroundBelow(0.2f, out RaycastHit2D centerHit))
                return false;

            // Forward MRDS ray (slightly ahead)
            Vector2 forwardMRDSOrigin = forwardOrigin + new Vector2(directionX * skinWidth, 0f);
            RaycastHit2D forwardMRDS = Physics2D.Raycast(forwardMRDSOrigin, Vector2.down, 0.2f, collisionMask);

            // Only correct if center sees ground but forward does not
            if (forwardMRDS)
                return false;

            // Compute correction amount
            float correction = forwardHit.distance - skinWidth;

            if (correction is <= 0f or > 0.15f)
                return false;

            // Apply upward correction
            velocity.y += correction;

            return true;
        }

        private void BlockHorizontalMovement(ref Vector2 velocity, float directionX, RaycastHit2D hit)
        {
            float pushBack = Mathf.Max(hit.distance - skinWidth, 0f);

            if (pushBack < EPSILON)
                velocity.x = 0f;
            else
                velocity.x = pushBack * directionX;

            Collisions.Left = directionX < 0f;
            Collisions.Right = directionX > 0f;
        }

        private static float ComputeSlopeAngleBetween(RaycastHit2D footHit, RaycastHit2D kneeHit)
        {
            Vector2 slopeVector = kneeHit.point - footHit.point;

            if (slopeVector.sqrMagnitude < 0.0001f)
                return Vector2.Angle(footHit.normal, Vector2.up);

            float angle = Vector2.Angle(slopeVector.normalized, Vector2.up);

            return angle;
        }

// TODO : REVISIT!!
        private void VerticalCollisions(ref Vector2 velocity)
        {
            // 1. If climbing or descending slope, skip vertical clamp
            if (Collisions.ClimbingSlope || Collisions.DescendingSlope)
                return;

            // 2. Cast 3 vertical rays (left, center, right)
            // 3. Clamp velocity.y
            // 4. Set Above/Below

            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            Vector2 left = directionY < 0 ? _raycastOrigins.BottomLeft : _raycastOrigins.TopLeft;
            Vector2 right = directionY < 0 ? _raycastOrigins.BottomRight : _raycastOrigins.TopRight;
            Vector2 center = (left + right) * 0.5f;

            RaycastHit2D hitLeft = CastAndRecord(left, Vector2.up * directionY, rayLength);
            RaycastHit2D hitCenter = CastAndRecord(center, Vector2.up * directionY, rayLength);
            RaycastHit2D hitRight = CastAndRecord(right, Vector2.up * directionY, rayLength);

            RaycastHit2D hit = ClosestHit(hitLeft, hitCenter, hitRight);

            if (!hit)
                return;

            // If inside collider, zero out downward motion
            if (hit.distance <= skinWidth + EPSILON)
            {
                if (directionY < 0 && velocity.y < 0)
                    velocity.y = 0f;

                Collisions.Below = directionY < 0;
                Collisions.Above = directionY > 0;
                return;
            }

            // Clamp vertical movement
            velocity.y = (hit.distance - skinWidth) * directionY;

            Collisions.Below = directionY < 0;
            Collisions.Above = directionY > 0;
        }

        private bool SampleGroundBelow(float maxDistance, out RaycastHit2D bestHit)
        {
            bestHit = new RaycastHit2D();

            Vector2 left = _raycastOrigins.BottomLeft;
            Vector2 right = _raycastOrigins.BottomRight;
            Vector2 midLeft = Vector2.Lerp(left, right, 0.25f);
            Vector2 center = Vector2.Lerp(left, right, 0.50f);
            Vector2 midRight = Vector2.Lerp(left, right, 0.75f);

            // 1. Center ray (authoritative)
            RaycastHit2D c = CastAndRecord(center, Vector2.down, maxDistance);
            if (c && c.normal.y > 0.5f)
            {
                bestHit = c;
                return true;
            }

            // 2. Mid rays (supportive)
            RaycastHit2D ml = CastAndRecord(midLeft, Vector2.down, maxDistance);
            if (ml && ml.normal.y > 0.5f)
            {
                bestHit = ml;
                return true;
            }

            RaycastHit2D mr = CastAndRecord(midRight, Vector2.down, maxDistance);
            if (mr && mr.normal.y > 0.5f)
            {
                bestHit = mr;
                return true;
            }

            // 3. Outer rays (only for slope continuity, NOT grounding)
            RaycastHit2D l = CastAndRecord(left, Vector2.down, maxDistance);
            RaycastHit2D r = CastAndRecord(right, Vector2.down, maxDistance);

            // Only accept outer rays if they are BOTH valid and similar
            if (l && r && l.normal.y > 0.5f && r.normal.y > 0.5f)
            {
                bestHit = (l.distance < r.distance) ? l : r;
                return true;
            }

            return false;
        }

        private RaycastHit2D ClosestHit(params RaycastHit2D[] hits)
        {
            RaycastHit2D best = new();
            float bestDist = float.MaxValue;

            foreach (RaycastHit2D hit in hits)
            {
                if (!hit) continue;

                if (!(hit.distance < bestDist)) continue;
                best = hit;
                bestDist = hit.distance;
            }

            return best;
        }

        private void ClimbSlope(ref Vector2 velocity, float slopeAngle)
        {
            if (Mathf.Abs(slopeAngle - Collisions.SlopeAngleOld) < ANGLE_EPSILON)
                slopeAngle = Collisions.SlopeAngleOld;

            float moveDistance = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (!(velocity.y <= climbVelocityY)) return;

            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

            Collisions.Below = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
            Collisions.SlopeAngleOld = slopeAngle;
        }

        private RaycastHit2D CastAndRecord(Vector2 origin, Vector2 direction, float length)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, collisionMask);
            _debugRays.Add(new DebugRay(origin, direction, length, hit));
            return hit;
        }

        void UpdateRaycastOrigins()
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.Expand(skinWidth * -2f);

            _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            _raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        struct RaycastOrigins
        {
            public Vector2 TopLeft, TopRight;
            public Vector2 BottomLeft, BottomRight;
        }

        public struct CollisionInfo
        {
            public bool Above, Below;
            public bool Left, Right;

            public bool ClimbingSlope;
            public bool DescendingSlope;

            public float SlopeAngle, SlopeAngleOld;

            public Vector2 VelocityOld;

            public bool WasGroundedLastFrame;

            public void Reset(bool wasGroundedLastFrame)
            {
                WasGroundedLastFrame = wasGroundedLastFrame;

                Above = Below = false;
                Left = Right = false;

                ClimbingSlope = false;
                DescendingSlope = false;

                SlopeAngleOld = SlopeAngle;
                SlopeAngle = 0;
            }
        }

        private struct DebugRay
        {
            public Vector2 origin;
            public Vector2 direction;
            public float length;
            public RaycastHit2D hit;

            public DebugRay(Vector2 origin, Vector2 direction, float length, RaycastHit2D hit)
            {
                this.origin = origin;
                this.direction = direction;
                this.length = length;
                this.hit = hit;
            }
        }

        private void OnDrawGizmos()
        {
            if (_debugRays == null)
                return;

            foreach (DebugRay r in _debugRays)
            {
                Gizmos.color = r.hit ? Color.red : Color.green;
                Gizmos.DrawLine(r.origin, r.origin + r.direction.normalized * r.length);

                if (r.hit)
                {
                    Gizmos.DrawSphere(r.hit.point, 0.03f);
                }
            }
        }
        
        private bool TryGetSlopeAngle(float directionX, out float slopeAngle, out RaycastHit2D slopeHit)
        {
            slopeAngle = 0f;
            slopeHit = new RaycastHit2D();

            Vector2 origin = directionX > 0 
                ? _raycastOrigins.BottomRight 
                : _raycastOrigins.BottomLeft;

            slopeHit = Physics2D.Raycast(origin, Vector2.down, 0.5f, collisionMask);

            if (!slopeHit)
                return false;

            slopeAngle = Mathf.Acos(Mathf.Clamp(slopeHit.normal.y, -1f, 1f)) * Mathf.Rad2Deg;
            return true;
        }
        
        private static float SlopeAngleFromNormal(Vector2 normal)
        {
            // Celeste-style: angle between surface and "flat ground" via normal.y
            float ny = Mathf.Clamp(normal.y, -1f, 1f);
            return Mathf.Acos(ny) * Mathf.Rad2Deg;
        }

        private string AngleStr(float angle) => $"{angle:F2}°";
    }
}