using UnityEngine;

/** This script serves as a trigger for activating the BottleSpawner when the player enters its collider. 
 * It checks for collision with the player and calls the ActivateSpawner method on the BottleSpawner to start spawning flying bottles.
 */
public class BottleTrigger : MonoBehaviour
{
    [SerializeField] private BottleSpawner bottleSpawner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bottleSpawner.ActivateSpawner();
    }
}
