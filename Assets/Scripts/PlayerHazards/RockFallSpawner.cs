using System.Collections;
using UnityEngine;

/*
 * This script is responsible for spawning rocks at specified intervals and locations when triggered by the RockFallTrigger script.
 * Using a coroutine to handle the spawning of rocks allows for better control over the timing and number of rocks spawned.
 */

public class RockFallSpawner : MonoBehaviour
{

    [Header("RockFall Settings")]
    [SerializeField] private GameObject rockFallPrefab;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int numberOfRocks = 10;
    [SerializeField] private Transform[] spawnPoints;

    private bool _isSpawning;

    // Flag to indicate whether the rock fall is currently spawning, prevents multiple simultaneous spawns
    public void BeginRockFalling()
    {

        if (_isSpawning) return;

        StartCoroutine(SpawnRocks());
    }

    // IEnumerator method to spawn rocks at specified points and intervals
    private IEnumerator SpawnRocks()
    {
        _isSpawning = true;

        for (int i = 0; i < numberOfRocks; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(rockFallPrefab, spawnPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }

        _isSpawning = false;
    }
}
