using System;

namespace GameState
{
    [Serializable]
    public class LevelDataDTO
    {
        public string sceneName;
        public float elapsedTime;
        public int coins;
        public int deaths;
        public int maxCoins;
    }
}