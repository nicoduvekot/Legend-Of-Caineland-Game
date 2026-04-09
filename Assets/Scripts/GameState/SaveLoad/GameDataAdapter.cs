using System.Collections.Generic;
using System.Linq;
using GameState.Core;

namespace GameState.SaveLoad
{
    /// <summary>
    /// Serialization Bridge
    ///
    /// Converts GameData into the DTO
    /// </summary>
    public static class GameDataAdapter
    {
        public static GameDataDTO ToDTO(GameData data)
        {
            return new GameDataDTO
            {
                saveName = data.SaveName,
                
                playerHealth =  data.PlayerHealth,
                maxHealth =  data.MaxHealth,
                
                currentLevel = data.CurrentLevel.Value,
                currentPlayer =  data.CurrentPlayer.Value,
                
                currentCheckpoint =  data.CurrentCheckpoint,
                coins = data.Coins,
                
                levelsUnlocked = new List<string>(
                    data.LevelsUnlocked.Select(l => l.Value)),
                levelsCompleted = new List<string>(
                    data.LevelsCompleted.Select(l => l.Value)),
            };
        }

        public static GameData FromDTO(GameDataDTO dto)
        {
            GameData data = new(dto.saveName)
            {
                PlayerHealth = dto.playerHealth,
                MaxHealth = dto.maxHealth,
                
                CurrentLevel = new LevelId(dto.currentLevel),
                CurrentPlayer = new PlayerId(dto.currentPlayer),
                
                CurrentCheckpoint = dto.currentCheckpoint,
                Coins = dto.coins,
                
                LevelsUnlocked = new HashSet<LevelId>(
                    dto.levelsUnlocked.Select(id => new LevelId(id))),
                LevelsCompleted = new HashSet<LevelId>(
                    dto.levelsCompleted.Select(id => new LevelId(id))),
            };
            return data;
        }
    }
}