using UnityEngine;

/** This script controls the behavior of an arrow hazard in the game. 
 * The arrow moves in a specified direction at a specified speed and is destroyed after a certain lifetime or upon collision with the player or other non-trigger colliders.
 * The arrow's rotation is set to match its movement direction for visual consistency.
 */

[RequireComponent(typeof(Collider2D))]
public class ArrowHazard : MonoBehaviour
{
    [SerializeField] private float lifetime = 6f;

    private Vector2 moveDirection;
    private float moveSpeed;

    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }
        if(!other.isTrigger)
        {
            Destroy(gameObject);
        }

       
    }
}