using GameState;
using GameState.Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float moveSpeed;

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

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
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

        if ((!other.isTrigger))
        {
            Destroy(gameObject);

        }
    }

}
