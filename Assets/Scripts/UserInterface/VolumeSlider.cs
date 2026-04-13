using UnityEngine;
using UnityEngine.UI;

/*
 * This script controls the volume slider and passes the data over to sys memory to be written to AudioData
 * 
 * Made by: Yoko Parks
 * Modified: 04/13/26
 */
public class VolumeSlider : MonoBehaviour
{
    //Passed from config and serialized
    [SerializeField] private AudioSource BackgroundMusic;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private AudioData audioData;

    void Start() //Exec on pgrm start
    {
        // TESTING DEBUGS
        if (musicSlider == null) { Debug.LogError("Music Slider is not assigned in the Inspector!"); return; }
        if (audioData == null) { Debug.LogError("AudioData asset is not assigned in the Inspector!"); return; }
        if (BackgroundMusic == null) { Debug.LogError("BackgroundMusic AudioSource is not assigned!"); return; }


        // Load the saved state from AudioData asset
        musicSlider.value = audioData.masterVolume;
        BackgroundMusic.volume = audioData.masterVolume;

        // Listen for slider movement
        musicSlider.onValueChanged.AddListener(SetVolume);
    }


    //Sets Volume of Playing Audio
    public void SetVolume(float value)
    {
        BackgroundMusic.volume = value;
        audioData.masterVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}