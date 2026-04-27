using GameState;
using UnityEngine;

namespace TransitionAndResults
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Transition : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameFlowManager.Instance.CompleteLevel();
        }

        private void OnValidate()
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null && !box.isTrigger)
                box.isTrigger = true;
        }
    }
}