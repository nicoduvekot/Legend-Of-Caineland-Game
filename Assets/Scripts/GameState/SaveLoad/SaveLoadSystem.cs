using System.Collections.Generic;
using System.Linq;
using GameState.Core;
using UnityEngine;
using Utilities;

namespace GameState.SaveLoad
{
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        private IDataService _dataService;
        public IDataService GetDataService() => _dataService;
        
        protected override void Awake()
        {
            base.Awake();
            _dataService = new FileDataService(new JsonSerializer());
        }
        
        /// <summary>
        /// Saves the current active GameData
        /// Call after making changes you want to persist
        /// </summary>
        public void SaveGame()
        {
            GameData domainData = GameStateManager.Instance.Data;
            
            GameDataDTO dto = GameDataAdapter.ToDTO(domainData);
            
            _dataService.Save(dto);
        }
        
        /// <summary>
        /// Loads the save with the given name
        /// Use <see cref="GetSaveNames"/> to retrieve valid save names
        /// </summary>
        public void LoadGame(string saveName)
        { 
            GameDataDTO dto;

            try
            {
                dto = _dataService.Load(saveName);
            }
            catch
            {
                Debug.LogError($"The file '{saveName}' was not found.");
                return;
            }
            
            GameData loadedData = GameDataAdapter.FromDTO(dto);
            
            GameStateManager.Instance.SetActiveData(loadedData);
        }
        
        /// <summary>
        /// Creates a new save using <see cref="GenerateNextSaveName"/>
        /// A new save slot will be created with the default values below
        /// and set as the active save
        /// </summary>
        public void NewGame()
        {
            string saveName = GenerateNextSaveName();

            GameData data = new(saveName);
            
            GameStateManager.Instance.SetActiveData(data);
            
            SaveGame();
            //Debug.Log(Application.persistentDataPath);
        }

        /// <summary>
        /// Returns all available save names.
        /// Use this to choose which save to load or delete.
        /// </summary>
        public IEnumerable<string> GetSaveNames()
        {
            return _dataService.ListSaves();
        }

        /// <summary>
        /// Deletes the save with the given name
        /// Use <see cref="GetSaveNames"/> to select which save to remove
        /// </summary>
        public void DeleteGame(string saveName)
        {
            _dataService.Delete(saveName);
        }
        
        // Helpers

        /// <summary>
        /// Generates the next unused save name (format: save_X)
        /// Used internally when creating a new game
        /// </summary>
        private string GenerateNextSaveName()
        {
            List<string> existing = _dataService.ListSaves().ToList();

            int counter = 1;

            while (true)
            {
                string candidate = $"save_{counter}";
                
                if (!existing.Contains(candidate))
                    return candidate;
                
                counter++;
            }
        }
    }
}