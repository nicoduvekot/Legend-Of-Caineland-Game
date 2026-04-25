using System.Collections;
using UnityEngine;
using GameState;
using GameState.Core;

public class KingBossController : MonoBehaviour, IEnemy
{
    // ------------------- ENUMS -----------------------

    private enum BossState
    {
        Idle,
        Moving,
        Shooting,
        Slamming,
        Slashing,
        Cooldown,
        Dead
    }

    // -------------------- SERIALIZED FIELDS -----------------------

    [Header("References")]
    [SerializeField] private Transform firePoint; // Where the projectile spawns
    [SerializeField] private GameObject projectilePrefab; // The projectile prefab

    [Header("Animations")]
    [SerializeField] private Animator Attack;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f; // Range at which the boss begins enacting attacks

    [Header("Health")]
    [SerializeField] private int maxHealth = 10; // Boss Max Health set to 10
    private int currentHealth; // current health of boss, initialized at start

    [Header("Shooting")]
    [SerializeField] private float shootCooldown = 2f; // Time between shots, dont want it too fast or too slow, will adjust once we have the different phases
    [SerializeField] private float projectileSpeed = 8f; // Speed of projectile, same thing with the cooldown ^^

    [Header("Slam Attack")]
    [SerializeField] private float slamRadius = 2.5f; // The radius of the slam attack
    [SerializeField] private int slamDamage = 1; // Damage dealt by slam
    [SerializeField] private float slamCooldown = 3f; // Cooldown for slam until next attack is chosen

    [Header("Slam Warning Visuals")]
    [SerializeField] private SpriteRenderer bossSpriteRenderer;
    [SerializeField] private GameObject slamWarningCircle;
    [SerializeField] private Color ChargeColor = Color.red;
    [SerializeField] private Color NormalColor = Color.white;

    [Header("Melee Slash Attack")]
    [SerializeField] private GameObject bossSlashHitbox;
    [SerializeField] private float slashWindup = 0.4f;
    [SerializeField] private float slashActiveTime = 0.15f;
    [SerializeField] private float slashCooldown = 2f;

    [Header("Movement Jumping")]
    [SerializeField] private Transform[] jumpPoints;
    [SerializeField] private float jumpDuration = 0.1f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpCooldown = 5f;

    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = 1; // Damage dealt by touching the boss
    [SerializeField] private float contactCooldown = 2f; // Cooldown for contact damage to prevent rapid damage when player is touching the boss

    [Header("Boss Health Bar")]
    [SerializeField] private BossHealthBar healthBar; 
    // -------------------- PRIVATE FIELDS -----------------------

    private float _lastContactTime; // Tracks last time contact damage was applied to prevent spamming
    private Transform _player; // Reference to player transform
    private BossState _state = BossState.Idle; // Initial state of the boss is idle, will transition to other states based on player proximity and attack choices

    // -------------------- UNITY LIFECYCLES -----------------------

    // Start is called and sets health, finds player transform
    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.UpdateHealth(currentHealth, maxHealth);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    // Update calls state of boss, checks player distance, and calls attack routines based on distance and cooldown
    private void Update()
    {
        // If boss is dead or player is not found, do nothing
        if (_state == BossState.Dead || _player == null)
            return;

        // Calculate distance to player
        float distance = Vector2.Distance(transform.position, _player.position);

        // If player is out of detection range, do nothing (boss will idle and not attack)
        if (distance > detectionRange)
            return;

        // If player is within detection range and boss is idle, choose an attack to perform
        if (_state == BossState.Idle)
        {
            ChooseAttack(); // Randomizes between shooting and slam attack for now (Can add more later)
        }

        // Make the boss face the player (Literally turns boss to look at player)
        FacePlayer();
    }

    // -------------------- ATTACK LOGIC -----------------------

    // Chooses a random attack to perform when player in range
    private void ChooseAttack()
    {
        float choice = Random.value;

        if (choice < 0.25f)
            StartCoroutine(ShootRoutine());
        else if (choice < 0.5f)
            StartCoroutine(SlamRoutine());
        else if (choice < 0.75f)
            StartCoroutine(MoveToPlatformRoutine());
        else
            StartCoroutine(SlashRoutine());
    }

    // Shoots projectile towards player, then goes on cooldown before next attack
    private IEnumerator ShootRoutine()
    {
        // sets enum state to shooting
        _state = BossState.Shooting;

        // 4 shots per shooting attack
        for (int i = 0; i < 4; i++)
        {
            if (firePoint != null && projectilePrefab != null)
            {
                // Calculate direction from fire point to player and normalize it
                Vector2 direction = (_player.position - firePoint.position).normalized;

                //Instantiate the projectile at the fire point position
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

                // Get the projectile script component to initialize its movement
                EnemyShooterProjectile projectile = proj.GetComponent<EnemyShooterProjectile>();

                if (projectile != null)
                {
                    // Moves projectile towards player at set speed, direction to player
                    projectile.Initialize(direction, projectileSpeed);
                }
            }
            yield return new WaitForSeconds(0.5f); // delay per shot
        }

        yield return new WaitForSeconds(shootCooldown); // cooldown
        _state = BossState.Idle; // Set to Idle so boss can choose next attack
    }

