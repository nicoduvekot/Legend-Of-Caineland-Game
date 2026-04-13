using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioSource BackgroundMusic;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private AudioData audioData;

    void Start()
    {
        // Safety Check: This will tell you exactly which one is missing in the Console
        if (musicSlider == null) { Debug.LogError("Music Slider is not assigned in the Inspector!"); return; }
        if (audioData == null) { Debug.LogError("AudioData asset is not assigned in the Inspector!"); return; }
        if (BackgroundMusic == null) { Debug.LogError("BackgroundMusic AudioSource is not assigned!"); return; }

        // Load the saved state from your AudioData asset
        musicSlider.value = audioData.masterVolume;
        BackgroundMusic.volume = audioData.masterVolume;

        // Listen for slider movement
        musicSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        BackgroundMusic.volume = value;
        audioData.masterVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}