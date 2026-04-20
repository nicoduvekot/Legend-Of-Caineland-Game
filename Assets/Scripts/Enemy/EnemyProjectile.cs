using GameState;
using GameState.Core;
using UnityEngine;

/**
 * EnemyProjectile is a projectile that moves in a straight line and damages the player on contact.
 * It is destroyed on contact with any non-trigger collider or after a certain lifetime.
 * 
 */

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float moveSpeed;

    // Initialize is called by the spawner to set the direction and speed of the projectile
    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // FixedUpdate is used for consistent movement regardless of frame rate
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    // Handle collisions with the player and other objects
    private void OnTriggerEnter2D(Collider2D other) 
    {
        // Check if the projectile hit the player, if so, apply damage and check for player death
        if (other.CompareTag("Player"))
        {
            GameStateManager.Instance.TakeDamage(damage);

            if(GameStateManager.Instance.Data != null && GameStateManager.Instance.Data.PlayerHealth <= 0) 
            {
                GameFlowManager.Instance.OnPlayerDeath();
            }
            Destroy(gameObject);
            return;
        }
        // If the projectile hits any non-trigger collider, destroy it
        if ((!other.isTrigger))
        {
            Destroy(gameObject);

        }
    }

}
