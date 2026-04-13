using UnityEngine;

public class AudioSync : MonoBehaviour
{
    [SerializeField] private AudioData audioData;
    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (audioData != null && source != null)
        {
            source.volume = audioData.masterVolume;
        }
    }
}