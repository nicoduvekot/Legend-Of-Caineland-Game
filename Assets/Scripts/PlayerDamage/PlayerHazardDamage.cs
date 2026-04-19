using GameState;
using GameState.Core;
using PlayerRespawnSystem;
using UnityEngine;

/* This script serves to detect when the player has come into contact with a hazard and apply damage accordingly.
 * 
 * The debug log statements tell the console to echo back when the player has touched a hazard and what their health is after taking damage. 
 * This will tell if the code received input
 * For now when a player hits a hazard, its based off of collision detection, not sure if we want trigger or both
 * 
 */

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerHazardDamage : MonoBehaviour
{
    [Header("Hazard Detection]")]
    [SerializeField] private LayerMask hazard;

    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    /*   time in seconds for cooldown, 
     *   using this to prevent instant death when a player is on top of something
     *   This may or may not be needed depending on how we design the respawn system.
     */

    private BoxCollider2D _boxCollider;
    private float _coolDown;
    private bool _isDead = false;

    // This method initializes the BoxCollider2D component reference.
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    // This method checks for hazards every frame and applies damage if the player is currently overlapping with one.
    private void Update()
    {
        if(_coolDown > 0f) 
        {
            _coolDown -= Time.deltaTime;
            return;
        }

        DetectHazard();
        CheckDeath();
    }

    // This method checks if the player is currently overlapping with a hazard and applies damage if so.
    private void DetectHazard()
    {
        if (_coolDown > 0f) return;

        Bounds bounds = _boxCollider.bounds;

        Collider2D hit = Physics2D.OverlapBox(bounds.center, bounds.size, 0f, hazard);

        if (hit == null) return;

        if(!GameStateManager.HasInstance || GameStateManager.Instance.Data == null) return;

        GameStateManager.Instance.TakeDamage(damageAmount);
        _coolDown = damageCooldown;

        Debug.Log($"Player Touched Hazard! Health is now {GameStateManager.Instance.Data.PlayerHealth}");

    }

    // This is to visualize the detection box. Its more for debugging rn.
    private void OnDrawGizmos()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if(boxCollider == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
    }

    private void CheckDeath()
    {
        if (!GameStateManager.HasInstance || GameStateManager.Instance.Data == null)
            return;

        if (GameStateManager.Instance.Data.PlayerHealth <= 0 && !_isDead)
        {
            _isDead = true;

            
            GameFlowManager.Instance.OnPlayerDeath();
            GameStateManager.Instance.Data.PlayerHealth = 3; // This will need to change when we implement a current hearts variable
        }

        if (GameStateManager.Instance.Data.PlayerHealth > 0)
        {
            _isDead = false;
        }
    }


}
