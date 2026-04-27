using System.Collections.Generic;
using UnityEngine;

namespace PlayerMovementSystem
{
    /// <summary>
    /// This is the Collision Layer
    ///
    /// Responsible for taking the motor's weighted movement, and resolving if that movement can happen
    ///
    /// 1. Cast Rays in intended direction for length of that Velocity
    /// 2. If Hit, resolve what must occur
    /// 3. Apply adjustments to the velocity value
    /// 4. Return the velocity value via CollisionInfo report
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerCollisionResolver : MonoBehaviour
    {
        [Header("Collision Settings")]
        public LayerMask collisionMask;
        //public float skinWidth = 0.02f; // I might bring this back?
        
        [Header("Horizontal Ray Settings")]
        public float horizontalRayVerticalOffset = 0.05f;
        
        [Header("Vertical Ray Settings")]
        public float verticalRayHorizontalOffset = 0.05f;

        private CollisionInfo _info;
        
        private BoxCollider2D _collider;
        private RaycastOrigins _raycastOrigins;
        private readonly List<DebugRay> _debugRays = new();

        //private const float Epsilon = 0.001f; // again. might bring this back?
        
        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }
        
        // Move is the core flow director for collision resolving
        public CollisionInfo Move(Vector2 displacement)
        {
            _debugRays.Clear();
            _info.Reset();
            UpdateColliderCorners();
            
            ResolveVerticalCollisions(ref displacement);
            ResolveHorizontalCollisions(ref displacement);
            
            _info.Displacement = displacement;
            return _info;
        }
        
        private void UpdateColliderCorners()
        {
            Vector2 size = _collider.size;
            Vector2 offset = _collider.offset;
            
            Vector2 scaledSize = new(
                size.x * transform.lossyScale.x,
                size.y * transform.lossyScale.y
            );
            
            Vector2 half = scaledSize * 0.5f;

            Vector2 localBL = offset + new Vector2(-half.x, -half.y);
            Vector2 localBR = offset + new Vector2(+half.x, -half.y);
            Vector2 localTL = offset + new Vector2(-half.x, +half.y);
            Vector2 localTR = offset + new Vector2(+half.x, +half.y);
            
            _raycastOrigins.BottomLeft  = transform.TransformPoint(localBL);
            _raycastOrigins.BottomRight = transform.TransformPoint(localBR);
            _raycastOrigins.TopLeft     = transform.TransformPoint(localTL);
            _raycastOrigins.TopRight    = transform.TransformPoint(localTR);
        }

        private void ResolveVerticalCollisions(ref Vector2 move)
        {
            float moveY = move.y;
            
            // always raycast down for ground check
            float downRayLength = Mathf.Max(Mathf.Abs(move.y), Mathf.Epsilon);
            RaycastHit2D[] downHits = CastDownRays(downRayLength);

            if (TryFindClosestHit(downHits, out RaycastHit2D closestDownHit))
            {
                float closestDownDistance = closestDownHit.distance;

                if (moveY <= 0)
                {
                    float clampedDownDistance = Mathf.Min(Mathf.Abs(move.y), closestDownDistance);
                    move.y = -clampedDownDistance;
                }
                _info.Below = true;
            }
            else
            {
                _info.Below = false;
            }
            
            // check vertical if motor has positive y movement
            if (moveY > 0)
            {
                float upRayLength = Mathf.Abs(move.y);
                RaycastHit2D[] upHits = CastUpRays(upRayLength);

                if (TryFindClosestHit(upHits, out RaycastHit2D closestUpHit))
                {
                    float closestUpDistance = closestUpHit.distance;
                    float clampedUpDistance = Mathf.Min(Mathf.Abs(move.y), closestUpDistance);
                    move.y = clampedUpDistance;
                    
                    _info.Above = true;
                }
                else
                {
                    _info.Above = false;
                }
            }
            else
            {
                _info.Above = false;
            }
        }
        
