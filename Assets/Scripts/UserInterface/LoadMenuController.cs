using Esper.Freeloader;
using GameState;
using GameState.Core;
using GameState.SaveLoad;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenuController : MonoBehaviour
{
    // Assign these in the Inspector — one slot per save entry in your UI
    [SerializeField] private SaveSlotUI[] saveSlots;

    private List<string> _saveNames = new();

       public void Start()
        {
        //SaveLoadSystem.Instance.NewGame();    // <-- TEMP: Create a dummy save for testing. Remove this line when you have actual saves to load.
        RefreshSaveList();
        }
    

    // Call this anytime you need the list to reflect disk (e.g. after a delete)
    public void RefreshSaveList()
    {
        _saveNames = SaveLoadSystem.Instance.GetSaveNames().ToList();

        foreach (SaveSlotUI slot in saveSlots)
            slot.gameObject.SetActive(false);

        for (int i = 0; i < _saveNames.Count && i < saveSlots.Length; i++)
        {
            string name = _saveNames[i];
            GameDataDTO dto = SaveLoadSystem.Instance.GetDataService().Load(name);

        // If found:
            // Display on Load_Save scene -> PlayerId, Lvl, Collectables, etc 
            // SceneManager.Load(LastPlayerScene)
            
            // Nico's Notes: the end of this should call: 
            // GameFlowManager.Instance.LoadGame(saveName);
            // with that saveName being the saveName of the selected save
            saveSlots[i].gameObject.SetActive(true);
            saveSlots[i].Setup(dto, this);
        }
    }

    // Called by each SaveSlotUI button's onClick
    public void LoadSave(string saveName)
    {
        GameFlowManager.Instance.LoadGame(saveName);
    }

    // Called by each SaveSlotUI delete button's onClick
    public void DeleteSave(string saveName)
    {
        //TODO: Ask for confirmation before deleting via Unity Pane

        //Delete save from disk
        SaveLoadSystem.Instance.DeleteGame(saveName);
        RefreshSaveList(); // Redraw list immediately
    }


   


    public void BackButton()
    {
        Debug.Log("Back Button Activated in Load Menu!");
        SceneManager.LoadScene("menu");
    }
}