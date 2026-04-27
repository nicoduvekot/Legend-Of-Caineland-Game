using UnityEngine;
using UnityEngine.UI;

/*
 * This script controls the volume slider and passes the data over to sys memory to be written to AudioData.
 * Fetches the AudioSource from AudioSync.Instance at runtime so it always targets the
 * carried-over source rather than a local Inspector reference that may have been destroyed.
 * 
 * Made by: Yoko Parks
 * Modified: 04/13/26
 */
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private AudioData audioData;

    private AudioSource BackgroundMusic; // Resolved at runtime from AudioSync



    void Start() // Exec on pgrm start
    {
        if (musicSlider == null) { Debug.LogError("Music Slider is not assigned in the Inspector!"); return; }
        if (audioData == null) { Debug.LogError("AudioData asset is not assigned in the Inspector!"); return; }

        // Grab the surviving AudioSource from the carried-over AudioSync object
        if (AudioSync.Instance != null)
        {
            BackgroundMusic = AudioSync.Instance.Source;
        }
        else
        {
            Debug.LogError("AudioSync instance not found — is the AudioSync object present in Load_Save?");
            return;
        }

        // Load the saved state from AudioData asset
        musicSlider.value = audioData.masterVolume;
        BackgroundMusic.volume = audioData.masterVolume;

        // Listen for slider movement
        musicSlider.onValueChanged.AddListener(SetVolume);
    }



    // Sets volume of playing audio
    public void SetVolume(float value)
    {
        BackgroundMusic.volume = value;
        audioData.masterVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}