using UnityEngine;

/** * This script controls the rotation of a fire bar hazard in the game. 
 * The fire bar will continuously rotate around its center at a specified speed and direction
 */

public class FireBarSpinner : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool clockwise = true;

    private void Update()
    {
        float direction = clockwise ? -1f : 1f;
        transform.Rotate(0f, 0f, rotationSpeed * direction * Time.deltaTime);
    }
}
