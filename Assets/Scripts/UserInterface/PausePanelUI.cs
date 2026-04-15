using PlayerMovementSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/*
 * This code manages the behavior of The Universal Pause Menu 
 * Calls upon OptionMenuController to save rewrites and wasteful calls 
 * Utilizes MenuController & OptionsController as depends 
 * 
 * Made by: Yoko Parks
 * Modified: 04/15/26
 */
public class PausePanelUI : MonoBehaviour
{
    private OptionMenuController optionMenuController; //Inherits optionMenuController Functions

    [Header("PauseUIPanel")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Options Subpanel")]
    [SerializeField] private GameObject OptionsPanel;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button videoButton;

    [Header("Video")]
    // SERIALIZE VIDEO DATA HERE!

    [Header("Audio")]
    [SerializeField] private GameObject AudioOptPanel;  // the slider GameObject — assign in Inspector
    [SerializeField] private AudioData audioData;

 

    //Initialized on Interface Start
    private void Start()
    {
        gameObject.SetActive(false);

        PauseManager.OnPaused += ShowPanel;
        PauseManager.OnUnpaused += HidePanel;

        // Hard hide both subpanels on start — mirrors OptionMenuController behaviour
        if (OptionsPanel != null) OptionsPanel.SetActive(false);
        if (AudioOptPanel != null) AudioOptPanel.SetActive(false);

        // Listen for button clicks
        if (audioButton != null) audioButton.onClick.AddListener(OnAudioSelected);
        if (videoButton != null) videoButton.onClick.AddListener(OnVideoSelected);

        // Sync slider value to saved audioData on load
        SyncSlider();
    }



    private void OnDestroy()
    {
        PauseManager.OnPaused -= ShowPanel;
        PauseManager.OnUnpaused -= HidePanel;

        if (audioButton != null) audioButton.onClick.RemoveListener(OnAudioSelected);
        if (videoButton != null) videoButton.onClick.RemoveListener(OnVideoSelected);
    }


    //TODO: FINISH THIS METHOD
    public void Save()
    {
        Debug.Log("Save Button Activated in Universal Pause!");
        //MK SAVE SUBPANEL
    }



    // Called by the Options button in the main pause panel
    public void OptionsPopup()
    {
        if (OptionsPanel == null) return;

        bool opening = !OptionsPanel.activeSelf;
        OptionsPanel.SetActive(opening);


        // Hide main pause buttons while options is open, restore when closed
        saveButton.gameObject.SetActive(!opening);
        optionsButton.gameObject.SetActive(!opening);
        quitButton.gameObject.SetActive(!opening);


        // Close audio panel if options subpanel is closing — mirrors BackButton in OptionMenuController
        if (!opening && AudioOptPanel != null)
            AudioOptPanel.SetActive(false);

        Debug.Log("Options subpanel " + (opening ? "opened" : "closed"));
    }



    // --- Options Subpanel Handlers ---

    // Mirrors AudioSettings() in OptionMenuController exactly
    private void OnAudioSelected()
    {
        Debug.Log("Audio Button Activated!");

        if (AudioOptPanel != null)
        {
            AudioOptPanel.SetActive(!AudioOptPanel.activeSelf);
        }

        if (audioData != null)
        {
            SyncSlider();
            Debug.Log("Current saved volume: " + audioData.masterVolume);
        }
    }



    //TODO: FINISH THIS IMPLEMENT
    private void OnVideoSelected()
    {
        Debug.Log("Video settings selected.");
        // TODO: serialize videoPanel and show it here same as audio
    }



    // Finds the slider inside AudioOptPanel and syncs it to saved audioData
    private void SyncSlider()
    {
        if (AudioOptPanel == null || audioData == null) return;

        Slider slider = AudioOptPanel.GetComponentInChildren<Slider>();
        if (slider == null) return;

        slider.onValueChanged.RemoveAllListeners();
        slider.value = audioData.masterVolume;
        slider.onValueChanged.AddListener(OnVolumeChanged);
    }



    // Fires every time the slider moves — writes back to AudioData immediately
    private void OnVolumeChanged(float value)
    {
        if (audioData != null)
        {
            audioData.masterVolume = value;
            AudioListener.volume = value;
            Debug.Log("Master volume set to: " + value);
        }
    }



    // Goes Back to Menu Scene
    public void QuitToMenu()
    {
        Debug.Log("Quit Activated In Universal Pause!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }







    // --- Utility Functions to Handle Reset, Back Buttons, etc. ---


    //Button Closes Options SubMenu Without Closing Pause
    public void BackToggle() { 
    
        //Close SubPanels
        OptionsPanel.SetActive(false);
        AudioOptPanel.SetActive(false);

        //Toggle pause buttons 
        saveButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        //Add Video SetActive after implement

    }



    //Panel Toggles to keep persistence
    private void ShowPanel() => gameObject.SetActive(true); //Toggles Pause Menu Vis
    private void HidePanel()
    {
        // Reset everything back to main pause state on every close
        if (OptionsPanel != null) OptionsPanel.SetActive(false);
        if (AudioOptPanel != null) AudioOptPanel.SetActive(false);

        saveButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        gameObject.SetActive(false);
    }
}