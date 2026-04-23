using UnityEngine;
using Utilities;

namespace GameState.Core
{
    /// <summary>
    /// Runtime State Management Layer
    ///
    /// Responsibilities:
    ///     Provide safe mutation operations for gameplay systems
    ///     Enforce some domain invariants (like maxHealth >= health >= 0)
    ///
    /// Note that mutation here does not mean the data is saved. This mutates the data,
    /// SaveLoadSystem saves the data
    /// </summary>
    public class GameStateManager : PersistentSingleton<GameStateManager>
    {
        public GameData Data { get; private set; }
        
        public string CurrentLevel => Data?.CurrentLevel;
        public int CurrentCheckpoint => Data?.CurrentCheckpoint ?? -1;
        
        public string[] levelOrder = { "Level_01", "Level_02", "Level_03", "Level_04" };
        
        public void SetActiveData(GameData data)
        {
            Data = data;
        }
        
        // GameState mutation operations
        
        public void AddCoin(int amount)
        {
            Data.TotalCoins += amount;
            
            Data.CurrentLevelData.Coins += amount;
        }

        public void TakeDamage(int amount)
        {
            Data.PlayerHealth = Mathf.Max(0, Data.PlayerHealth - amount);
        }

        public void AddDeath()
        {
            Data.TotalDeaths++;
            
            Data.CurrentLevelData.Deaths++;
        }

        public void SetCurrentLevel(string levelId)
        {
            Data.CurrentLevel = levelId;
        }

        public void SetCheckpoint(int checkpointIndex)
        {
            Data.CurrentCheckpoint = checkpointIndex;
        }
        
        // Per level data updates

        public void BeginLevelFresh(string sceneName, int maxCoins)
        {
            if (Data == null)
            {
                Debug.LogWarning("GameState Manager has no active GameData");
                return;
            }

            Data.CurrentLevel = sceneName;
            Data.CurrentCheckpoint = 0; 
            
            Data.CurrentLevelData = new LevelData(
                time: 0f,
                coins : 0,
                deaths : 0,
                maxCoins : maxCoins
            );
        }

        public void ContinueLevelFromLoad(string sceneName)
        {
            if (Data.CurrentLevel != sceneName)
                Debug.LogError($"Scene mismatch! GameData says '{Data.CurrentLevel}' but scene loaded is '{sceneName}'.");

            if (Data.CurrentLevelData == null)
                Debug.LogError($"No saved LevelData found for scene '{sceneName}'. Cannot resume.");
        }

        public void SetCurrentLevelElapsedTime(float time)
        {
            Data.CurrentLevelData.ElapsedTime = time;
        }

        public void CompleteCurrentLevel()
        {
            Data.CompletedLevelData[Data.CurrentLevel] = Data.CurrentLevelData;
            Data.CurrentLevelData = null;
        }
    }
}