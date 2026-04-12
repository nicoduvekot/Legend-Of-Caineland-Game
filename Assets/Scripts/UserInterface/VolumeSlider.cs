using UnityEngine;
using UnityEngine.UI;


public class VolumeSlider : MonoBehaviour
{

    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private Slider slider;


    //Initialized code for the slider to save last state
    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        slider.value = savedVolume;
        backgroundMusic.volume = savedVolume;

        slider.onValueChanged.AddListener(SetVolume);
    }

    //Changes volume with interaction
    public void SetVolume(float value)
    {
        backgroundMusic.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}