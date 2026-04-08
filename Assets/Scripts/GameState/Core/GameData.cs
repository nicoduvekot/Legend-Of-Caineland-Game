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
        
        public LevelId CurrentLevel { get; internal set; }
        public PlayerId CurrentPlayer { get; internal set; }

        public int CurrentCheckpoint { get; internal set; }
        public int Coins { get; internal set; }

        public HashSet<LevelId> LevelsUnlocked { get; internal set; } = new();
        public HashSet<LevelId> LevelsCompleted { get; internal set; } = new();

        // constructor
        
        public GameData(string saveName)
        {
            SaveName = saveName;
        }
    }
}