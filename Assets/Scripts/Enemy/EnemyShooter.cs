using UnityEngine;

/** EnemyShooter.cs
 * 
 * This script allows an enemy to shoot projectiles at the player when they are within a certain range.
 * The enemy will have a cooldown between shots and can be set up with a projectile prefab and fire point.
 * 
 * Future improvements:
 * - Implementing enemy health and damage from player attacks
 * 
 */
public class EnemyShooter : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shooting")]
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float detectionRange = 10f;

    // This will be used for later once we implement a player attack 
    [Header("Enemy Health")]
    [SerializeField] private int maxHealth = 3;

    private Transform player;
    private float fireTimer;
    private int currentHealth;

    // Sets enemy with health and finds player
    private void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    // Checks if player is in range and shoots at them
    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRange)
            return;

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireCooldown)
        {
            ShootAtPlayer();
            fireTimer = 0f;
        }
    }

    // Instantiates a projectile and shoots it towards the player if in range
    private void ShootAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        EnemyShooterProjectile projectile = proj.GetComponent<EnemyShooterProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileSpeed);
        }
    }

    /* 
     possible code for later when we implement player attacks

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    */
}