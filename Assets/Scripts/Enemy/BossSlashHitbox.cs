using UnityEngine;
using GameState;
using GameState.Core;

public class BossSlashHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private bool hasHit = false;

    private void OnEnable()
    {
        hasHit = false; // reset each time hitbox turns on
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger ENTER with: " + other.name);

        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Trigger STAY with: " + other.name);

        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (hasHit) return;

        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player"))
            return;

        hasHit = true;

        Debug.Log($"BossSlashHitbox hit player, dealing {damage} damage.");
        GameStateManager.Instance.TakeDamage(damage);

        if (GameStateManager.Instance.Data != null &&
            GameStateManager.Instance.Data.PlayerHealth <= 0)
        {
            GameFlowManager.Instance.OnPlayerDeath();
        }
    }
}