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
                
                totalDeaths = data.TotalDeaths,
                
                currentLevel = data.CurrentLevel,
                currentPlayer =  data.CurrentPlayer.Value,
                
                currentCheckpoint =  data.CurrentCheckpoint,
                totalCoins = data.TotalCoins,
                
                levelsUnlocked = data.LevelsUnlocked.ToList(),
                levelsCompleted = data.LevelsCompleted.ToList(),
                
                levelStats = ToLevelStatsDTO(data.LevelStats)
            };
        }

        public static GameData FromDTO(GameDataDTO dto)
        {
            GameData data = new(dto.saveName)
            {
                PlayerHealth = dto.playerHealth,
                MaxHealth = dto.maxHealth,
                
                TotalDeaths = dto.totalDeaths,
                
                CurrentLevel = dto.currentLevel,
                CurrentPlayer = new PlayerId(dto.currentPlayer),
                
                CurrentCheckpoint = dto.currentCheckpoint,
                TotalCoins = dto.totalCoins,
                
                LevelsUnlocked = new HashSet<string>(dto.levelsUnlocked),
                LevelsCompleted = new HashSet<string>(dto.levelsCompleted),
                
                LevelStats = FromLevelStatsDTO(dto.levelStats)
            };

            return data;
        }

        private static List<LevelDataDTO> ToLevelStatsDTO(Dictionary<string, LevelData> dict)
        {
            List<LevelDataDTO> list = new();

            foreach (KeyValuePair<string, LevelData> kvp in dict)
            {
                list.Add(new LevelDataDTO
                {
                    sceneName = kvp.Key,
                    time =  kvp.Value.Time,
                    coins = kvp.Value.Coins,
                    deaths = kvp.Value.Deaths,
                });
            }
            return list;
        }

        private static Dictionary<string, LevelData> FromLevelStatsDTO(List<LevelDataDTO> list)
        {
            Dictionary<string, LevelData> dict = new();

            foreach (LevelDataDTO item in list)
            {
                dict[item.sceneName] = new LevelData(
                    item.time,
                    item.coins,
                    item.deaths
                );
            }
            return dict;
        }
    }
}