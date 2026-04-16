using System;

namespace GameState
{
    [Serializable]
    public class LevelDataDTO
    {
        public string sceneName;
        public float time;
        public int coins;
        public int deaths;
    }
}