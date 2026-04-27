using UnityEngine;

/*
 * Lightweight local audio source register for scenes that don't need persistence.
 * Attach this instead of AudioSync to AudioSource GameObjects in Level_01 through Level_04.
 * The VolumeSlider will find this via LocalAudioSource.Instance when AudioSync.Instance is null.
 *
 * Made by: Yoko Parks
 * Last Modified: 04/13/26
 */
public class LocalAudioSource : MonoBehaviour
{
    public static LocalAudioSource Instance { get; private set; }
    public AudioSource Source { get; private set; }

    void Awake()
    {
        Source = GetComponent<AudioSource>();
        Instance = this;
    }

    void OnDestroy()
    {
        // Clear instance when this scene unloads so it doesn't linger
        if (Instance == this) Instance = null;
    }

    // This is called by the boss trigger zone to change the music when the boss fight begins
    public void ChangeMusic(AudioClip newClip, bool loop = true)
    {
        if (Source == null || newClip == null)
            return;

        Source.Stop();
        Source.clip = newClip;
        Source.loop = loop;
        Source.Play();
    }
}