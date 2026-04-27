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
    [SerializeField] private SaveSlotUI[] saveSlots; // Ensure this has exactly 3 elements in Inspector

    public void Start()
    {
        RefreshSaveList();
    }

    public void RefreshSaveList()
    {
        List<string> existingSaves = SaveLoadSystem.Instance.GetSaveNames().ToList();

        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slotNumber = i + 1;
            string slotName = $"save_{slotNumber}";

            if (existingSaves.Contains(slotName))
            {
                GameDataDTO dto = SaveLoadSystem.Instance.GetDataService().Load(slotName);
                saveSlots[i].Setup(dto, slotName, this);
            }
            else
            {
                // Pass null to indicate an empty slot
                saveSlots[i].Setup(null, slotName, this);
            }
        }
    }



    // Logic for clicking an Empty Slot
    public void CreateNewGameInSlot(string slotName)
    {
        GameData data = new(slotName);
        GameStateManager.Instance.SetActiveData(data);
        SaveLoadSystem.Instance.SaveGame();

        // Either refresh UI or jump straight into the game
        RefreshSaveList();
        // GameFlowManager.Instance.LoadGame(slotName); 
    }



    public void LoadSave(string saveName) => GameFlowManager.Instance.LoadGame(saveName);



    public void DeleteSave(string saveName)
    {
        SaveLoadSystem.Instance.DeleteGame(saveName);
        RefreshSaveList();
    }



    public void BackButton()
        {
            Debug.Log("Back Button Activated in Load Menu!");
            SceneManager.LoadScene("menu");
        }
    }