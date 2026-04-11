using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Parallax Settings")]
    [SerializeField] private float parallaxEffect = 0.5f;
    [SerializeField] private bool lockY = true;

    private Vector3 startPosition;
    private float lastPlayerX;

    // Start retrieves initial position for later calculations
    private void Start()
    {
        startPosition = transform.position;

        if (player != null)
        {
            lastPlayerX = player.position.x;
        }
    }

    private void LateUpdate()
    {
        // simple check to prevent errors
        if (player == null) return;

        float playerDeltax = player.position.x - lastPlayerX;

        // Moves the background slightly based on the player movement
        transform.position += new Vector3(playerDeltax * parallaxEffect, 0, 0);

        // Locks the Y position to prevent any vertical movement
        if (lockY)
        {
            transform.position = new Vector3(transform.position.x, startPosition.y, startPosition.z);
        }
        lastPlayerX = player.position.x;
    }

}
