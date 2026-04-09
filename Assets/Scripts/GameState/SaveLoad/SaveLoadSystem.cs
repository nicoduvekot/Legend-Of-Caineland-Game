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
        
        public void SaveGame()
        {
            GameData domainData = GameStateManager.Instance.Data;
            
            GameDataDTO dto = GameDataAdapter.ToDTO(domainData);
            
            _dataService.Save(dto);
        }
        
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
        
        public void NewGame()
        {
            string saveName = GenerateNextSaveName();
            
            GameData data = new(saveName)
            {
                PlayerHealth = 3,
                MaxHealth = 5,
                Coins = 0,
                CurrentCheckpoint = 0,
                
                CurrentLevel = LevelId.New(),
                CurrentPlayer = PlayerId.New(),
            };
            
            //data.LevelsUnlocked.Add(data.CurrentLevel);

            GameStateManager.Instance.SetActiveData(data);
            
            SaveGame();
        }
        
        public void DeleteGame(string saveName)
        {
            _dataService.Delete(saveName);
        }
        
        // Helper

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