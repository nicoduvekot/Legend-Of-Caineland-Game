using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* Controls the Video Options Panel behavior.
 * Handles resolution selection and fullscreen toggle.
 * Made by: Yoko Parks
 * Last Update: 04/26/26
 */
public class VideoOptPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    //[SerializeField] private Toggle fullscreenToggle;

    private Resolution[] availableResolutions;

    private void OnEnable()
    {
        PopulateResolutionDropdown();
    }

    private void PopulateResolutionDropdown()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
            options.Add(option);

            // Pre-select whichever resolution matches the current screen
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Wire up the listener
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        //fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void OnResolutionChanged(int index)
    {
        Resolution selected = availableResolutions[index];
        Screen.SetResolution(selected.width, selected.height, Screen.fullScreen);
        Debug.Log($"Resolution set to: {selected.width} x {selected.height}");

        // Log before and after to confirm the change registered
        Debug.Log($"Requested: {selected.width} x {selected.height}");
        Debug.Log($"Actual Screen: {Screen.width} x {Screen.height}");
        Debug.Log($"Current Resolution: {Screen.currentResolution.width} x {Screen.currentResolution.height}");

    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"Fullscreen set to: {isFullscreen}");
    }

    private void OnDisable()
    {
        // Clean up listeners when panel closes
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        //fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }
}