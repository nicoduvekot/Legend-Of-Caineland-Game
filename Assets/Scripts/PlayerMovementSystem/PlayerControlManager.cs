using Utilities;

namespace PlayerMovementSystem
{
    public class PlayerControlManager : PersistentSingleton<PlayerControlManager>
    {
        private PlayerMovementMotor _motor;
        
        public void RegisterMotor(PlayerMovementMotor motor)
        {
            _motor = motor;
        }
        
        public void Freeze() => _motor.enabled = false;
        public void Unfreeze() => _motor.enabled = true;
    }
}