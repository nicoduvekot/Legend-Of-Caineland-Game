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
        public int totalCoins;
        
        public string currentLevel;
        public int currentCheckpoint;

        public List<LevelDataDTO> completedLevelData;
        public LevelDataDTO currentLevelData;
        
        public bool beatGame;
    }
}