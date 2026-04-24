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
        
        public void FreezeInput() => _motor.LockInput();
        public void UnfreezeInput() => _motor.UnlockInput();
        
        public void PlayDeathAnimation()
        {
            _motor.PlayDeathAnimation();
        }
    }
}