using GameState.Core;
using UnityEngine;

/*
 * This script defines the behavior of a flying bottle hazard in the game. 
 * It moves in a specified direction at a certain speed, applies damage to the player upon collision, and destroys itself after a set lifetime.
 */
public class FlyingBottle : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector2 _direction;
    private float _speed;
    private int _damage;

    // Initialize is a public method that allows external scripts (like the BottleSpawner) to set the direction, speed, and damage of the flying bottle when it is spawned
    public void Initialize(Vector2 direction, float speed, int damage) { 
    
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
    }

    //Update is called once per frame and moves the bottle in its set direction at its set speed
    private void Update()
    {
        transform.position += (Vector3)_direction * (_speed * Time.deltaTime);

    }

    // Start is called when the bottle is first created and sets a timer to destroy the bottle after its lifetime expires
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // OnTriggerEnter2D is called when the bottle collides with another collider. If it collides with the player, it applies damage to the player and destroys itself
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player")) return;

        if(!GameStateManager.HasInstance || GameStateManager.Instance.Data == null) return;
    
            GameStateManager.Instance.TakeDamage(_damage);
    
            Debug.Log($"Player Touched Bottle! Health is now {GameStateManager.Instance.Data.PlayerHealth}");
            Destroy(gameObject);
    }
}
