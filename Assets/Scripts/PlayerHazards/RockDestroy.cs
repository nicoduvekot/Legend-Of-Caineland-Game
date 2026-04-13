using UnityEngine;

/*
 * This script is responsible for destroying the rock game object when it collides with the ground. 
 * It uses a LayerMask to identify the ground layer and checks for collisions in the OnCollisionEnter2D method. 
 * If the rock collides with an object on the ground layer, it will be destroyed.
 */

public class RockDestroy : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }
        //TODO: add destroy when it hits player 

    }
}
