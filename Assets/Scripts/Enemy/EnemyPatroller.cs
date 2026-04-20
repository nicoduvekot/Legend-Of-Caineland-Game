using Unity.Hierarchy;
using UnityEditor.Tilemaps;
using UnityEngine;
using GameState;
using GameState.Core;

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

    private void FixedUpdate()
    {
        Patrol();

    }

    private void Update()
    {
        ShootPlayer();

    }

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

    private void UpdateDirection() 
    {
        if (currentTarget == null) return;

        bool shouldFaceRight = currentTarget.position.x > transform.position.x;
        if (shouldFaceRight != facingRight) {
            Flip();
        
        }
    }

    private void Flip() 
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

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
