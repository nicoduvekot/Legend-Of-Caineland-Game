using UnityEngine;

/** This is a trigger zone that changes the music to the boss music when the player enters it.
 * It should be placed in the scene where the boss fight begins, and should have a BoxCollider2D with IsTrigger enabled.
 *
 */
public class BossMusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip bossMusic;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        _triggered = true;

        if (LocalAudioSource.Instance != null)
        {
            LocalAudioSource.Instance.ChangeMusic(bossMusic);
        }
    }
}