using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;



/* This script controls behavior of the Options Menu. It includes the back button as part of it's functioning.
 * BackButton behavior may be assigned to a universal script at a later time.  
 * 
 * 
 * Not sure if I should make new subscenes for these or if I should try to save on scenes in the proj...
 * 
 * 
 * Made by: Yoko Parks 
 * Last Update: 04/11/26
 */
public class OptionMenuController : MonoBehaviour
{
    //Video Settings Button
    public void VideoSettings() {

        Debug.Log("Video Button Activated!");

        //Load Scene with button layout?
        // OR redraw with relevant buttons

    }






    //Audio Settings Button
    public void AudioSettings() {

        Debug.Log("Audio Button Activated!");
    
        //Load Scene with Slider? 
        // OR redraw same scene
    }





    //Control Settings Button 
    public void ControlSettings() {

        Debug.Log("Settings Button Activated!");

        //Load Scene with button layout?
        // OR redraw with relevant buttons
    
    }





    //Back Button Behavior 
    public void BackButton()
    {

        Debug.Log("Back Button Activated!");
        SceneManager.LoadScene("Menu"); //Go back to main menu
    }

}
