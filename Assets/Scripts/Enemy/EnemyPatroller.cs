using Unity.Hierarchy;
using UnityEditor.Tilemaps;
using UnityEngine;
using GameState;
using GameState.Core;

/*
 * EnemyPatroller is a simple enemy that patrols between two points and shoots at the player when they are in range.
 * 
 */

public class EnemyPatroller : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float pointReachedThreshold = 0.1f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shotCooldown = 1f;
    [SerializeField] private float projectileSpeed = 8f;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private float lastShotTime;
    private bool facingRight = true;


    private void Awake()
    { 
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentTarget = pointB;

        UpdateDirection();

    }

    // FixedUpdate is used for consistent movement regardless of frame rate
    private void FixedUpdate()
    {
        Patrol();

    }

    private void Update()
    {
        ShootPlayer();

    }

    // Handles patrolling between point A and point B
    private void Patrol() 
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentTarget.position;

        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);

        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
        if(distanceToTarget <= pointReachedThreshold)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
            UpdateDirection();
        }
    }

    // Updates the facing direction of the enemy based on the current target
    private void UpdateDirection() 
    {
        if (currentTarget == null) return;

        bool shouldFaceRight = currentTarget.position.x > transform.position.x;
        if (shouldFaceRight != facingRight) {
            Flip();
        
        }
    }
    // Flips the enemy's facing direction by inverting the local scale
    private void Flip() 
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    // Handles shooting at the player if they are within detection range and in the correct direction
    private void ShootPlayer() 
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, detectionRange, playerLayer);
        Debug.DrawRay(firePoint.position, direction * detectionRange, Color.red);

        if (hit.collider == null) return;

        if (Time.time < lastShotTime + shotCooldown) return;

        Shoot(direction);
        lastShotTime = Time.time;
    }
    // Instantiates a projectile and initializes its movement in the given direction
    private void Shoot(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
        
        if (projectileScript != null)
        {
            projectileScript.Initialize(direction, projectileSpeed);
        }
        else
        {  
            Debug.LogWarning("Projectile prefab does not have an EnemyProjectile script attached.");
        }
    }



}
