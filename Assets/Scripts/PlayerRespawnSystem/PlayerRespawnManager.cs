using UnityEngine;
using Utilities;

namespace PlayerRespawnSystem
{
    public class PlayerRespawnManager : PersistentSingleton<PlayerRespawnManager>
    {
        private Vector3 _currentCheckpoint;
        private Transform _playerTransform;

        /// <summary>
        /// This is the core of this script we interact with.
        ///
        /// Upon death (0 life) or missing a jump and triggering some death line,
        /// this operation should be called.
        /// </summary>
        public void RespawnPlayer()
        {
            if (_playerTransform == null)
            {
                Debug.LogError("[Respawn Manager]: No player transform assigned for respawn");
                return;
            }
            _playerTransform.position = _currentCheckpoint;
        }
        
        /// <summary>
        /// This is auto-used by the player in PlayerMovementMotor script
        /// </summary>
        /// <param name="playerTransform"></param>
        public void RegisterPlayer(Transform playerTransform)
        {
            _playerTransform = playerTransform;
        }

        /// <summary>
        /// This is auto used by Checkpoints when they are triggered by the player
        /// </summary>
        /// <param name="checkpoint"></param>
        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null)
            {
                Debug.LogError("[Respawn Manager]: Null checkpoint passed to SetCheckpoint");
                return;
            }
            
            _currentCheckpoint = checkpoint.transform.position;
        }
    }
}