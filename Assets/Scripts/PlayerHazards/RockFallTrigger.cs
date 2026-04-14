using UnityEngine;

public class RockFallTrigger : MonoBehaviour
{
    [SerializeField] private RockFallSpawner spawner;
    [SerializeField] private bool triggeredOnce = true;

    private bool _hasTriggered;


    // function to trigger the rock fall when the player enters the trigger area, is triggered only once 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggeredOnce && _hasTriggered)
        {
            Debug.Log("Already triggered.");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Entered object is not tagged Player.");
            return;
        }

        _hasTriggered = true;
        Debug.Log("Player entered trigger. Starting rockfall.");

        if (spawner != null)
        {
            spawner.BeginRockFalling();
        }
        else
        {
            Debug.LogWarning("RockFallTrigger has no spawner assigned.");
        }

    }
}
