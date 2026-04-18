using UnityEngine;

namespace CoinManagement
{
    /// <summary>
    /// This represents a Coin collectible, it must have a Collider2D for player to trigger
    ///
    /// id represents a unique guid identifier for this particular GameObject
    /// Value represents the points player receives when collecting the coin 
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Coin : MonoBehaviour
    {
        [SerializeField] private CoinId id;
        [SerializeField] private int value = 1;

        public CoinId Id => id;
        public int Value => value;

        /// <summary>
        /// This is called by the CoinManager, when the collection operation has terminated
        /// </summary>
        public void Collect()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// This is called by the CoinManager, when the coin needs to be collectable again
        /// </summary>
        public void Respawn()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// This is called when the Player triggers the coin object
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            CoinManager.Instance.CollectCoin(this);
        }
        
        /// <summary>
        /// This is a Unity Editor Event Function ensuring the collider exists, and is a trigger
        /// It also ensures that if the id value should change, that unity records this change.
        ///
        /// This helps so duplicating a coin GameObject ensures the new id is properly updated with Unity
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isEditor || Application.isPlaying)
                return;
            
            if (string.IsNullOrEmpty(id.Value))
            {
                id = CoinId.New();
                // ensure Unity saves any update to this value
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            
            CircleCollider2D circle = GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                if (!circle.isTrigger)
                    circle.isTrigger = true;
            }
        }
    }
}