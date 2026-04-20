using UnityEngine;
using UnityEngine.SceneManagement;



/* This script controls behavior of the Save Menu. It includes the back button as part of it's functioning.
 * BackButton behavior may be assigned to a universal script at a later time.  
 * 
 * 
 * Not sure if I should make new subscenes for these or if I should try to save on scenes in the proj...
 * 
 * 
 * Made by: Yoko Parks 
 * Last Update: 04/13/26
 */
public class LoadMenuController : MonoBehaviour
{
    //Back Button Behavior 
    public void BackButton()
    {

        Debug.Log("Back Button Activated!");
        SceneManager.LoadScene("Menu"); //Go back to main menu
    }



    // Sketched idea for Loading Player Save
    void Update()
    {

        // Search for Save JSONs from the SaveLoad Mechanic 
        

        // If found:
            // Display on Load_Save scene -> PlayerId, Lvl, Collectables, etc 
            // SceneManager.Load(LastPlayerScene)
            
            // Nico's Notes: the end of this should call: 
            // GameFlowManager.Instance.LoadGame(saveName);
            // with that saveName being the saveName of the selected save

        // If !found && SaveExists.false:
            // Print("No saves")


        // Keep repeating until you leave the save menu || save is found 
    }

}
