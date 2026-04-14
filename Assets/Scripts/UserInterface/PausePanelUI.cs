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
 * Modified: 04/13/26
 */
public class PausePanelUI : MonoBehaviour
{
    private OptionMenuController optionMenuController; //Call upon the OptionsController to Change Settings

    //Serialize the buttons 
    [SerializeField] private Button saveButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject audioPanel;

    //Code executed on start
    private void Start()
    {
        gameObject.SetActive(false); //Mk pause not visible by default

        //Assign behavior to alias code
        PauseManager.OnPaused += ShowPanel;
        PauseManager.OnUnpaused += HidePanel;
    }



    //Executed on close
    private void OnDestroy() //Hide panel on scene destroy
    {
        PauseManager.OnPaused -= ShowPanel;
        PauseManager.OnUnpaused -= HidePanel;
    }



    public void Save()
    {

        // Show current slots 
        // Ask player if they want to overwrite selected save 

        // if yes:
        // call save controller & write to save using playerID, level, unlocks, etc
        // declare complete

        //if no:
        //cancel operation and go back to universal pause

        Debug.Log("Save Button Activated in Universal Pause!");


    }



    public void OptionsPopup()
    {
        //invoke slider, video panel, && controls here for adjustment 
        Debug.Log("Options Activated in Universal Pause!");


        //Mk new slider using existing audioData & volumeSlider code
        //Refer to main menu options for a breakdown
        

    }



    public void QuitToMenu()
    {
        //Ask if sure popup dialog
        //if yes:

        Debug.Log("Quit Activated In Universal Pause!");

        Time.timeScale = 1f; // Unpause before loading
        SceneManager.LoadScene("Menu");
    }

        //if no:
        //go back a step 
  



// Aliases to make code exec easier to read

    private void ShowPanel()
    {
        gameObject.SetActive(true);
    }



    private void HidePanel()
    {
        gameObject.SetActive(false);
    }


}