        private RaycastHit2D[] CastDownRays(float rayLength)
        {
            RaycastHit2D[] hits = new RaycastHit2D[5];
            
            Vector2 bl = _raycastOrigins.BottomLeft;
            Vector2 br = _raycastOrigins.BottomRight;
            
            bl.x += verticalRayHorizontalOffset;
            br.x -= verticalRayHorizontalOffset;
            
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Vector2 origin = Vector2.Lerp(bl, br, t);
                Vector2 direction = Vector2.down;

                RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, collisionMask);
                hits[i] = hit;
                
                _debugRays.Add(new DebugRay(origin, direction, rayLength, hit));
            }
            return hits;
        }

        private RaycastHit2D[] CastUpRays(float rayLength)
        {
            RaycastHit2D[] hits = new RaycastHit2D[5];
            
            Vector2 tl = _raycastOrigins.TopLeft;
            Vector2 tr = _raycastOrigins.TopRight;
            
            tl.x += verticalRayHorizontalOffset;
            tr.x -= verticalRayHorizontalOffset;

            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Vector2 origin = Vector2.Lerp(tl, tr, t);
                Vector2 direction = Vector2.up;
                
                RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, collisionMask);
                hits[i] = hit;
                
                _debugRays.Add(new DebugRay(origin, direction, rayLength, hit));
            }
            return hits;
        }

        private void ResolveHorizontalCollisions(ref Vector2 move)
        {
            if (Mathf.Abs(move.x) < Mathf.Epsilon)
                return;

            float rayLength = Mathf.Abs(move.x);
            float directionSign = Mathf.Sign(move.x);
            
            RaycastHit2D[] hits = CastHorizontalRays(rayLength, directionSign);
            
            if (!TryFindClosestHit(hits, out RaycastHit2D closestHit))
                return;
            
            float closestDistance = closestHit.distance;
            
            float clamped = Mathf.Min(Mathf.Abs(move.x), closestDistance);
            move.x = clamped * directionSign;
            
            if (directionSign < 0f)
                _info.Left = true;
            else
                _info.Right = true;
        }
        
        private RaycastHit2D[] CastHorizontalRays(float rayLength, float directionSign)
        {
            RaycastHit2D[] hits = new RaycastHit2D[5];
            
            Vector2 direction = directionSign < 0 ? Vector2.left : Vector2.right;
            Vector2 bottom = directionSign < 0 ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;
            Vector2 top = directionSign < 0 ? _raycastOrigins.TopLeft : _raycastOrigins.TopRight;
            
            bottom += Vector2.up * horizontalRayVerticalOffset;
            
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Vector2 origin = Vector2.Lerp(bottom, top, t);

                RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, collisionMask);
                hits[i] = hit;

                _debugRays.Add(new DebugRay(origin, direction, rayLength, hit));
            }
            
            return hits;
        }

        private static bool TryFindClosestHit(RaycastHit2D[] hits, out RaycastHit2D closestHit)
        {
            bool hasHit = false;
            float closestDist = float.MaxValue;
            closestHit = default;

            foreach (RaycastHit2D hit in hits)
            {
                if (!hit) continue;
                if (!(hit.distance < closestDist)) continue;
                
                closestDist = hit.distance;
                closestHit = hit;
                hasHit = true;
            }
            return hasHit;
        }

        private struct RaycastOrigins
        {
            public Vector2 TopLeft, TopRight;
            public Vector2 BottomLeft, BottomRight;
        }
        
        private struct DebugRay
        {
            public readonly Vector2 Origin;
            public readonly Vector2 Direction;
            public readonly float Length;
            public readonly RaycastHit2D Hit;

            public DebugRay(Vector2 origin, Vector2 direction, float length, RaycastHit2D hit)
            {
                Origin = origin;
                Direction = direction;
                Length = length;
                Hit = hit;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (_debugRays == null)
                return;

            foreach (DebugRay r in _debugRays)
            {
                Gizmos.color = r.Hit ? Color.red : Color.green;
                Gizmos.DrawLine(r.Origin, r.Origin + r.Direction.normalized * r.Length);

                if (r.Hit)
                    Gizmos.DrawSphere(r.Hit.point, 0.03f);
            }
        }
    }
}