using UnityEngine;

/*
 * This script is responsible for destroying the rock game object when it collides with the ground. 
 * It uses a LayerMask to identify the ground layer and player tag to ensure the rock is destroyed.
 */

public class RockDestroy : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject, 0.05f);
        }
    
        if (collision.gameObject.CompareTag("Player")) 
        { 
            Destroy(gameObject, 0.05f);
        }

    }
}
