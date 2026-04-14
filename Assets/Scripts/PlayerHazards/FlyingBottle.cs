using GameState.Core;
using UnityEngine;

public class FlyingBottle : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector2 _direction;
    private float _speed;
    private int _damage;

    public void Initialize(Vector2 direction, float speed, int damage) { 
    
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
    }

    private void Update()
    {
        transform.position += (Vector3)_direction * (_speed * Time.deltaTime);

    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player")) return;

        if(!GameStateManager.HasInstance || GameStateManager.Instance.Data == null) return;
    
            GameStateManager.Instance.TakeDamage(_damage);
    
            Debug.Log($"Player Touched Bottle! Health is now {GameStateManager.Instance.Data.PlayerHealth}");
            Destroy(gameObject);
    }
}
