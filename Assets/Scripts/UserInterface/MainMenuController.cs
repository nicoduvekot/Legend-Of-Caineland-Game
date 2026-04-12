using GameState.Core;
using GameState.SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * This script serves to control the main menu system in the game.
 * 
 * The debug log statements tell the console to echo back when the buttons are pressed. This will tell if the code received input
 *
 *
 * Made by: Yoko Parks
 * Last Update: 04/11/2026 
 * 
*/
public class MainMenuController : MonoBehaviour
{
    //[SerializeField] GameObject optionsPanel;

    //Mks new game & save. Then, it proceeds to load lvl 1
    public void NewGame()
    {
        Debug.Log("New Game Button Activated!");

        SaveLoadSystem.Instance.NewGame();
        SceneManager.LoadScene("Level_01");
    }



    //Triggers Scene of the aforementioned name to load 
    public void LoadGame()
    {
      Debug.Log("Load Game Button Activated!");
      
        SceneManager.LoadScene("Load_Save");
    }



    //Loads Options Menu
    public void OpenOptions()
    {
      Debug.Log("Options Button Activated!");
      //optionsPanel.SetActive(true);

       SceneManager.LoadScene("Options");
    }



    //This one is pretty self-explainatory 
    public void QuitGame()

    {
      Debug.Log("Quit Button Activated!");
      Application.Quit();

      //THIS BLOCK ALLOWS THE GAME CODE TO RUN IN TEST MODE DO NOT REMOVE
      #if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
      #endif
    }
}