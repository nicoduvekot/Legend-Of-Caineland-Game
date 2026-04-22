using UnityEngine;
using GameState.Core;
using GameState;

/**
 * EnemyShooterProjectile is a projectile that moves in a straight line and damages the player on contact.
 * It is destroyed on contact with any non-trigger collider or after a certain lifetime.
 * 
 * This is a simpler version of EnemyProjectile that does not use Rigidbody2D for movement, but instead moves the transform directly.
 * This allows for more precise control over the projectile's movement and can be useful for certain types of projectiles.
 * 
 */

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyShooterProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private int damage = 1;

    private Vector2 direction;
    private float speed;

    // Initialize is called by the spawner to set the direction and speed of the projectile
    public void Initialize(Vector2 dir, float moveSpeed)
    {
        direction = dir.normalized;
        speed = moveSpeed;

        Destroy(gameObject, lifeTime);
    }

    // Always moves in the set direction at the set speed
    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    //handle trigger collisions with the player and other objects
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the projectile hit the player, if so, apply damage and check for player death
        if (other.CompareTag("Player"))
        {
            GameStateManager.Instance.TakeDamage(damage);

            if (GameStateManager.Instance.Data != null && GameStateManager.Instance.Data.PlayerHealth <= 0)
            {
                {
                    GameFlowManager.Instance.OnPlayerDeath();
                }

                Destroy(gameObject);
                return;
            }

            // Will want for now to pass through ground for now
            /*if (other.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }*/
        }
    }
}