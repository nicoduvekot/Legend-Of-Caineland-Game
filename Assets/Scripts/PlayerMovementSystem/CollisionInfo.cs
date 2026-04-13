using UnityEngine;

namespace PlayerMovementSystem
{
    public struct CollisionInfo
    {
        public bool Above;
        public bool Below;
        public bool Left;
        public bool Right;

        public Vector2 Displacement;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            Displacement = Vector2.zero;
        } 
    }
}