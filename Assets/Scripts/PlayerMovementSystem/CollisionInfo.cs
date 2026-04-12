using UnityEngine;

namespace PlayerMovementSystem
{
    public struct CollisionInfo
    {
        public bool Above;
        public bool Below;
        public bool Left;
        public bool Right;

        public Vector2 Velocity;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            Velocity = Vector2.zero;
        } 
    }
}