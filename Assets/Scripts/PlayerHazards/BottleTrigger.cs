using UnityEngine;

public class BottleTrigger : MonoBehaviour
{
    [SerializeField] private BottleSpawner bottleSpawner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bottleSpawner.ActivateSpawner();
    }
}
