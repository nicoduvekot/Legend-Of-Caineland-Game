using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Collision")]
        public LayerMask collisionMask;
        
        [Header("Raycast Settings")]
        public int horizontalRayCount = 4;
        public int verticalRayCount = 6;
        public float skinWidth = 0.02f;
        public float maxSlopeAngle = 60f;
        
        private const float EPSILON = 0.0001f;
        private const float ANGLE_EPSILON = 0.5f;
        
        private float _horizontalRaySpacing;
        private float _verticalRaySpacing;
        
        private BoxCollider2D _boxCollider;
        private RaycastOrigins _raycastOrigins;
        
        public CollisionInfo Collisions;
        
        void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount   = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
            CalculateRaySpacing();
        }
        
        public void Move(Vector2 velocity)
        {
            UpdateRaycastOrigins();
            Collisions.Reset();
            Collisions.VelocityOld = velocity;
            
            if (Mathf.Abs(velocity.x) > EPSILON)
            {
                HorizontalCollisions(ref velocity);
            }
            
            if (Mathf.Abs(velocity.y) > EPSILON)
            {
                VerticalCollisions(ref velocity);
            }

            transform.Translate(velocity);
        }
        
        void HorizontalCollisions(ref Vector2 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            Vector2 footForward = (directionX < 0)
                ? _raycastOrigins.BottomLeft
                : _raycastOrigins.BottomRight;

            RaycastHit2D footHit = Physics2D.Raycast(
                footForward,
                Vector2.right * directionX,
                rayLength,
                collisionMask
            );
            
            Vector2 headForward = (directionX < 0f)
                ? _raycastOrigins.TopLeft
                : _raycastOrigins.TopRight;

            RaycastHit2D headHit = Physics2D.Raycast(
                headForward,
                Vector2.right * directionX,
                rayLength,
                collisionMask
            );
            
            Vector2 kneeForward = Vector2.Lerp(footForward, headForward, 0.25f);

            RaycastHit2D kneeHit = Physics2D.Raycast(
                kneeForward,
                Vector2.right * directionX,
                rayLength,
                collisionMask
            );
            
            // no collision in front free to move
            if (!footHit && !kneeHit && !headHit)
                return;
            
            if (footHit && !kneeHit && !headHit)
            {
                float slopeAngle = Vector2.Angle(footHit.normal, Vector2.up);

                if (slopeAngle <= maxSlopeAngle)
                {
                    //Debug.Log("STATE: ASCEND");

                    float distanceToSlopeStart = footHit.distance - skinWidth;

                    if (distanceToSlopeStart > 0f)
                        velocity.x -= distanceToSlopeStart * directionX;

                    ClimbSlope(ref velocity, slopeAngle);

                    velocity.x += distanceToSlopeStart * directionX;
                    return;
                }
                
                //Debug.Log("STATE: BLOCKED (too steep to climb)");
                velocity.x = (footHit.distance - skinWidth) * directionX;
                Collisions.Left = directionX < 0f;
                Collisions.Right = directionX > 0f;
                return;
            }
            
            //Debug.Log("STATE: BLOCKED");

            RaycastHit2D hit = footHit ? footHit : (kneeHit ? kneeHit : headHit);

            velocity.x = (hit.distance - skinWidth) * directionX;

            Collisions.Left = directionX < 0f;
            Collisions.Right = directionX > 0f;
        }
        
// TODO : REVISIT!!
        void VerticalCollisions(ref Vector2 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                float t = (verticalRayCount == 1) ? 0f : (float)i / (verticalRayCount - 1);
                
                Vector2 edgeLeft = (directionY < -EPSILON)
                    ? _raycastOrigins.BottomLeft
                    : _raycastOrigins.TopLeft;
                Vector2 edgeRight = (directionY < -EPSILON)
                    ? _raycastOrigins.BottomRight
                    : _raycastOrigins.TopRight;

                Vector2 rayOrigin = Vector2.Lerp(edgeLeft, edgeRight, t);

                RaycastHit2D hit = Physics2D.Raycast(
                    rayOrigin,
                    Vector2.up * directionY,
                    rayLength,
                    collisionMask
                );

                if (!hit) continue;
                
                if (hit.distance <= skinWidth + EPSILON)
                {
                    // Only zero out downward motion; let upward motion be handled normally.
                    if (directionY < -EPSILON && velocity.y < 0f)
                        velocity.y = 0f;

                    Collisions.Below = directionY < -EPSILON;
                    Collisions.Above = directionY > EPSILON;

                    // Don't shorten rayLength here; just move on to next ray.
                    continue;
                }
                
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                Collisions.Below = directionY < -EPSILON;
                Collisions.Above = directionY > EPSILON;
                
                // CORNER CORRECTION WHILE CLIMBING SLOPE
                if (Collisions.ClimbingSlope)
                {
                    float directionX = Mathf.Sign(velocity.x);
                    float rayLengthX = Mathf.Abs(velocity.x) + skinWidth;

                    Vector2 edgeBottom = (directionX < -EPSILON)
                        ? _raycastOrigins.BottomLeft
                        : _raycastOrigins.BottomRight;
                    Vector2 edgeTop = (directionX < -EPSILON)
                        ? _raycastOrigins.TopLeft
                        : _raycastOrigins.TopRight;

                    Vector2 rayOriginX = Vector2.Lerp(edgeBottom, edgeTop, t);

                    RaycastHit2D hit2 = Physics2D.Raycast(
                        rayOriginX,
                        Vector2.right * directionX,
                        rayLengthX,
                        collisionMask
                    );

                    if (hit2)
                    {
                        float slopeAngle = Vector2.Angle(hit2.normal, Vector2.up);
                        if (Mathf.Abs(slopeAngle - Collisions.SlopeAngle) > ANGLE_EPSILON)
                        {
                            velocity.x = (hit2.distance - skinWidth) * directionX;
                            Collisions.SlopeAngle = slopeAngle;
                        }
                    }
                }
            }
            
            // ADJUST FOR SLOPE CLIMBING
            if (Collisions.ClimbingSlope)
            {
                float directionX = Mathf.Sign(velocity.x);
                float rayLengthX = Mathf.Abs(velocity.x) + skinWidth;

                Vector2 rayOrigin = (directionX < -EPSILON)
                    ? _raycastOrigins.BottomLeft
                    : _raycastOrigins.BottomRight;

                RaycastHit2D hit = Physics2D.Raycast(
                    rayOrigin,
                    Vector2.right * directionX,
                    rayLengthX,
                    collisionMask
                );

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (Mathf.Abs(slopeAngle - Collisions.SlopeAngle) > ANGLE_EPSILON)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        Collisions.SlopeAngle = slopeAngle;
                    }
                }
            }
        }
        
        void ClimbSlope(ref Vector2 velocity, float slopeAngle)
        {
            Debug.Log("STATE: ClimbSlope() ENTERED");
            float moveDistance = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.y <= climbVelocityY)
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

                Collisions.Below = true;
                Collisions.ClimbingSlope = true;
                Collisions.SlopeAngle = slopeAngle;
            }
        }
        
