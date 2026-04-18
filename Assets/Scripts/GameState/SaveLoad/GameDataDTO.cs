using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace GameState.SaveLoad
{
    /// <summary>
    /// Data Transfer Object : Serialization Layer
    ///
    /// Represents GameData in a "serialization-friendly" shape
    /// </summary>
    [Serializable]
    public class GameDataDTO
    {
        public string saveName;

        public int playerHealth;
        public int maxHealth;

        public int totalDeaths;
        
        public string currentLevel;
        public string currentPlayer;

        public int currentCheckpoint;
        public int totalCoins;

        public List<string> levelsUnlocked;
        public List<string> levelsCompleted;

        public List<LevelDataDTO> levelStats;
    }
}