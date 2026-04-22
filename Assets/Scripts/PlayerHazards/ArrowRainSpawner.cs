using UnityEngine;

/* 
 * This script spawns arrows at regular intervals from the right side of the screen, with random vertical positions.
 * The arrows will move in a specified direction and speed, creating a "rain" effect.
 */
public class ArrowRainSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float spawnInterval = 1f;

    [Header("Spawn Area Settings")]
    [SerializeField] private float minY = 2f;
    [SerializeField] private float maxY = 6f;
    [SerializeField] private float spawnX = 12f;

    [Header("Arrow Direction")]
    [SerializeField] private Vector3 arrowDirection = new Vector2 (-1f, -0.5f);
    [SerializeField] private float arrowSpeed = 8f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnArrow();
            timer = 0f;
        }
    }

    private void SpawnArrow()
    {
        float spawnY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        ArrowHazard arrowScript = arrow.GetComponent<ArrowHazard>();

        if (arrowScript != null)
        {
            arrowScript.Initialize(arrowDirection, arrowSpeed);
        }
        else
        {
            Debug.LogError("Arrow prefab does not have an ArrowHazard script attached!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 topLeft = new Vector3(spawnX, maxY, 0f);
        Vector3 bottomLeft = new Vector3(spawnX, minY, 0f);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

}