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
        
        public string CurrentLevel { get; internal set; }
        public PlayerId CurrentPlayer { get; internal set; }

        public int CurrentCheckpoint { get; internal set; }
        public int Coins { get; internal set; }

        public HashSet<string> LevelsUnlocked { get; internal set; } = new();
        public HashSet<string> LevelsCompleted { get; internal set; } = new();

        // string is the scene name associated with the corresponding level data
        public Dictionary<string, LevelData> LevelStats { get; internal set; } = new();

        // constructor
        
        public GameData(string saveName)
        {
            SaveName = saveName;
        }
    }
}