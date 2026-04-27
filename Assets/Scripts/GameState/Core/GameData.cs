using System.Collections.Generic;

namespace GameState.Core
{
    /// <summary>
    /// Authoritative Runtime Domain Layer
    ///
    /// Responsibilities:
    ///     -Store the current game session's data
    /// </summary>
    public class GameData
    {
        public string SaveName { get; }
        
        public int PlayerHealth { get; internal set; }
        public int MaxHealth { get; internal set; }
        
        public int TotalDeaths { get; internal set; }
        public int TotalCoins { get; internal set; }
        
        public string CurrentLevel { get; internal set; }
        public int CurrentCheckpoint { get; internal set; }
        
        public Dictionary<string, LevelData> CompletedLevelData { get; internal set; }
        public LevelData CurrentLevelData { get; internal set; }
        public bool BeatGame { get; internal set; }

        // constructor for a new GameData save
        
        public GameData(string saveName)
        {
            SaveName = saveName;
            
            // default values for a new save are set upon creating a new save
            PlayerHealth = 3;
            MaxHealth = 5;
            
            TotalDeaths = 0;
            TotalCoins = 0;
            
            CurrentLevel = null;
            CurrentCheckpoint = -1;
            
            CompletedLevelData = new Dictionary<string, LevelData>();
            CurrentLevelData = null;
            
            BeatGame = false;
        }
    }
}