using UnityEngine;

namespace PlayerRespawnSystem
{
    /// <summary>
    /// DeathLine creates a really long box collider that when the player triggers
    /// it calls the PlayerRespawnManager to respawn the player.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DeathLine : MonoBehaviour
    {
        private const float Width = 9999f;
        private const float Height = 1f;

        /// <summary>
        /// If you click reset in inspector : this ensures the collider is re-sized
        /// </summary>
        private void Reset()
        {
            SetupCollider();
        }

        /// <summary>
        /// on validate event cycle ensures collider is proper sized
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isEditor || Application.isPlaying)
                return;

            SetupCollider();
        }

        /// <summary>
        /// Ensures Collider is proper sized
        /// </summary>
        private void SetupCollider()
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            
            box.size = new Vector2(Width, Height);
            box.offset = Vector2.zero;
        }

        /// <summary>
        /// This gets called when the collider is triggered
        /// If it is the player that triggers this, it calls RespawnPlayer operation
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;
            
            PlayerRespawnManager.Instance.RespawnPlayer();
        }
    }
}