    // Slam attack damages player if within radius, small wind-up time before slam hits, then goes on cooldown before next attack
    private IEnumerator SlamRoutine()
    {
        _state = BossState.Slamming; // sets enum state to slamming

        SetBossChargeColor(true);
        SetSlamCircle(false);

        Debug.Log("Boss is preparing slam!");

        yield return new WaitForSeconds(3.0f); // Small wind-up time

        SetBossChargeColor(false);
        SetSlamCircle(true);

        DoSlamDamage();
        Debug.Log("Boss slam activated!");

        yield return new WaitForSeconds(slamCooldown); // waits for cooldown, then allows next attack
        _state = BossState.Idle; // Resets state to idle so that boss can choose next attack when player is in range

        SetSlamCircle(false);
    }

    // Melee Attack for boss, short windup time, activates hitbox for a short time, then goes to cooldown
    private IEnumerator SlashRoutine()
    {
        _state = BossState.Slashing;
   

        yield return new WaitForSeconds(slashWindup);

        Attack.SetTrigger("Slash");

        if (bossSlashHitbox != null)
        {
            bossSlashHitbox.SetActive(true);
            Debug.Log("Hitbox exists, enabling it");
        }


        yield return new WaitForSeconds(slashActiveTime);

        if (bossSlashHitbox != null)
            bossSlashHitbox.SetActive(false);

        yield return new WaitForSeconds(slashCooldown);

        _state = BossState.Idle;
    }

    // Moves boss to random platform with jump arc, cools down after landing
    private IEnumerator MoveToPlatformRoutine()
    {
        _state = BossState.Moving;

        // checks if there are any jump points set, if not, just goes back to idle state
        if (jumpPoints == null || jumpPoints.Length == 0)
        {
            _state = BossState.Idle;
            yield break;
        }

        // target is a randomly selected jump point
        Transform target = jumpPoints[Random.Range(0, jumpPoints.Length)];

        // Starting position is current position of boss, end position is position of target jump point
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;

        float time = 0f;

        // while jump duration has not been reached, move boss towards target with an arc to simulate jumping
        while (time < jumpDuration)
        {
            time += Time.deltaTime;
            float t = time / jumpDuration;

            // Horizontal movement
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            // Arc (jump effect) had to look this one up
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            pos.y += arc;

            transform.position = pos;

            yield return null;
        }

        // Ensures boss is exactly at target position at end of jump
        transform.position = endPos;

        // Waits for jump cooldown before allowing next action
        yield return new WaitForSeconds(jumpCooldown);

        _state = BossState.Idle;
    }

    // --------------------- DAMAGE LOGIC -----------------------

    // Checks for player within slam radius and applies damage if hit
    private void DoSlamDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRadius);

        foreach (Collider2D hit in hits)
        {
            Debug.Log("Slam detected: " + hit.name);

            if (hit.CompareTag("Player"))
            {
                Debug.Log("Slam hit player!");
                DamagePlayer(slamDamage);
                return;
            }
        }
    }
    // Damages player through game state manager, checks for player death and calls game flow manager if player dies
    private void DamagePlayer(int amount)
    {
        GameStateManager.Instance.TakeDamage(amount);

        if (GameStateManager.Instance.Data != null &&
            GameStateManager.Instance.Data.PlayerHealth <= 0)
        {
            GameFlowManager.Instance.OnPlayerDeath();
        }
    }

    // Damages boss through player attacks, checks for boss death and handles death logic
    public void TakeDamage(int amount)
    {
        if (_state == BossState.Dead)
            return;

        currentHealth -= amount;
        Debug.Log("Boss took damage, current health: " + currentHealth);
        healthBar.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ---------------------- BOSS STATE / UTILITIES -----------------------

    // Handles boss death, sets state to dead, stops all ongoing attacks, destroys game object
    private void Die()
    {
        _state = BossState.Dead;
        StopAllCoroutines();
        Destroy(gameObject);
    }

    // Just a method so boss can always face player
    private void FacePlayer()
    {
        if (_player == null)
            return;

        Vector3 scale = transform.localScale;

        if (_player.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    // ----------------------- VISUALS / EFFECTS -----------------------

    // Sets boss color to red when charging slam attack
    private void SetBossChargeColor(bool isCharging)
    {
        if (bossSpriteRenderer != null)
        {
            bossSpriteRenderer.color = isCharging ? ChargeColor : NormalColor;
        }
    }

    // Sets the warning circle 
    private void SetSlamCircle(bool active)
    {
        if (slamWarningCircle != null)
        {
            slamWarningCircle.SetActive(active);
        }
    }

    // ---------------------- GIZMOS -----------------------

    // Gizmo to visualize detection range and slam radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}