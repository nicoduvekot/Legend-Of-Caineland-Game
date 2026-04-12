using System.Collections.Generic;
using UnityEngine;

namespace PlayerMovementSystem
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerCollisionResolver : MonoBehaviour
    {
        [Header("Collision Settings")]
        public LayerMask collisionMask;
        public float skinWidth = 0.02f;
        
        private BoxCollider2D _collider;
        private RaycastOrigins _raycastOrigins;
        private readonly List<DebugRay> _debugRays = new();
        
        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }
        
// TODO : Intentionally void for the moment : will return a CollisionInfo reference
        // Move is the core flow director for collision resolving
        public void Move(Vector2 velocity)
        {
            UpdateRaycastOrigins();
        }
        
        private void UpdateRaycastOrigins()
        {
            Bounds bounds = _collider.bounds;
            bounds.Expand(skinWidth * -2);

            _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }
        
        private struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
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
                    Gizmos.DrawSphere(r.hit.point, 0.03f);
            }
        }
    }
}