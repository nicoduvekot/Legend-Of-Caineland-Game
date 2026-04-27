using GameState; 
using UnityEngine;

namespace PlayerRespawnSystem
{
    /// <summary>
    /// This is to live on a checkpoint GameObject. It is required to have a BoxCollider2D, with IsTrigger enabled.
    ///
    /// When player collider triggers this GameObject's collider, it will set the current Checkpoint
    /// in GameData to this checkpointIndex, and tell the PlayerRespawnManager this is the respawn point.
    ///
    /// The idea is that the "start" of a level should be "0" and increments from there.
    ///
    /// When loading, it should read what is the current level, what is the current checkpoint index,
    /// and spawn the player object there.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Index")]
        [SerializeField] private int checkpointIndex;

        [Header("Checkpoint Visual")]
        [SerializeField] private ParticleSystem checkpointParticle;
        private bool _hasBeenReached;
        
        public int CheckpointIndex => checkpointIndex;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (_hasBeenReached)
                return;
            
            _hasBeenReached = true;
            
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            Vector3 spawnPos = new Vector3(box.bounds.center.x, box.bounds.min.y, transform.position.z);

            if (checkpointParticle != null)
            { 
                Instantiate(checkpointParticle, spawnPos, Quaternion.identity);
            }

            GameFlowManager.Instance.OnCheckpointReached(checkpointIndex, this);
        }
        
        public void ForceMarkAsReached()
        {
            _hasBeenReached = true;
        }

        /// <summary>
        /// This validates that the checkpoint's BoxCollider IsTrigger is set to true
        /// Also Logs an Error if two or more checkpoints in this scene share the same checkpoint index value
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isEditor || Application.isPlaying)
                return;
            
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null && !box.isTrigger)
                box.isTrigger = true;
            
            Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            foreach (Checkpoint checkpoint in allCheckpoints)
            {
                if (checkpoint == this)
                    continue;

                if (checkpoint.checkpointIndex == checkpointIndex)
                {
                    Debug.LogError($"Duplicate checkpoint index detected: {checkpointIndex} on '{name}' and '{checkpoint.name}'");
                }
            }
        }
    }
}