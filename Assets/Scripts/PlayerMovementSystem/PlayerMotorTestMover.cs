using UnityEngine;

namespace PlayerMovementSystem
{
    /// <summary>
    /// TEMP LAYER:
    ///
    /// This layer applies the movement from the motor
    /// </summary>
    public class PlayerMotorTestMover : MonoBehaviour
    {
        [SerializeField] private PlayerMovementMotor motor;

        private void Update()
        {
            // Apply horizontal velocity directly to transform
            transform.position += (Vector3)(motor.Velocity * Time.deltaTime);
        }
    }
}