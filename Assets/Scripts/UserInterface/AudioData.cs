using UnityEngine;

// This allows you to right-click in Project -> Create -> Settings -> AudioData
[CreateAssetMenu(fileName = "AudioData", menuName = "Settings/AudioData")]
public class AudioData : ScriptableObject
{
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1.0f;

    public bool isMuted = false;

    // You can add more audio-specific data here later (e.g., SFX vs Music)
}