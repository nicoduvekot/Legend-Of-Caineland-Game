using UnityEngine;
/*
 * This script manages the spawning of flying bottles as a hazard for the player. 
 * It allows for randomization of spawn timing, bottle speed, and direction
 */
public class BottleSpawner : MonoBehaviour
{
    // Self-explanatory serialized fields for configuring the spawner in the Unity Editor
    [SerializeField] private GameObject bottlePrefab;
    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnDelay = 0.5f;
    [SerializeField] private float maxSpawnDelay = 1.5f;

    [Header("Bottle Settings")]
    [SerializeField] private float bottleMinSpeed = 2f;
    [SerializeField] private float bottleMaxSpeed = 5f;

    [Header("Direction Randomization")]
    [SerializeField] private float minXDirection = -1f;
    [SerializeField] private float maxXDirection = 1f;
    [SerializeField] private float minYDirection = -0.2f;
    [SerializeField] float maxYDirection = 2f;

    // Internal timers and state management for spawning, activiation, and deactivation
    private float _spawnTimer;
    private bool _isActive = false;

    // Initialize the spawn timer when the spawner is created
    private void Start()
    {
        ResetSpawnTimer();
    }

    // per frame update to manage spawning and activation state
    private void Update()
    {
        if (!_isActive) return;

        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f) {
            
            SpawnBottle();
            ResetSpawnTimer();
        }
    }

    // Method to activate the spawner, this is for when the player enters the hazard area
    public void ActivateSpawner()
    {
        _isActive = true;
        ResetSpawnTimer();
        Debug.Log("Bottle Spawner Activated!");
    }

    public void DeactivateSpawner()
    {
        _isActive = false; 
        Debug.Log("Bottle Spawner Deactivated!");
    }

    // Method to spawn a bottle with randomized direction and speed, will probably randomize rotation as well
    private void SpawnBottle()
    {
        GameObject bottleObj = Instantiate(bottlePrefab, transform.position, Quaternion.identity);

        FlyingBottle bottle = bottleObj.GetComponent<FlyingBottle>();

        Vector2 direction = new Vector2(Random.Range(minXDirection, maxXDirection), Random.Range(minYDirection, maxYDirection)).normalized;
        
        float speed = Random.Range(bottleMinSpeed, bottleMaxSpeed);
        bottle.Initialize(direction, speed);
    }

    // Helper method to reset the spawn timer to a new random value within the configured range
    private void ResetSpawnTimer()
    {
        _spawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
    }


}
