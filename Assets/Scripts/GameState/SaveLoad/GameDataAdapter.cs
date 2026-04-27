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
                totalCoins = data.TotalCoins,
                
                currentLevel = data.CurrentLevel,
                currentCheckpoint =  data.CurrentCheckpoint,
                
                completedLevelData = ToLevelDataListDTO(data.CompletedLevelData),
                
                // only serialize currentLevelData if CurrentLevel is valid
                currentLevelData = 
                    string.IsNullOrEmpty(data.CurrentLevel)
                    ? null
                    : ToLevelDataDTO(data.CurrentLevelData, data.CurrentLevel),
                
                beatGame = data.BeatGame,
            };
        }

        public static GameData FromDTO(GameDataDTO dto)
        {
            GameData data = new(dto.saveName)
            {
                PlayerHealth = dto.playerHealth,
                MaxHealth = dto.maxHealth,
                
                TotalDeaths = dto.totalDeaths,
                TotalCoins = dto.totalCoins,
                
                CurrentLevel = dto.currentLevel,
                CurrentCheckpoint = dto.currentCheckpoint,
                
                CompletedLevelData = FromLevelDataListDTO(dto.completedLevelData),
                CurrentLevelData = FromLevelDataDTO(dto.currentLevelData),
                
                BeatGame = dto.beatGame
            };
            return data;
        }

        private static List<LevelDataDTO> ToLevelDataListDTO(Dictionary<string, LevelData> dict)
        {
            List<LevelDataDTO> list = new();

            foreach (KeyValuePair<string, LevelData> kvp in dict)
            {
                list.Add(new LevelDataDTO
                {
                    sceneName = kvp.Key,
                    elapsedTime =  kvp.Value.ElapsedTime,
                    coins = kvp.Value.Coins,
                    deaths = kvp.Value.Deaths,
                    maxCoins = kvp.Value.MaxCoins,
                });
            }
            return list;
        }

        private static Dictionary<string, LevelData> FromLevelDataListDTO(List<LevelDataDTO> list)
        {
            Dictionary<string, LevelData> dict = new();

            foreach (LevelDataDTO item in list)
            {
                dict[item.sceneName] = new LevelData(
                    item.elapsedTime,
                    item.coins,
                    item.deaths,
                    item.maxCoins
                );
            }
            return dict;
        }

        private static LevelDataDTO ToLevelDataDTO(LevelData data, string sceneName)
        {
            if (data == null)
            {
                return null;
            }

            return new LevelDataDTO
            {
                sceneName = sceneName,
                elapsedTime = data.ElapsedTime,
                coins = data.Coins,
                deaths = data.Deaths,
                maxCoins = data.MaxCoins
            };
        }

        private static LevelData FromLevelDataDTO(LevelDataDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new LevelData(
                dto.elapsedTime,
                dto.coins,
                dto.deaths,
                dto.maxCoins
                );
        }
    }
}