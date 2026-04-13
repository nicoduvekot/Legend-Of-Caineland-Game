using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

/* This script controls behavior of the Options Menu. It includes the back button as part of it's functioning.
 * BackButton behavior may be assigned to a universal script at a later time.  
 * * Made by: Yoko Parks 
 * Last Update: 04/13/26
 */
public class OptionMenuController : MonoBehaviour
{
    // Fields to be serialized
    [SerializeField] private AudioData audioData;
    [SerializeField] private GameObject AudioOptPanel;
   

    // Hard reset at start to ensure the slider isn't visible by default
    private void Start()
    {
        if (AudioOptPanel != null)
        {
            AudioOptPanel.SetActive(false);
        }
    }



    // Video Settings Button
    public void VideoSettings()
    {
        Debug.Log("Video Button Activated!");
        // Future: Toggle Video panel here
    }



    // Audio Settings Button
    public void AudioSettings()
    {
        Debug.Log("Audio Button Activated!");

        if (AudioOptPanel != null)
        {
            // Toggles visibility on click
            AudioOptPanel.SetActive(!AudioOptPanel.activeSelf);
        }

        if (audioData != null)
        {
            Debug.Log("Current saved volume: " + audioData.masterVolume);
        }
    }



    // Control Settings Button 
    public void ControlSettings()
    {
        Debug.Log("Control Button Activated!");
        // Future: Toggle Controls panel here
    }



    // Back Button Behavior 
    public void BackButton()
    {
        Debug.Log("Back Button Activated!");

        // Hide the panel if it's open before leaving
        if (AudioOptPanel != null) AudioOptPanel.SetActive(false);

        SceneManager.LoadScene("Menu");
    }
}