// TODO : REVISIT!!
        void DescendSlope(ref Vector2 velocity)
        {
            // moving horizontally
            if (Mathf.Abs(velocity.x) < EPSILON)
                return;
            
            float directionX = Mathf.Sign(velocity.x);
            
            Vector2 rayOrigin = (directionX < -EPSILON)
                ? _raycastOrigins.BottomRight
                : _raycastOrigins.BottomLeft;

            const float maxDescendDistance = 0.5f;
            
            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                maxDescendDistance,
                collisionMask
            );

            if (!hit) return;

            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            
            if (slopeAngle < ANGLE_EPSILON) return;
            if (slopeAngle - maxSlopeAngle > ANGLE_EPSILON) return;

            Vector2 t1 = new(hit.normal.y, -hit.normal.x);
            Vector2 t2 = new(-hit.normal.y, hit.normal.x);
            Vector2 slopeDownDir = Vector2.Dot(t1, Vector2.down) > 0f ? t1 : t2;

            // Check if the slope descends in the direction of movement
            float alignment = Vector2.Dot(slopeDownDir.normalized, new Vector2(directionX, 0f));

            if (alignment <= EPSILON)
                return;
            
            float distanceToGround = hit.distance - skinWidth;
            if (distanceToGround > 0.1f)
                return;
            
            float moveDistance = Mathf.Abs(velocity.x);
            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            velocity.y -= descendVelocityY;

            Collisions.SlopeAngle = slopeAngle;
            Collisions.DescendingSlope = true;
            Collisions.Below = true;
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
        
        void CalculateRaySpacing()
        {
            Bounds bounds = _boxCollider.bounds;
            bounds.Expand(skinWidth * -2);
            
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
            
            _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
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

            public void Reset()
            {
                Above = Below = false;
                Left = Right = false;
                
                ClimbingSlope = false;
                DescendingSlope = false;
                
                SlopeAngleOld = SlopeAngle;
                SlopeAngle = 0;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (_boxCollider == null)
                _boxCollider = GetComponent<BoxCollider2D>();
            
            UpdateRaycastOrigins();

            DrawSideRays(-1);
            DrawSideRays(+1); 
            
// TODO : VERTICAL RAYS
        }
        
        private void DrawSideRays(int directionX)
        {
            const float rayLength = 0.35f;
            
            // Foot
            Vector2 foot = (directionX < 0)
                ? _raycastOrigins.BottomLeft
                : _raycastOrigins.BottomRight;

            // Head
            Vector2 head = (directionX < 0)
                ? _raycastOrigins.TopLeft
                : _raycastOrigins.TopRight;

            // Knee
            Vector2 knee = Vector2.Lerp(foot, head, 0.25f);
            
            // Foot Ray
            RaycastHit2D footHit = Physics2D.Raycast(
                foot,
                Vector2.right * directionX,
                rayLength,
                collisionMask
            );
            Gizmos.color = footHit ? Color.red : Color.green;
            Gizmos.DrawLine(foot, foot + Vector2.right * directionX * rayLength);

            // Knee Ray
            RaycastHit2D kneeHit = Physics2D.Raycast(
                knee,
                Vector2.right * directionX,
                rayLength,
                collisionMask
            );
            Gizmos.color = kneeHit ? Color.red : Color.green;
            Gizmos.DrawLine(knee, knee + Vector2.right * directionX * rayLength);

            // Head Ray
            RaycastHit2D headHit = Physics2D.Raycast(
                head,
                Vector2.right * directionX,
                rayLength,
                collisionMask
            );
            Gizmos.color = headHit ? Color.red : Color.green;
            Gizmos.DrawLine(head, head + Vector2.right * directionX * rayLength);
        }
    }
}