using UnityEngine;

public class BottleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private int damage = 1;
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

    private float _spawnTimer;
    private bool _isActive = false;

    private void Start()
    {
        ResetSpawnTimer();

    }

    private void Update()
    {
        if (!_isActive) return;

        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnBottle();
            ResetSpawnTimer();
        }

    }

    public void ActivateSpawner()
    {
        _isActive = true;
        ResetSpawnTimer();
        Debug.Log("Bottle Spawner Activated!");
    }

    private void SpawnBottle()
    {
        GameObject bottleObj = Instantiate(bottlePrefab, transform.position, Quaternion.identity);

        FlyingBottle bottle = bottleObj.GetComponent<FlyingBottle>();

        Vector2 direction = new Vector2(Random.Range(minXDirection, maxXDirection), Random.Range(minYDirection, maxYDirection)).normalized;
        
        float speed = Random.Range(bottleMinSpeed, bottleMaxSpeed);
        bottle.Initialize(direction, speed, damage);
    }

    private void ResetSpawnTimer()
    {
        _spawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
    }


}
