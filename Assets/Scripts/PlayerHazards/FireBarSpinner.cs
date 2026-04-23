using UnityEngine;

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
