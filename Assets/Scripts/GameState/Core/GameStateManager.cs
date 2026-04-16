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
    /// </summary>
    public class GameStateManager : PersistentSingleton<GameStateManager>
    {
        public GameData Data { get; private set; }
        
        private float _levelStartTime;
        private int _levelCoins;
        private int _levelDeaths;
        private int _maxCoinsInLevel;
        
        public void SetActiveData(GameData data)
        {
            Data = data;
        }
        
        // GameState mutation operations
        
        public void AddCoin(int amount)
        {
            Data.TotalCoins += amount;
        }

        public void TakeDamage(int amount)
        {
            Data.PlayerHealth = Mathf.Max(0, Data.PlayerHealth - amount);
        }

        public void AddDeath()
        {
            Data.TotalDeaths++;
        }

        public void UnlockLevel(string levelId)
        {
            Data.LevelsUnlocked.Add(levelId);
        }

        public void CompleteLevel(string levelId)
        {
            Data.LevelsCompleted.Add(levelId);
        }

        public void SetCurrentLevel(string levelId)
        {
            Data.CurrentLevel = levelId;
        }

        public void SetCurrentPlayer(PlayerId playerId)
        {
            Data.CurrentPlayer = playerId;
        }

        public void SetCheckpoint(int checkpointIndex)
        {
            Data.CurrentCheckpoint = checkpointIndex;
        }
        
        // Per level data updates

        public void BeginLevel(string sceneName, int maxCoins)
        {
            if (Data == null)
            {
                Debug.LogWarning("GameState Manager is missing Data");
                return;
            }

            Data.CurrentLevel = sceneName;
            
            _levelStartTime = Time.time;
            _levelCoins = 0;
            _levelDeaths = 0;
            _maxCoinsInLevel = maxCoins;
        }
        
        public void AddLevelCoin(int amount)
        {
            _levelCoins += amount;
        }

        public void AddLevelDeath()
        {
            _levelDeaths++;
        }

        public void SaveCurrentLevelProgress()
        {
            if (Data == null)
                return;
            
            string level = Data.CurrentLevel;
            float time = Time.time - _levelStartTime;

            LevelData snapshot = new(
                time: time,
                coins: _levelCoins,
                deaths: _levelDeaths
            );
            
            Data.LevelStats[level] = snapshot;
        }

        public LevelData EndLevel()
        {
            float time = Time.time - _levelStartTime;

            return new LevelData(
                time: time,
                coins: _levelCoins,
                deaths: _levelDeaths
            );
        }
        
        public int GetMaxCoinsInLevel() => _maxCoinsInLevel;

    }
}